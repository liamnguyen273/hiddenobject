using System;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class AdManager
    {
        private static readonly Color BANNER_COLOR = NumberUtilities.FromHex("#a66a1f");
        private static readonly Color BANNER_COLOR_CHRISTMAS = NumberUtilities.FromHex("#205ca8");

        private bool _hasBanner;
        
        public event Action OnBannerShowEvent;
        public event Action OnBannerHideEvent;

        public bool HasBanner => !GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME) && _hasBanner;

        // public Rect GetBannerLayout()
        // {
        //     return MaxSdk.GetBannerLayout(BANNER_AD_UNIT);
        // }

        public void SetTheme(string themeName)
        {
            // MaxSdk.SetBannerBackgroundColor(BANNER_AD_UNIT, 
            //     GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? BANNER_COLOR_CHRISTMAS : BANNER_COLOR);
        }
        
        public void InitializeBannerAds()
        {
            // // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            // MaxSdk.CreateBanner(BANNER_AD_UNIT, MaxSdkBase.BannerPosition.BottomCenter);
            //
            // // Set background or background color for banners to be fully functional
            // SetTheme(GlobalConstants.DEFAULT_THEME);
            //
            // MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
            // MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
            // MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
            // MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            // MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
            // MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
        }

        public void RequestShowBanner()
        {
            if (GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME)) return;
            
            // MaxSdk.ShowBanner(BANNER_AD_UNIT);
            _hasBanner = true;
            OnBannerShowEvent?.Invoke();
        }

        public void RequestHideBanner()
        {
            // MaxSdk.HideBanner(BANNER_AD_UNIT);
            _hasBanner = false;
            OnBannerHideEvent?.Invoke();
        }

        // private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     _hasBanner = true;
        // }
        //
        // private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        // {
        //     Log.Warn($"Banner ad load failed, reason: {errorInfo.Message}");
        // }
        //
        // private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     Log.Info("Banner ad clicked");
        // }
        //
        // private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     Log.Info("Banner ad expanded");
        // }
        //
        // private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     Log.Info("Banner ad collapsed");
        // }
    }
}