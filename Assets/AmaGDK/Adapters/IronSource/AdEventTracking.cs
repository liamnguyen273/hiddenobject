using Amanotes.Core.Internal;
using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Amanotes.Core.Internal.Logging;

namespace Amanotes.Core
{
    internal static partial class AdEventTracking
    {
        internal const string INTER_REQUEST = "fullads_request";
        internal const string INTER_REQUEST_FAILED = "fullads_request_failed";
        internal const string INTER_REQUEST_SUCCESS = "fullads_request_success";
        internal const string INTER_SHOW_CALLED = "fullads_show_called";
        internal const string INTER_SHOW_READY = "fullads_show_ready";
        internal const string INTER_SHOW_NOTREADY = "fullads_show_notready";
        internal const string INTER_IMPRESSION = "fullads_show";
        internal const string INTER_SHOW_FAILED = "fullads_show_failed";
        internal const string INTER_CLICK = "fullads_click";
        internal const string INTER_CLOSE = "fullads_finish";

        // internal const string REWARD_REQUEST
        // internal const string REWARD_REQUEST_FAILED
        // internal const string REWARD_REQUEST_SUCCESS
        internal const string REWARD_SHOW_CALLED = "videoads_show_called";
        internal const string REWARD_SHOW_READY = "videoads_show_ready"; // about to show
        internal const string REWARD_SHOW_NOTREADY = "videoads_show_notready"; // should show but not ready
        internal const string REWARD_IMPRESSION = "videoads_show";
        internal const string REWARD_SHOW_FAILED = "videoads_show_failed";
        internal const string REWARD_RECEIVED = "videoads_finish";
        // internal const string REWARD_CLOSED
        // internal const string REWARD_CANCEL
        internal const string REWARD_CLICK = "videoads_click";
        
        // AppsFlyer events
        internal const string AF_INTER_DISPLAYED = "af_inters_displayed";
        internal const string AF_REWARD_DISPLAYED = "af_rewarded_displayed";
        
        // Parameter name
        const string PARAM_CONNECTION = "connection";
        const string PARAM_LOCATION = "location";
        const string PARAM_SONG_ACM_ID = "song_acm_id";
        const string PARAM_SONG_NAME = "song_name";
        const string PARAM_SONG_UNLOCK_TYPE = "song_unlock_type";
        const string PARAM_REWARD = "reward";
        const string PARAM_MEDIATION = "mediation";
    }

    internal static partial class AdEventTracking
    {
        internal static bool _hasInited;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoStart()
        {
            if (_hasInited)
            {
                LogWarning($"{nameof(AdEventTracking)} already init!");
                return;
            }
            _hasInited = true;

            Inter_AddListeners();
            Reward_AddListeners();

            IronSourceEvents.onImpressionDataReadyEvent += OnImpressionDataReady;
            AppsFlyerAdRevenue.start();
        }

        private static void LogAdEvent(string adEvent, Dictionary<string, object> customParams = null)
        {
            customParams ??= new Dictionary<string, object>();
            if (!customParams.ContainsKey(PARAM_CONNECTION)) customParams.Add(PARAM_CONNECTION, Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ? "online" : "offline");
            if (!customParams.ContainsKey(PARAM_LOCATION)) customParams.Add(PARAM_LOCATION, AmaGDK.Context.Location);
            if (!customParams.ContainsKey(PARAM_MEDIATION)) customParams.Add(PARAM_MEDIATION, "ironsource");

            // AnalyticsManager.LogAdEvent(adEvent, true, isRewardedVideoEvent, customParams);
            AmaGDK.Analytics.LogEvent(adEvent, customParams)
                .SetAsAccumulated();
        }

