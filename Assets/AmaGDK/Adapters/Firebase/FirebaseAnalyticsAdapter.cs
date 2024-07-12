using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;
using static Amanotes.Core.Internal.Logging;
using Amanotes.Core.Internal;
using Firebase;
using Firebase.Extensions;
using UnityEngine.Serialization;

namespace Amanotes.Core
{
    public partial class AmaGDKConfigAsset
    {
        [SerializeField]
        internal FirebaseAnalyticsAdapter FirebaseAnalytics;
    }

    [Serializable] internal partial class FirebaseAnalyticsAdapter : AnalyticsAdapter
    {
        public const string VERSION = "9.1.0";

        [Tooltip("Will trigger:\n\nFirebaseAnalytics.SetAnalyticsCollectionEnabled()")]
        public bool enableAnalyticsCollection = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoStart()
        {
            Register(nameof(AmaGDKConfigAsset.FirebaseAnalytics), VERSION);
            FirebaseHook.OnCheckDependenciesComplete(ProcessDelayedActions);
        }
        protected override bool SendEventByDefault => true;
        protected override string GetServiceKey() => "";

        protected override ConfigStatus configStatus => ConfigStatus.Ok;
    }
    internal partial class FirebaseAnalyticsAdapter
    {
        private bool IsFirebaseReady => FirebaseHook.status == DependencyStatus.Available;

        protected override void Init(Action<bool> onComplete)
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(enableAnalyticsCollection);
            AmaGDK.SetCallback_OnUserIdUpdated(SetUserId);
            GetAnalyticsInstanceID(id =>
            {
                AmaGDK.User.PseudoId = id;
            });
            
            SetUserId(AmaGDK.User.UserId);
            onComplete(IsFirebaseReady);
        }

        void GetAnalyticsInstanceID(Action<string> appId)
        {
            FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted)
                {
                    LogWarning($"Get Firebase analytics instanceId exception: {task.Exception?.Message}");
                    appId?.Invoke("");
                    return;
                }

                appId?.Invoke(task.Result);
            });
        }
    }

    internal partial class FirebaseAnalyticsAdapter
    {
        private static ConcurrentQueue<Action> delayedActions = new ConcurrentQueue<Action>();

        // UTILS
        Parameter[] CreateParameterList(Dictionary<string, object> dictParams)
        {
            var result = new Parameter[dictParams.Count];
            var i = 0;
            foreach (KeyValuePair<string, object> pair in dictParams)
            {
                if (pair.Value == null)
                {
                    LogWarning($"Parameter {pair.Key} is null");
                    continue;
                }

                var paramKey = pair.Key;

                switch (pair.Value)
                {
                    case long longValue:
                        result[i] = new Parameter(paramKey, longValue);
                        break;

                    case float floatValue:
                        result[i] = new Parameter(paramKey, floatValue);
                        break;

                    case double doubleValue:
                        result[i] = new Parameter(paramKey, doubleValue);
                        break;

                    default:
                        result[i] = new Parameter(paramKey, pair.Value.ToString());
                        break;
                }

                i++;
            }

            return result;
        }

        private void InternalLogEvent(string eventNameToSend, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count < 1)
            {
                FirebaseAnalytics.LogEvent(eventNameToSend);
            } else
            {
                FirebaseAnalytics.LogEvent(eventNameToSend, CreateParameterList(parameters));
            }
        }
        
        private static void ProcessDelayedActions(DependencyStatus status)
        {
            if (status != DependencyStatus.Available) return;
            if (delayedActions.Count <= 0) return;
            while (delayedActions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    LogWarning($"[FirebaseAnalytics] Error: {e.Message}");
                }
                
            }
        }

        void SetUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            
            if (IsFirebaseReady)
            {
                FirebaseAnalytics.SetUserId(userId);
            }
            else
            {
                delayedActions.Enqueue(() => FirebaseAnalytics.SetUserId(userId));
            }
        }

        public override void SetUserProperty(string key, string value)
        {
            if (IsFirebaseReady)
            {
                FirebaseAnalytics.SetUserProperty(key, value);
            }
            else
            {
                delayedActions.Enqueue(() => FirebaseAnalytics.SetUserProperty(key, value));
            }
        }

        public override void LogEvent(string eventNameToSend, Dictionary<string, object> parameters)
        {
            if (IsFirebaseReady)
            {
                InternalLogEvent(eventNameToSend, parameters);
            }
            else
            {
                delayedActions.Enqueue(() => InternalLogEvent(eventNameToSend, parameters));
            }
        }
    }
}
