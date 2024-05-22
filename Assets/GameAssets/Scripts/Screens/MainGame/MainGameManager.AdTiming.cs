using System;
using com.brg.Utilities;
using com.tinycastle.StickerBooker.RemoteConfig;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager
    {
        // Ad book-keepers
        private bool _masterAdLock = false;
        private float _interTimer = 0;
        private bool _interShowing = false;
        private int _sessionAdCounter = 0;

        private float _adInitialInterDelay = -1;
        private float _adSuccessiveInterDelay = -1;

        private int _allInterCounted = 0;
        private int _popupPerInterCount = 1;

        private void SetMasterAdLock(bool adLock)
        {
            _masterAdLock = adLock;
        }

        private void ResetAdCounter()
        {
            _sessionAdCounter = 0;
        }

        private void FetchTimings()
        {
            float timing1 = (float)GM.Instance.RemoteConfigs.GetValue(GameRemoteConfigs.INTER_START, 60.0);
            float timing2 = (float)GM.Instance.RemoteConfigs.GetValue(GameRemoteConfigs.INTER_CAPPING, 60.0);
            timing1 = Math.Max(10f, timing1);
            timing2 = Math.Max(10f, timing2);
            _adInitialInterDelay = timing1;
            _adSuccessiveInterDelay = timing2;
        }

        private void UpdateAdTimer(float dt)
        {
            if (_masterAdLock) return;
            if (_interShowing) return;
            
            var oldTime = (int)_interTimer;
            _interTimer -= dt;
            //
            // if (oldTime != (int)_interTimer)
            // {
            //     Log.Info("Inter ad time: " + (int)_interTimer);
            // }

            if (!(_interTimer < 0f)) return;
            
            RequestAdDuringGameplay();
        }

        public void RequestGameplayAd()
        {
            if (GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME)) return;
            RequestAdDuringGameplay();
        }

        private void RequestAdDuringGameplay()
        {
            _interShowing = true;
            _sessionAdCounter++;

            var request = new AdRequest(AdManager.TYPE_INTERSTITIAL, ConcludeAd, ConcludeAd);
            GM.Instance.Ad.RequestAd(request);
        }

        private void ConcludeAd()
        {
            _interShowing = false;
            _interTimer = _sessionAdCounter < 1 ? _adInitialInterDelay : _adSuccessiveInterDelay;
            ++_allInterCounted;
            
            Log.Info($"All inter count is: {_allInterCounted}");

            if (!_entry.IsMultiplayer)
            {
                if (_allInterCounted % _popupPerInterCount == 0 && !GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME))
                {
                    GM.Instance.HandleOnAdFreeButton();
                }
            }
            else
            {
                MultiplayerModule.OnHalftimeAdOver();
            }
        }
    }
}