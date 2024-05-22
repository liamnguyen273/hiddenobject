using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Common.Logging;
using Firebase;
using Firebase.RemoteConfig;
using UnityEngine;

namespace com.tinycastle.StickerBooker.RemoteConfig
{
    public static class GameRemoteConfigs
    {
        public const string DEBUG_VALUE = "debug_shhhh";
        public const string INTER_START = "inter_start";
        public const string INTER_CAPPING = "inter_capping";
        public const string AOA_ENABLE = "AOA_enable";
        public const string AOA_START_SESSION = "AOA_start_session";
        public const string REMOVEAD_POPUPFREQUENCY = "removeAd_PopupFrequency";
        public const string REWARD_FREQUENCY = "Reward_Frequency";
        public const string CHECK_INTERNET = "checkInternet";
        public const string GAME_THEME = "gameTheme";

        public static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>
        {
            { DEBUG_VALUE, "Did not get remote config" },
            { INTER_START, 60 },
            { INTER_CAPPING, 60 },
            { AOA_ENABLE, true },
            { AOA_START_SESSION, 1 },
            { REMOVEAD_POPUPFREQUENCY, 10 },
            { REWARD_FREQUENCY, 60 },
            { CHECK_INTERNET, true },
            { GAME_THEME, GlobalConstants.DEFAULT_THEME }
        };
    }
    
    public class RemoteConfigManager: ManagerBase
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        
        protected override void StartInitializationBehaviour()
        {
            var dependency = FirebaseHelper.CheckDependencies();
            dependency.ContinueWith(t1 =>
            {
                Log.Info("Dependencies checked.");
                var available = t1.Result == DependencyStatus.Available;

                if (available)
                {
                    InitializeAsync().ContinueWith(t2 => EndInitialize(true), TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    Log.Error($"Could not resolve all Firebase dependencies: {t1.Result}. Initialization halted.");
                    EndInitialize(false);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void EndInitializationBehaviour()
        {
            // Debug value test
            var debugValue = GetValue(GameRemoteConfigs.DEBUG_VALUE, "Failed");
            LogObj.Default.Info("DEBUG_VALUE is: " + debugValue);
        }

        public string GetValue(string name, string clientDefinedFailedValue)
        {
            if (Usable)
            {
                try
                {
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(name);
                    return configValue.StringValue;
                }
                catch (Exception e)
                {
                    Log.Warn("Getting remote config met an exception, " +
                                        "will return client default.");
                    Log.Warn(e);
                }
            }
            else
            {
                Log.Warn($"Cannot get value when initialization status is ${State}");
            }
    
            return clientDefinedFailedValue;
        }
        
        public long GetValue(string name, long clientDefinedFailedValue)
        {
            if (Usable)
            {
                try
                {
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(name);
                    return configValue.LongValue;
                }
                catch (Exception e)
                {
                    Log.Warn("Getting remote config met an exception, " +
                                        "will return client default.");
                    Log.Warn(e);
                }
                
            }
            else
            {
                Log.Warn($"Cannot get value when initialization status is ${State}");
            }

            return clientDefinedFailedValue;
        }      
        
        public bool GetValue(string name, bool clientDefinedFailedValue)
        {
            if (Usable)
            {
                try
                {
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(name);
                    return configValue.BooleanValue;
                }
                catch (Exception e)
                {
                    Log.Warn("Getting remote config met an exception, " +
                                        "will return client default.");
                    Log.Warn(e);
                }
                
            }
            else
            {
                Log.Warn($"Cannot get value when initialization status is ${State}");
            }

            return clientDefinedFailedValue;
        }   
        
        public double GetValue(string name, double clientDefinedFailedValue)
        {
            if (Usable)
            {
                try
                {
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(name);
                    return configValue.DoubleValue;
                }
                catch (Exception e)
                {
                    Log.Warn("Getting remote config met an exception, " +
                                        "will return client default.");
                    Log.Warn(e);
                }
                
            }
            else
            {
                Log.Warn($"Cannot get value when initialization status is ${State}");
            }

            return clientDefinedFailedValue;
        }

        private async Task<bool> InitializeAsync()
        {
            await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(GameRemoteConfigs.Defaults);
            Log.Info("Set defaults for remote config.");
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(0));
            
            var index = Task.WaitAny(new []{fetchTask}, 8000);

            var activated = false;
            if (index != -1)
            {
                Log.Info("Fetched values for remote config.");
                activated = await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                Log.Info("Tried activation for remote config.");
            }

            if (!activated)
            {
                Log.Warn($"RemoteConfig fetched failed, will use default values");
            }
            else
            {
                Log.Info($"RemoteConfig fetched and activated.");
            }
            
            return activated;
        }
    }
}