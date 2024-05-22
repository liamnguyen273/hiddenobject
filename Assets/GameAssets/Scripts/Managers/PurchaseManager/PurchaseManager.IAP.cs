using com.brg.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common.UI;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace com.tinycastle.StickerBooker
{
    public partial class PurchaseManager : IDetailedStoreListener
    {
        public const string ENVIRONMENT = "production";
        
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        
        private bool _iapInitializing = false;
        
        public bool IAPUsable => _controller != null;

        public SingleProgressItem IAPInitializeProgress { get; protected set; }

        private void InitializeUnityServices()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(ENVIRONMENT);
                UnityServices.InitializeAsync(options).ContinueWith(_ => EndInitialize(true), TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                // An error occurred during services initialization.
                Log.Error(e);
                Log.Info("Unity Services has failed to initialize.");
                EndInitialize(false);
            }
        }

        private void InitializeIAP()
        {
            if (IAPUsable) return;
            
            // TODO: Use catalog
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            var products = _dataManager.GetAllProducts();

            foreach (var (key, entry) in products)
            {
                if (entry.IsIAP)
                {
                    builder.AddProduct(entry.Id, entry.IsConsumable ? ProductType.Consumable : ProductType.NonConsumable);
                }
            }
            
            UnityPurchasing.Initialize(this, builder);

            IAPInitializeProgress = new SingleProgressItem((out bool success) =>
            {
                success = IAPUsable;
                return _iapInitializing;
            }, null, null, Priority);
        }

        /// <summary>
        /// On IAP Initialized
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;

            _iapProductMetadatas = new Dictionary<string, ProductMetadata>();
            foreach (var (id, entry) in _dataManager.GetAllProducts())
            {
                if (!entry.IsIAP) continue;
                
                var prod = _controller.products.WithID(id);

                if (prod == null)
                {
                    Log.Error($"Missing on Unity IAP's product catalog, or wrong client's product entry:" +
                               $"\"{id}\" is marked as IAP, but does not exist in Unity IAP's catalog.");
                    continue;
                }
                
                var metadata = prod.metadata;
                _iapProductMetadatas.Add(id, metadata);
            }

            _iapInitializing = false;
            ProductMetadataAccessibleEvent?.Invoke();
        }

        public void RestorePurchases()
        {
            Log.Info("Restore purchase requested.");
            
            _restorePurchaseRequested = true;
            GM.Instance.WaitHelper.StartWait();
#if UNITY_ANDROID
            _extensions.GetExtension<IGooglePlayStoreExtensions>()
#elif UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>()
#endif
                
#if UNITY_ANDROID || UNITY_IOS
            .RestoreTransactions((result, error) =>
            {
                GM.Instance.WaitHelper.EndWait();
                
                if (result)
                {
                    var popup = GM.Instance.Popups.GetPopup<PopupBehaviourCongrats>(out var behaviour);
                    behaviour.SetContentText("Purchases restored successfully.");
                    popup.Show();
                }
                else
                {
                    Log.Error($"Restore purchase failed. Reason: {error}");
                    var popup = GM.Instance.Popups.GetPopup(PopupNames.ERROR);
                    popup.Show();
                }
            });
#else
            Log.Error("Cannot restore purchase on this platform.");
#endif
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Error("Unity Unity IAP failed to initialize, reason: " + error);
            _iapInitializing = false;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Log.Error("Unity IAP failed to initialize, reason: " + error + ".\n Message: " + message);
            _iapInitializing = false;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Log.Error($"Unity IAP purchase failed for id \"{product.definition.id}\"");
            Log.Error("Failure Description: " + failureDescription);

            if (product.definition.id != _currentPurchaseId)
            {
                CLog.Warn($"Unity IAP product id (\"${product.definition.id}\") on purchase failed " +
                          $"is different than the cached id (\"${_currentPurchaseId}\"), please check.");
            }

            FinalizePurchase(_currentPurchaseId, false);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Log.Error($"Unity IAP purchase failed for id \"{product.definition.id}\"");
            Log.Error("Failure Reason: " + failureReason);

            if (_currentPurchaseId == null)
            {
                if (!_restorePurchaseRequested)
                {
                    CLog.Warn(
                        $"Unity IAP process purchase is called for id (\"${product.definition.id}\")" +
                        $"but it is not invoked by RequestPurchase, this is likely the automatic restore purchase behaviour," +
                        $" since no restore purchase is requested, this will be ignored.");
                    return ;
                }
            }
            else if (product.definition.id != _currentPurchaseId)
            {
                CLog.Warn($"Unity IAP product id (\"${product.definition.id}\") on purchase failed " +
                          $"is different than the cached id (\"${_currentPurchaseId}\")." +
                          $"\nWill resolve as FAILED regardless.");
            }
            
            FinalizePurchase(_currentPurchaseId, false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Log.Info($"Purchase success for id \"{purchaseEvent.purchasedProduct.definition.id}\"");

            if (_currentPurchaseId == null)
            {
                if (!_restorePurchaseRequested)
                {
                    CLog.Warn(
                        $"Unity IAP process purchase is called for id (\"${purchaseEvent.purchasedProduct.definition.id}\")" +
                        $"but it is not invoked by RequestPurchase, this is likely the automatic restore purchase behaviour," +
                        $" since no restore purchase is requested, this will be ignored.");
                    return PurchaseProcessingResult.Complete;
                }
            }
            else if (purchaseEvent.purchasedProduct.definition.id != _currentPurchaseId)
            {
                CLog.Warn($"Unity IAP purchased product id (\"${purchaseEvent.purchasedProduct.definition.id}\")" +
                          $"is different than the cached id (\"${_currentPurchaseId}\")" +
                          $"\nWill resolve purchase with \"${purchaseEvent.purchasedProduct.definition.id}\" and" +
                          $"perform the callback as SUCCESS.");
            }

            FinalizePurchase(purchaseEvent.purchasedProduct.definition.id, true);
            return PurchaseProcessingResult.Complete;
        }
    }
}
