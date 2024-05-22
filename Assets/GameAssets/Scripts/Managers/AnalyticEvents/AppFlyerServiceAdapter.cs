using com.brg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.AnalyticEvents;
using com.brg.Common.Logging;

namespace com.tinycastle.StickerBooker
{

    public class AppFlyerServiceAdapter : IAnalyticsServiceAdapter
    {
        private static readonly string DevKey = "du47t3cBGyBVPvN8ext74R";

        private InitializationState _state = InitializationState.NOT_INITIALIZED;
        public InitializationState State => _state;
        public bool Usable => State == InitializationState.SUCCESSFUL;
        public ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;

        public event Action OnInitializationSuccessfulEvent;
        public event Action OnInitializationFailedEvent;
        
        public IProgressItem GetInitializeProgressItem()
        {
            return new SingleProgressItem((out bool success) =>
            {
                success = State == InitializationState.SUCCESSFUL;
                return State > InitializationState.INITIALIZING;
            },
                null,
                null,
                10);
        }

        public void Initialize()
        {
            // try
            // {
            //     AppsFlyer.initSDK(DevKey, null);
            //     AppsFlyer.startSDK();
            //     AppsFlyerAdRevenue.start();
            //     
            //     AppsFlyer.setIsDebug(true);
            //     AppsFlyerAdRevenue.setIsDebug(true);
            //     
            // }
            // catch
            // {
            //     _state = InitializationState.FAILED;
            //     OnInitializationFailedEvent?.Invoke();
            //     LogObj.Default.Info("FirebaseServiceAdapter", $"Failed to initialize.");
            //     return;
            // }
            
            _state = InitializationState.SUCCESSFUL;
            LogObj.Default.Info("FirebaseServiceAdapter", $"Initialize successfully.");
            OnInitializationSuccessfulEvent?.Invoke();
        }
        
        public void SendEvent(AnalyticsEventBuilder eventBuilder)
        {
            // if (TranslateGameEventName(eventBuilder.Name, out var name))
            // {
            //     var parametersPreParse = eventBuilder.Parameters.ToDictionary(p => p.name, p => p.value);
            //     var parameters = parametersPreParse.ToDictionary(x => x.Key, x => x.Value.ToString());
            //     
            //     if (eventBuilder.Name != GameEvents.AD_IMPRESSION_OR_REVENUE)
            //     {
            //         AppsFlyer.sendEvent(name, parameters);
            //         LogObj.Default.Info("AppFlyerServiceAdapter", $"Logged event: {eventBuilder}");
            //     }
            //     else if (eventBuilder.Name == GameEvents.AD_IMPRESSION_OR_REVENUE
            //         && parametersPreParse.ContainsKey("ad_source")
            //         && parametersPreParse.ContainsKey("value"))
            //     {
            //         try
            //         {
            //             var source = (string)parametersPreParse["ad_source"];
            //             var revenue = (double)parametersPreParse["value"];
            //         
            //             LogObj.Default.Info($"Will send to revenue connector (Network: {source}, Revenue: {revenue}).");
            //             AppsFlyerAdRevenue.logAdRevenue(
            //                 source,
            //                 AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
            //                 revenue,
            //                 "USD",
            //                 parameters);
            //         }
            //         catch (Exception e)
            //         {
            //             LogObj.Default.Error(e);
            //             LogObj.Default.Warn("Revenue connector sent failed.");
            //         }
            //     }
            //     else
            //     {
            //         LogObj.Default.Warn($"There is no flow that handles {GameEvents.AD_IMPRESSION_OR_REVENUE}.");
            //     }
            // }
            // else
            // {
            //     LogObj.Default.Info("AppFlyerServiceAdapter", $"Event not sent. Does not have event {eventBuilder.Name} translation.");
            // }
        }

        public bool TranslateGameEventName(string name, out string translatedName)
        {
            return GameEvents.AppFlyerTranslations.TryGetValue(name, out translatedName);
        }
    }
}
