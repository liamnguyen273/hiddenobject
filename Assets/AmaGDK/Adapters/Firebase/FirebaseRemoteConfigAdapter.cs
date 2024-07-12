using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using static Amanotes.Core.Internal.Logging;
using Amanotes.Core.Internal;
using UnityEngine.Serialization;

namespace Amanotes.Core
{
    public partial class AmaGDKConfigAsset
    {
        
        [SerializeField]
        [FormerlySerializedAs("firebaseRemoteConfig")]
        internal FirebaseRemoteConfigAdapter FirebaseRemoteConfig;
    }
    
    [Serializable] internal partial class FirebaseRemoteConfigAdapter : RemoteConfigAdapter
    {
        public const string VERSION = "9.1.0";
        
        [Tooltip("Fetch immediately (bypass the minimumFetchIntervalInSeconds)\n\nMake sure to set back to <false> after complete testing")]
        public bool devMode = false;
        
        
        [Tooltip("Minimum fetch interval default = 43200 (12h)\n\nNote: This config will be ignored in Editor")]
        [FormerlySerializedAs("minimumFetchInternalInSeconds")]
        public long minimumFetchIntervalInSeconds = 12 * 60 * 60;
        
        FirebaseRemoteConfig frbRemoteConfig;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoStart()
        {
            Register(nameof(AmaGDKConfigAsset.FirebaseRemoteConfig), VERSION);
        }

        protected override string GetServiceKey() => "";

        protected override ConfigStatus configStatus
        {
            get
            {
                if (devMode) return ConfigStatus.Warning;
                return (minimumFetchIntervalInSeconds <= 60) ? ConfigStatus.Invalid : ConfigStatus.Ok;
            }
        }
    }

    internal partial class FirebaseRemoteConfigAdapter
    {
        protected override void Init(Action<bool> onComplete)
        {
            bool isOk = FirebaseHook.status == DependencyStatus.Available;
            frbRemoteConfig = FirebaseRemoteConfig.DefaultInstance;
            onComplete(isOk);
        }
        
        public override void FetchConfig(bool isFirstTime, Action<bool, Dictionary<string, string>> onComplete)
        {
            ulong fetchInterval = (ulong) minimumFetchIntervalInSeconds * 1000;
            if (devMode || Application.isEditor)
            {
                fetchInterval = 10; // 10 secs
            }
            
            var settings = new ConfigSettings
            {
                FetchTimeoutInMilliseconds = (ulong)(isFirstTime 
                    ? moduleConfig.firstFetchTimeOutInSeconds
                    : moduleConfig.defaultTimeOutInSeconds) * 1000,
                MinimumFetchIntervalInMilliseconds = fetchInterval
            };
            
            var isSuccess = false;
            Dictionary<string, string> dictResult = null;
            
            Task.Run(async () =>
            {
                await frbRemoteConfig.SetConfigSettingsAsync(settings);
                await frbRemoteConfig.FetchAsync();
                
                ConfigInfo info = FirebaseRemoteConfig.DefaultInstance.Info;
                if (info.LastFetchStatus != LastFetchStatus.Success)
                {
                    LogWarning("[Firebase Remote Config] Fetch is canceled or failed");
                    return;
                }
                
                await frbRemoteConfig.ActivateAsync();
                dictResult = new Dictionary<string, string>();
                foreach (string key in frbRemoteConfig.Keys)
                {
                    string value = frbRemoteConfig.GetValue(key).StringValue;
                    dictResult.Add(key, value);
                }
                isSuccess = true;
            }).ContinueWithOnMainThread((f) => onComplete(isSuccess, dictResult));
        }
        
    }
}
