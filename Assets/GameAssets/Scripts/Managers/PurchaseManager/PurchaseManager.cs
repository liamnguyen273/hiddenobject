using com.brg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.Purchasing;
using Random = System.Random;

namespace com.tinycastle.StickerBooker
{
    public partial class PurchaseManager
    {
        private PlayerManager _playerManager;
        private DataManager _dataManager;
        private Dictionary<string, Dictionary<string, ProductEntry>> _itemsByType;

        private Dictionary<string, ProductMetadata> _iapProductMetadatas;
        
        private string _currentPurchaseId = null;
        private Action _successCallback = null;
        private Action _failedCallback = null;

        private bool _restorePurchaseRequested = false;

        public event Action ProductEntryAccessibleEvent;
        public event Action ProductMetadataAccessibleEvent;

        public ProductEntry GetProductEntry(string id)
        {
            if (!Usable) return null;
            var entry = _dataManager.GetProductEntry(id);
            return entry;
        }

        public ProductMetadata GetProductMetaData(string id)
        {
            if (!IAPUsable)
            {
                Log.Warn($"IAP is not ready, getting metadata for \"{id}\" failed, returning null.");
                return null;
            }

            var entry = _dataManager.GetProductEntry(id);
            if (entry == null)
            {
                Log.Warn($"Product \"{id}\" doesn't exist, cannot get product metadata, returning null.");
                return null;
            }

            if (!entry.IsIAP)
            {
                Log.Warn($"Product \"{id}\" isn't an IAP, returning null.");
                return null;
            }
            
            if (!_iapProductMetadatas.TryGetValue(id, out var metadata))
            {
                Log.Error($"Cannot get metadata for product \"{id}\" returning null.");
                return null;
            }

            return metadata;
        }

        public bool RequestPurchase(string productId, Action successAction, Action failedAction = null)
        {
            if (!Usable)
            {
                Log.Warn("Unity Service is not initialized. Returning failed");
                Log.Warn("Please check for service at relevant screen start.");
                var action = failedAction ?? GetDefaultFailedCallback();
                action?.Invoke();
                return false;
            }
            
            if (_currentPurchaseId != null)
            {
                Log.Warn("Currently handling another purchase. Returning failed.");
                var action = failedAction ?? GetDefaultFailedCallback();
                action?.Invoke();
                return false;
            }

            var entry = _dataManager.GetProductEntry(productId);
            
            if (entry == null)
            {
                Log.Warn($"Product with id \"{productId}\" doesn't exist. Returning failed.");
                var action = failedAction ?? GetDefaultFailedCallback();
                action?.Invoke();
                return false;
            }
            
            if (entry.IsIAP)
            {
                if (!IAPUsable)
                {
                    Log.Warn("Unity IAP is not initialized. Returning failed");
                    Log.Warn("Please check for service at relevant screen start.");
                    var action = failedAction ?? GetDefaultFailedCallback();
                    action?.Invoke();
                    return false;
                }

                if (_controller.products.WithID(productId) == null)
                {
                    Log.Warn($"Unity IAP does not contains product id \"{productId}\". Returning failed.");
                    var action = failedAction ?? GetDefaultFailedCallback();
                    action?.Invoke();
                    return false;
                }
            }
            
            Log.Info($"Try purchasing product with id \"{productId}\" (IAP: {entry.IsIAP})." +
                     $"Purchase request is received. Returning true.");

            _currentPurchaseId = entry.Id;
            _successCallback = successAction;
            _failedCallback = failedAction ?? GetDefaultFailedCallback();
            if (entry.IsIAP) HandleIAPPurchase(entry); else HandleNormalPurchase(entry);

            return true;
        }

        private void HandleNormalPurchase(ProductEntry entry)
        {
            var currency = entry.Currency;
            var price = entry.Price;
            var current = _playerManager.GetResource(currency);

            if (currency == "none")
            {
                FinalizePurchase(_currentPurchaseId, true);
                return;
            }
            
            if (current < price)
            {
                Log.Info($"Player has insufficient resource to purchase \"{entry.Id}\"" +
                         $"(needs {price} \"{currency}\", has {price})");
                FinalizePurchase(_currentPurchaseId, false);
                return;
            }

            _playerManager.UseResource(currency, price);
            current = _playerManager.GetResource(currency);
            
            Log.Info($"Player used resource \"{currency}\" to purchase \"{entry.Id}\"" +
                     $"({current} remaining)");
            
            FinalizePurchase(_currentPurchaseId, true);
        }