        internal static void OnImpressionDataReady(IronSourceImpressionData data)
        {
            if (data == null) return;
            double revenue = data.revenue ?? 0;
            // double lifeTimeRevenue = data.lifetimeRevenue ?? 0;
            // double conversionValue = data.conversionValue ?? 0;
            var impressionData = new Dictionary<string, object>()
            {
                { "ad_platform", "ironSource" },
                { "ad_source", data.adNetwork },
                { "ad_format", data.adUnit },
                { "ad_unit_name", data.instanceName },
                { "currency", "USD" },
                { "value", revenue },
                
                { "ironsource_key", ((AmaGDKConfigAsset)AmaGDK.Config).IronSource.ServiceKey }
            };

            AmaGDK.Analytics.LogEvent("ad_impression", impressionData).LogImmediately();
            AmaGDK.Analytics.LogEvent("ad_impression_ama", impressionData).LogImmediately();
            
            
            // "REWARDED_VIDEO" -> IronSource.AD_UNIT.REWARDED_VIDEO
            // "INTERSTITIAL" -> IronSource.AD_UNIT.INTERSTITIAL
            // "OFFERWALL" -> IronSource.AD_UNIT.OFFERWALL
            // "BANNER" -> IronSource.AD_UNIT.BANNER
            string adUnit = data.adUnit.ToLowerInvariant();
            switch (adUnit)
            {
                case "interstitial":
                    AmaGDK.Analytics.LogEvent("fullads_impression").LogImmediately();
                break;
                
                case "rewarded_video":
                    AmaGDK.Analytics.LogEvent("videoads_impression").LogImmediately();
                break;
            }
            
            if (data.revenue == null) return;
            
            AppsFlyerAdRevenue.logAdRevenue(
                data.adNetwork,
                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
                Convert.ToDouble(revenue),
                "USD",
                new Dictionary<string, string>
                {
                    { "appsflyerid", AppsFlyer.getAppsFlyerId() },
                    { "ama_device_id", AmaGDK.User.AmaDeviceId },
                }
            );
        }
    }

    // 
    // INTERSTITIAL
    //
    internal static partial class AdEventTracking
    {
        // EXTRA
        internal static void Inter_Request()
        {
            LogAdEvent(INTER_REQUEST);
        }
        internal static void Inter_ShowCalled()
        {
            int timeDiff = AmaGDK.Ads.TimeSinceLastInterstitialShowCalled;
            int countInSession = AmaGDK.Ads.InterstitialShowCalledInSession;

            var dic = GetMoreParamForInter();
            dic["time_diff_last_fire"] = timeDiff;
            dic["count_in_session"] = countInSession;
            LogAdEvent(INTER_SHOW_CALLED, dic);
        }
        internal static void Inter_ShowReady()
        {
            LogAdEvent(INTER_SHOW_READY, GetMoreParamForInter());
        }
        internal static void Inter_ShowNotReady(AdShowReadyStatus status)
        {
            var dic = GetMoreParamForInter();
            dic["reason"] = GetReason(status);
            LogAdEvent(INTER_SHOW_NOTREADY, dic);
        }
        
        // EVENT HANDLERS
        static void Inter_AddListeners()
        {
            AmaGDK.Ads.OnInter_Request(Inter_Request);
            AmaGDK.Ads.OnInter_ShowCalled(Inter_ShowCalled);
            AmaGDK.Ads.OnInter_ShowReady(Inter_ShowReady);
            AmaGDK.Ads.OnInter_ShowNotReady(Inter_ShowNotReady);
            
            IronSourceInterstitialEvents.onAdLoadFailedEvent += _ =>
            {
                LogAdEvent(INTER_REQUEST_FAILED);
            };
            IronSourceInterstitialEvents.onAdReadyEvent += _ =>
            {
                LogAdEvent(INTER_REQUEST_SUCCESS);
            };
            IronSourceInterstitialEvents.onAdOpenedEvent += _ =>
            {
                LogAdEvent(INTER_IMPRESSION, GetMoreParamForInter());
                AmaGDK.Analytics.LogEvent(AF_INTER_DISPLAYED).OnlyTo(AmaGDK.AdapterID.APPSFLYER);
            };
            IronSourceInterstitialEvents.onAdShowFailedEvent += (error, adInfo) =>
            {
                LogAdEvent(INTER_SHOW_FAILED, GetMoreParamForInter());
            };
            IronSourceInterstitialEvents.onAdShowSucceededEvent += _ => { }; /* UNRELIABLE */
            IronSourceInterstitialEvents.onAdClickedEvent += _ =>
            {
                LogAdEvent(INTER_CLICK, GetMoreParamForInter());
            };
            IronSourceInterstitialEvents.onAdClosedEvent += _ =>
            {
                LogAdEvent(INTER_CLOSE, GetMoreParamForInter());
            };
        }

