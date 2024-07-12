using System;

namespace com.tinycastle.StickerBooker
{
    public partial class AdManager
    {
        private int _interRetryAttempt;

        public void InitializeInterstitialAds()
        {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            Log.Info($"MAX's interstitial load request (attempt {_interRetryAttempt})");
            MaxSdk.LoadInterstitial(INTERSTITIAL_AD_UNIT);
        }
        
        private void ShowInterstitial()
        {
            Log.Warn($"Requested showing MAX's interstitial ad.");
            MaxSdk.ShowInterstitial(INTERSTITIAL_AD_UNIT);
        }
        
        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interRetryAttempt = 0;
            Log.Info($"OnInterstitialLoadedEvent. Attempt: {_interRetryAttempt}");
        }
        
        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interRetryAttempt++;
            Log.Warn($"OnInterstitialLoadFailedEvent. Attempt: {_interRetryAttempt}.");
            Log.Error(errorInfo);
            double retryDelay = Math.Pow(1.5f, Math.Min(6, _interRetryAttempt));
            Invoke(nameof(LoadInterstitial), (float)retryDelay);
        }
        
        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        { 
            Log.Info($"OnInterstitialDisplayedEvent");
            GM.Instance.Events.MakeEvent(GameEvents.INTERSTITIAL_AD_SHOW)
                .SendEvent();
        }
        
        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Log.Error("OnInterstitialAdFailedToDisplayEvent");
            Log.Error(errorInfo);
            OnAdFailedToShow();
            LoadInterstitial();
        }
        
        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        {
            Log.Info("OnInterstitialClickedEvent");
        }
        
        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info("OnInterstitialHiddenEvent");
            
            OnAdCompleted(true);
            LoadInterstitial();
        }
    }
}