        private void HandleIAPPurchase(ProductEntry entry)
        {
            if (GM.Instance.IsCheat)
            {
                FinalizePurchase(_currentPurchaseId, true);
                Log.Info("Test IAP.");
                return;
            }
            
            Log.Info($"Unity IAP's handling the purchase request.");
            _controller.InitiatePurchase(_currentPurchaseId);
        }

        private void FinalizePurchase(string id, bool status)
        {
            if (!status)
            {
                Log.Info("Finalizing purchase with status failed.");
                _failedCallback?.Invoke();
            }
            else
            {
                Log.Info("Finalizing purchase with status successful.");
                var entry = _dataManager.GetProductEntry(id);

                var resolutions = entry.GetParsedBuyResolutions();
                var resolutionParams = entry.GetParsedResolutionData();

                for (var i = 0; i < resolutions.Length; ++i)
                {
                    var resolution = resolutions[i];
                    var parameters = i >= resolutionParams.Length ? "" : resolutionParams[i];
                    ResolveBuyResolution(id, resolution, parameters, out var shouldShowCongrats);

                    if (shouldShowCongrats)
                    {
                        GM.Instance.Popups.GetPopup<PopupBehaviourCongrats>().Show();
                    }
                }
                
                GM.Instance.Events.MakeEvent(GameEvents.IAP_PURCHASED)
                    .Add("product_id", id)
                    .SendEvent();
                
                // Call success action
                _successCallback?.Invoke();
            }
            
            ConcludeRequest();
        }

        private void ConcludeRequest()
        {
            _currentPurchaseId = null;
            _successCallback = null;
            _failedCallback = null;
        }

        private Action GetDefaultFailedCallback()
        {
            return () => GM.Instance.Popups.GetPopup(PopupNames.ERROR).Show();
        }

        private void ResolveBuyResolution(string itemId, string resolution, string resolutionParam, out bool shouldShowCongrats)
        {
            Log.Info($"Resolving resolution \"{resolution}\" with params: \"{resolutionParam}\"");

            shouldShowCongrats = true;
            switch (resolution)
            {
                case GlobalConstants.BUY_RESOLUTION_SET_OWN:
                    GM.Instance.Player.SetAsOwn(itemId);
                    break;
                case GlobalConstants.BUY_RESOLUTION_CHEST:
                    var rewards = RollRewards();
                    ResolveAddResources(out shouldShowCongrats, rewards);
                    break;
                case GlobalConstants.BUY_RESOLUTION_ADD_RESOURCE:
                    var tokens = resolutionParam.Split("&", StringSplitOptions.RemoveEmptyEntries);
                    ResolveAddResources(out shouldShowCongrats, tokens);
                    break;
                default:
                    Log.Warn($"Unknown buy resolution: {resolution}. Please check.");
                    break;
            }
        }

        private void ResolveAddResources(out bool shouldShowCongrats, string[] tokens)
        {
            var items = new List<string>();
            var counts = new List<int>();
            foreach (var token in tokens)
            {
                var subTokens = token.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (subTokens.Length != 2)
                {
                    Log.Error($"Cannot resolve add resource \"{token}\"");
                    continue;
                }

                var success = int.TryParse(subTokens[0], out var count);

                if (!success)
                {
                    Log.Error($"Cannot parse count of resource (\"{count}\")");
                    continue;
                }

                var resource = subTokens[1];
                Log.Info($"Will add {count} of \'{resource}\".");

                var leftCount = count;
                        
                if  (leftCount < 10)
                {
                    items.AddRange(Enumerable.Repeat(resource, leftCount));
                    counts.AddRange(Enumerable.Repeat(1, leftCount));
                }
                else
                {
                    while (leftCount > 0)
                    {
                        var amount = Math.Min(10, leftCount);
                        items.Add(resource);
                        counts.Add(amount);
                        leftCount -= amount;
                    }
                }
            }

            shouldShowCongrats = false;
            GM.Instance.ResolveAnimateAddItems(items.ToArray(), counts.ToArray(), true);
        }
        
        private static string[] RollRewards()
        {
            var ranLookups = UnityEngine.Random.Range(2, 4);
            var ranSearch = UnityEngine.Random.Range(2, 4);

            var rewards = new List<string>();
            if (ranLookups > 0)
            {
                rewards.Add($"{ranLookups} {GlobalConstants.POWER_LOOKUP}");
            }     
            
            if (ranSearch > 0)
            {
                rewards.Add($"{ranSearch} {GlobalConstants.POWER_COMPASS}");
            }

            return rewards.ToArray();
        }
    }
}