        static Dictionary<string, object> GetMoreParamForInter()
        {
            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongACMid))
            {
                dic.Add(PARAM_SONG_ACM_ID, AmaGDK.Context.SongACMid);
            }
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongName))
            {
                dic.Add(PARAM_SONG_NAME, AmaGDK.Context.SongName);
            }
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongUnlockType))
            {
                dic.Add(PARAM_SONG_UNLOCK_TYPE, AmaGDK.Context.SongUnlockType);
            }
            return dic;
        }
    }


    // 
    // REWARDED VIDEO
    //
    internal static partial class AdEventTracking
    {
        // Extra
        internal static void Reward_ShowCalled()
        {
            int timeDiff = AmaGDK.Ads.TimeSinceLastVideoRewardShowCalled;
            int countInSession = AmaGDK.Ads.RewardVideoShowCalledInSession;

            var dic = GetMoreParamForReward();
            dic["time_diff_last_fire"] = timeDiff;
            dic["count_in_session"] = countInSession;
            LogAdEvent(REWARD_SHOW_CALLED, dic);
        }
        
        internal static void Reward_ShowReady()
        {
            LogAdEvent(REWARD_SHOW_READY, GetMoreParamForReward());
        }

        internal static void Reward_ShowNotReady(AdShowReadyStatus status)
        {
            var dic = GetMoreParamForReward();
            dic["reason"] = GetReason(status);
            LogAdEvent(REWARD_SHOW_NOTREADY, dic);
        }
        
        // Event Handler
        internal static void Reward_AddListeners()
        {
            AmaGDK.Ads.OnReward_ShowCalled(Reward_ShowCalled);
            AmaGDK.Ads.OnReward_ShowReady(Reward_ShowReady);
            AmaGDK.Ads.OnReward_ShowNotReady(Reward_ShowNotReady);
            
            IronSourceRewardedVideoEvents.onAdLoadFailedEvent += _ => { }; /* NOT TRACKING */
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += () => { }; /* NOT TRACKING */
            IronSourceRewardedVideoEvents.onAdAvailableEvent += _ => { }; /* NOT TRACKING */
            IronSourceRewardedVideoEvents.onAdReadyEvent += _ => { }; /* NOT TRACKING */
            IronSourceRewardedVideoEvents.onAdClosedEvent += _ => { }; /* NOT TRACKING */

            IronSourceRewardedVideoEvents.onAdOpenedEvent += _ =>
            {
                LogAdEvent(REWARD_IMPRESSION, GetMoreParamForReward());
                AmaGDK.Analytics.LogEvent(AF_REWARD_DISPLAYED).OnlyTo(AmaGDK.AdapterID.APPSFLYER);
            };
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += (error, adInfo) =>
            {
                LogAdEvent(REWARD_SHOW_FAILED, GetMoreParamForReward());
            };
            IronSourceRewardedVideoEvents.onAdClickedEvent += (placement, adInfo) =>
            {
                LogAdEvent(REWARD_CLICK, GetMoreParamForReward());
            };
            IronSourceRewardedVideoEvents.onAdRewardedEvent += (placement, adInfo) =>
            {
                LogAdEvent(REWARD_RECEIVED, GetMoreParamForReward());
            };
        }
        
        static Dictionary<string, object> GetMoreParamForReward()
        {
            var dic = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongACMid))
            {
                dic.Add(PARAM_SONG_ACM_ID, AmaGDK.Context.SongACMid);
            }
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongName))
            {
                dic.Add(PARAM_SONG_NAME, AmaGDK.Context.SongName);
            }
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.SongUnlockType))
            {
                dic.Add(PARAM_SONG_UNLOCK_TYPE, AmaGDK.Context.SongUnlockType);
            }
            if (!string.IsNullOrWhiteSpace(AmaGDK.Context.Reward))
            {
                dic.Add(PARAM_REWARD, AmaGDK.Context.Reward);
            }
            return dic;
        }
    }

    internal static partial class AdEventTracking
    {
        private static readonly Dictionary<AdShowReadyStatus, string> showReadyStatusMap = new Dictionary<AdShowReadyStatus, string>
        {
            { AdShowReadyStatus.None, "" },
            { AdShowReadyStatus.Wifi3GDisabled, "wifi3g_disabled" },
            { AdShowReadyStatus.NoInternet, "no_internet" },
            { AdShowReadyStatus.TimeOut, "timeout" }
        };
        
        internal static string GetReason(AdShowReadyStatus status)
        {
            return showReadyStatusMap[status];
        }
    }
}
