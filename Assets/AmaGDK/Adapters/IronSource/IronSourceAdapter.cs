using UnityEngine;
using Amanotes.Core.Internal;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using static Amanotes.Core.Internal.Logging;

namespace Amanotes.Core
{
    public partial class AmaGDKConfigAsset
    {
        [SerializeField]
        [FormerlySerializedAs("Ironsource")]
        internal IronSourceAdapter IronSource;
    }

    [Serializable] public enum IronSourceBannerSizeEnum
    {
        BANNER,
        LARGE,
        RECTANGLE,
        SMART,
        CUSTOM
    }
    
    [Serializable] internal partial class IronSourceAdapter : AdAdapter, IOnApplicationPause
    {
        public const string VERSION = "7.5.2";
        
        [Header("APP KEY")]
        public string AndroidAppKey;
        public string IOSAppKey;
        
        [Header("CONFIG")]
        [Tooltip("Will trigger:\n\nIronSource.Agent.validateIntegration();")]
        public bool validateIntegration;
        
        [Tooltip("Will trigger:\n\nIronSource.Agent.shouldTrackNetworkState();")]
        public bool shouldTrackNetworkState = true;
        
        [Tooltip("Will trigger:\n\nIronSource.Agent.launchTestSuite();")]
        public bool launchTestSuite;
        
        [Header("BANNER")]
        public IronSourceBannerSizeEnum bannerSize = IronSourceBannerSizeEnum.BANNER;
        public IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM;
        
        [Tooltip("Only applicable when bannerSize == Custom")]
        public Vector2Int customBannerSize;

        public string ServiceKey {
            get
            {
#if UNITY_ANDROID
                return AndroidAppKey;
#elif UNITY_IOS
                return IOSAppKey;
#else
                return string.Empty;
#endif
            }
        }

        protected override string GetServiceKey() => ServiceKey;
        protected override ConfigStatus configStatus
        {
            get
            {
                #if UNITY_ANDROID
                if (string.IsNullOrEmpty(AndroidAppKey)) return ConfigStatus.Invalid;
                #endif

                #if UNITY_IOS
                if (string.IsNullOrEmpty(IOSAppKey)) return ConfigStatus.Invalid;
                #endif
                
                return launchTestSuite ? ConfigStatus.Warning : ConfigStatus.Ok;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoStart()
        {
            Register(nameof(AmaGDKConfigAsset.IronSource), VERSION);
        }
        
        protected override void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                Logging.LogWarning("[IS] userId is null or empty");
                return;
            }
            
            Logging.Log($"Mapping IronSource.setUserID: {userId}");
            IronSource.Agent.setUserId(userId);
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            #if UNITY_EDITOR
            #else
            IronSource.Agent.onApplicationPause(pauseStatus);
            #endif
        }
        
        protected override void Init(Action<bool> onComplete)
        {
            string appKey = GetServiceKey();
            SetUserId(AmaGDK.User.UserId);
            AmaGDK.SetCallback_OnUserIdUpdated(SetUserId);
            
            InitAdQuality(appKey);
            if (launchTestSuite) IronSource.Agent.setMetaData("is_test_suite", "enable");
            IronSourceEvents.onSdkInitializationCompletedEvent += () =>
            {
                onComplete(true);
                if (launchTestSuite) IronSource.Agent.launchTestSuite();
            };
            IronSource.Agent.init(appKey);
            
            if (validateIntegration) IronSource.Agent.validateIntegration();
            IronSource.Agent.shouldTrackNetworkState(shouldTrackNetworkState);
            
            // Banner
            IronSourceBannerEvents.onAdLoadedEvent += OnBannerLoaded;
            IronSourceBannerEvents.onAdLoadFailedEvent += OnBannerLoadFailed;
#if UNITY_EDITOR
            onComplete(true);
#endif
        }

		protected void InitAdQuality(string appKey){
			var adQualityConfig = new ISAdQualityConfig
            {
                LogLevel = ISAdQualityLogLevel.WARNING
            };
            if (!string.IsNullOrWhiteSpace(AmaGDK.User.UserId)) adQualityConfig.UserId = AmaGDK.User.UserId;
            IronSourceAdQuality.Initialize(appKey, adQualityConfig);
		}
    }

    internal class ISInterstitialLogic : AdLogic
    {
        public ISInterstitialLogic()
        {
            if (Application.isEditor) return;

            // Load
            IronSourceInterstitialEvents.onAdReadyEvent += _ => OnAdLoadSuccess();
            IronSourceInterstitialEvents.onAdLoadFailedEvent += (e) =>
            {
                // 508 : Interstitial - the server response does not contain interstitial data
                // 510		Load Fail: Server response failed | Load Fail: Adapters loading failure
                // 1037		Load Fail: Cannot load an  while another  is showing
                Logging.LogWarning("IronSourceInterstitialEvents.onAdLoadFailedEvent()\n" + e);
                OnAdLoadFailed(e.getDescription());
            };

            // Show
            IronSourceInterstitialEvents.onAdOpenedEvent += _ => OnAdOpen();
            IronSourceInterstitialEvents.onAdShowSucceededEvent += _ => OnAdShowSucceeded();
            IronSourceInterstitialEvents.onAdShowFailedEvent += (e, _) =>
            {
                Logging.LogWarning("IronSourceInterstitialEvents.onAdShowFailedEvent()\n" + e);
                OnAdShowFailed();
            };
            IronSourceInterstitialEvents.onAdClickedEvent += _ => OnAdClicked();
            IronSourceInterstitialEvents.onAdClosedEvent += _ => OnAdClosed();
        }

