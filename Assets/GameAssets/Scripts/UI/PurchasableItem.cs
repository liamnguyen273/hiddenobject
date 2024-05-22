using System;
using com.brg.Common.Localization;
using com.brg.Common.UI;
using UnityEngine;
using UnityEngine.Purchasing;

namespace com.tinycastle.StickerBooker
{
    public class PurchasableItem : MonoBehaviour
    {
        [SerializeField] private string _itemId = "";
        [SerializeField] private GameObject _showGroup;
        
        [SerializeField] private TextLocalizer _nameText;
        [SerializeField] private TextLocalizer _descText;
        [SerializeField] private TextLocalizer _priceText;
        [SerializeField] private UIButton _buyButton;
        [SerializeField] private GameObject _boughtTag;

        private ProductEntry _cachedEntry;
        
        public string ItemId => _itemId;

        public void OnBuy()
        {
            GM.Instance.Purchases.RequestPurchase(_itemId,
            () =>
                {
                    
                });
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(ItemId))
            {
                if (_showGroup != null) _showGroup.SetActive(false);
                return;
            }
            
#if !UNITY_EDITOR
            _buyButton.Interactable = false;
#endif
            if (GM.Instance.Purchases.Usable)
            {
                SetEntry();
            }
            else
            {
                GM.Instance.Purchases.ProductEntryAccessibleEvent += SetEntry;
            }
        }

        private void OnDisable()
        {
            GM.Instance.Purchases.ProductEntryAccessibleEvent -= SetEntry;
            GM.Instance.Purchases.ProductMetadataAccessibleEvent -= TryGetMetadata;
            _cachedEntry = null;
        }

        private void TryGetMetadata()
        {
            var metadata = GM.Instance.Purchases.GetProductMetaData(ItemId);
            if (metadata == null) return;
            
            _priceText.RawString = metadata.localizedPriceString;
            _buyButton.Interactable = true;
            CheckBoughtTag();
        }

        private void SetEntry()
        {
            GM.Instance.Purchases.ProductEntryAccessibleEvent -= SetEntry;
            
            var entry = GM.Instance.Purchases.GetProductEntry(ItemId);
            
            _cachedEntry = entry;

            if (_boughtTag != null)
            {
                _boughtTag.SetActive(false);
            }
            
            if (entry == null)
            {
                if (_nameText != null) _nameText.RawString = "";
                if (_descText != null) _descText.RawString = "";
                if (_priceText != null) _priceText.RawString = "...";
            }
            else
            {
                if (_nameText != null) _nameText.RawString = entry.ListingName;
                if (_descText != null) _descText.RawString = entry.Description;

                if (!entry.IsIAP)
                {
                    _buyButton.Interactable = true;
                    CheckBoughtTag();
                }
                else
                {
                    if (GM.Instance.Purchases.IAPUsable)
                    {
                        TryGetMetadata();
                    }
                    else
                    {
                        GM.Instance.Purchases.ProductMetadataAccessibleEvent += TryGetMetadata;
                    }
                }
            }
        }

        private void CheckBoughtTag()
        {
            if (_cachedEntry == null || _cachedEntry.IsConsumable) return;

            var owned = GM.Instance.Player.Own(_cachedEntry.Id);
            
            if (_boughtTag != null)
            {
                _boughtTag.SetActive(owned);
            }
            
            _buyButton.Interactable = !owned;
        }
    }
}