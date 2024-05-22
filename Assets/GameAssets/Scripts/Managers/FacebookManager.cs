using com.brg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class FacebookManager : MonoManagerBase
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;

        protected override void StartInitializationBehaviour()
        {
            // if (!FB.IsInitialized)
            // {
            //     // Initialize the Facebook SDK
            //     FB.Init(InitCallback, OnHideUnity);
            // }
            // else
            // {
            //     // Already initialized, signal an app activation App Event
            //     FB.ActivateApp();
            // }
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            // Do nothing
        }

        private void InitCallback()
        {
            // if (FB.IsInitialized)
            // {
            //     // Signal an app activation App Event
            //     FB.ActivateApp();
            //     // Continue with Facebook SDK
            //     // ...
            //
            //     EndInitialize(true);
            // }
            // else
            // {
            //     Log.Warn("Failed to Initialize the Facebook SDK");
            //     EndInitialize(false);
            // }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }
    }
}