        protected override AdType adType => AdType.Interstitial;
        protected override bool isAdReady => Application.isEditor || IronSource.Agent.isInterstitialReady();

        protected override void RequestAd()
        {
            if (Application.isEditor)
            {
                GDKUtils.DelayCall(Random.Range(0f, 1f), OnAdLoadSuccess);
                return;
            }
            
            IronSource.Agent.loadInterstitial();
        }
        protected override void ShowAd()
        {
            if (Application.isEditor)
            {
                OnAdShowSucceeded();
                OnAdClosed();
                return;
            }
            
			
            IronSource.Agent.showInterstitial();
        }
    }

    internal class ISRewardedVideoLogic : AdLogic
    {
        public ISRewardedVideoLogic()
        {
            if (Application.isEditor) return;

            // Load
            IronSourceRewardedVideoEvents.onAdAvailableEvent += _ => OnAdLoadSuccess();
            
            // Show
            IronSourceRewardedVideoEvents.onAdOpenedEvent += _ => OnAdOpen();
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += (error, adInfo) =>
            {
                Logging.LogWarning("IronSourceRewardedVideoEvents.onAdShowFailedEvent()\n" + error);
                OnAdShowFailed();
            };
            IronSourceRewardedVideoEvents.onAdClickedEvent += (placement, adInfo) => OnAdClicked();
            IronSourceRewardedVideoEvents.onAdClosedEvent += _ => OnAdClosed();
            IronSourceRewardedVideoEvents.onAdRewardedEvent += (placement, adInfo) => OnAdReward();
        }

        protected override AdType adType => AdType.VideoReward;
        protected override bool isAdReady => Application.isEditor || IronSource.Agent.isRewardedVideoAvailable();
                
        protected override void RequestAd()
        {
            if (Application.isEditor)
            {
                GDKUtils.DelayCall(Random.Range(0f, 1f), OnAdLoadSuccess);
            }

            // Do nothing as IRS automatically request video rewards
        }

        protected override void ShowAd()
        {
            if (Application.isEditor)
            {
                // fake result
                bool success = (Random.Range(0f, 1f) <= adConfig.rewardAdShowSuccessRate);
                if (success)
                {
                    OnAdReward();
                }
                else
                {
                    OnAdShowFailed();
                }
                OnAdClosed();
                return;
            }

            string placementName = context.placementName;
            if (string.IsNullOrEmpty(placementName))
            {
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                IronSource.Agent.showRewardedVideo(placementName);
            }
        }
    }

    internal enum BannerStatus
    {
        None,
        Requested,
        Failed,
        Success
    }
    
    internal partial class IronSourceAdapter
    {
        private readonly ISInterstitialLogic _interLogic = new ISInterstitialLogic();
        private readonly ISRewardedVideoLogic _rewardedLogic = new ISRewardedVideoLogic();

        private BannerStatus bannerStatus = BannerStatus.None;
        
        protected override AdLogic interstitial => _interLogic;
        protected override AdLogic rewardVideo => _rewardedLogic;


        private static Dictionary<IronSourceBannerSizeEnum, IronSourceBannerSize> bannerSizeMap = new Dictionary<IronSourceBannerSizeEnum, IronSourceBannerSize>()
        {
            { IronSourceBannerSizeEnum.BANNER, IronSourceBannerSize.BANNER },
            { IronSourceBannerSizeEnum.SMART, IronSourceBannerSize.SMART },
            { IronSourceBannerSizeEnum.LARGE, IronSourceBannerSize.LARGE }
        };
        
        public override void ShowBanner()
        {
            if (Application.isEditor) return;

            if (bannerStatus == BannerStatus.Success)
            {
                IronSource.Agent.displayBanner();
                return;
            }
            
            if (bannerStatus != BannerStatus.Requested) LoadBanner();
        }

        void LoadBanner()
        {
            if (bannerStatus != BannerStatus.Failed && bannerStatus != BannerStatus.None)
            {
                LogWarning($"[Ignored] Invalid banner status <{bannerStatus}>");
                return;
            }
            
            bannerStatus = BannerStatus.Requested;
            IronSource.Agent.loadBanner(bannerSizeMap.TryGetValue(bannerSize, out var size) 
                ? size
                : new IronSourceBannerSize(customBannerSize.x, customBannerSize.y), bannerPosition);
        }

        void OnBannerLoadFailed(IronSourceError error)
        {
            // NOTE: BannerLoadFailed callback can be trigger multiple times
            if (bannerStatus == BannerStatus.Success) return;
            bannerStatus = BannerStatus.Failed;
            LogWarning("OnBannerLoadFailed: " + error);
            
            if (error.getErrorCode() == 615)
            {
                // ironSourceSDK API: No banner configurations found
                // 615 : the server response does not contain banner data
                AmaGDK.Ads.HideBanner();
                return;
            }

            var delay = 1f;
            if (error.getErrorCode() == 606)
            {
                // 606 : Mediation No fill
                delay = 15f;
            }
            
            GDKUtils.DelayCall(delay, () => { if (AmaGDK.Ads.IsBannerShowing) LoadBanner(); });
        }

        void OnBannerLoaded(IronSourceAdInfo info)
        {
            bannerStatus = BannerStatus.Success;
            if (!AmaGDK.Ads.IsBannerShowing) return;
            IronSource.Agent.displayBanner();
        }
        
        public override void HideBanner()
        {
            if (Application.isEditor) return;
            IronSource.Agent.hideBanner();
        }
    }
}
