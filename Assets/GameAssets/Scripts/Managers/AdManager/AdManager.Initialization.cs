using System;
using com.brg.Common;

namespace com.tinycastle.StickerBooker
{
    public partial class AdManager
    {        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        
        protected override void StartInitializationBehaviour()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;
            
            MaxSdk.SetSdkKey(SDK_KEY);
            MaxSdk.InitializeSdk();
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            GM.Instance.Player.OnOwnEvent += OnOwn;
        }

        public void ManuallyInitializeBannerAds()
        {
            InitializeBannerAds();
        }
        
        private void OnMaxInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();
            EndInitialize(true);
        }

        private void OnOwn(string id)
        {
            if (id == GlobalConstants.NO_AD_ITEM_NAME)
            {
                RequestHideBanner();
            }
        }
    }
}