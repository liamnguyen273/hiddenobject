using System;
using UnityEngine;

using Firebase;
using Firebase.Extensions;
using static Amanotes.Core.Internal.Logging;

namespace Amanotes.Core
{
    internal static class FirebaseHook
    {
        internal static DependencyStatus status = DependencyStatus.UnavailableUpdaterequired;
        private static Action<DependencyStatus> onCheckDependenciesCompleteAction;
        private static bool hasChecked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoStart()
        {
            AmaGDK.Hook.Register(CheckAndFixDependencies);
        }

        private static void CheckAndFixDependencies(Action onComplete)
        {
            void DoOnComplete()
            {
                onComplete();
                var tmpAction = onCheckDependenciesCompleteAction;
                onCheckDependenciesCompleteAction = null;
                tmpAction?.Invoke(status);
            }
            
            FirebaseApp.CheckAndFixDependenciesAsync()
                .ContinueWithOnMainThread(task =>
            {
                status = task.Result;
                if (task.Result != DependencyStatus.Available)
                {
                    LogError($"Could not resolve all Firebase dependencies {task.Result}");
                }
                hasChecked = true;
                DoOnComplete();
            });
        }

        internal static void OnCheckDependenciesComplete(Action<DependencyStatus> action)
        {
            if (hasChecked)
            {
                action?.Invoke(status);
                return;
            }
            onCheckDependenciesCompleteAction -= action;
            onCheckDependenciesCompleteAction += action;
        }
    }
}
