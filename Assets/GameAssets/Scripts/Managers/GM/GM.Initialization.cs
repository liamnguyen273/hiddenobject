using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Common.AnalyticEvents;
using com.brg.Common.Random;
using com.brg.Utilities;
using com.tinycastle.StickerBooker.RemoteConfig;
using JSAM;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class GM
    {
        private bool _primaryLock = false;
        private bool _secondaryLock = false;
        private ProgressItemGroup _groupPrimary;
        private ProgressItemGroup _groupSecondary;
        private ProgressItemGroup _joinProgress;
        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override IProgressItem MakeProgressItem()
        {
            if (_joinProgress == null)
            {
                _groupPrimary = new ProgressItemGroup(_primaryManagers.Select(x => x.GetInitializeProgressItem()));
                _groupSecondary = new ProgressItemGroup(_secondaryManagers.Select(x => x.GetInitializeProgressItem()));
                _joinProgress = new ProgressItemGroup(_groupPrimary, _groupSecondary);
            }

            return _joinProgress;
        }
        
        protected override void StartInitializationBehaviour()
        {
            // _rng = RandomEngineFactory.CreateEngine(Engine.STANDARD, 1779062423);
            _maxDebuggerButton.gameObject.SetActive(_showMaxDebugger);

            _primaryLock = false;
            _secondaryLock = false;
            InitializePrimary();

            MakeProgressItem();            
            
            _loadingScreen.RequestLoad(GetInitializeProgressItem(), () =>
            {
                RequestGoTo(GameScreen.MENU, true);
            });
        }

        private void UpdateInitialization(float dt)
        {
            if (State != InitializationState.INITIALIZING) return;

            if (!_primaryLock && _groupPrimary.Completed)
            {
                _primaryLock = true;
                InitializeSecondary();
            }

            if (!_secondaryLock && _groupSecondary.Completed)
            {
                _secondaryLock = true;
                EndInitialize(_joinProgress.IsSuccess);
            }
        }

        protected override void EndInitializationBehaviour()
        {
            // TODO
            Events.MakeEvent(GameEvents.OPEN_APP)
                .SendEvent();

            var pref = _playerManager.GetPreference();

            AudioManager.MusicVolume = pref.MusicVolume > 0 ? 0.5f : 0f;
            AudioManager.SoundVolume = pref.SfxVolume > 0 ? 0.5f : 0f;

            AttachEvents();
            
            // Theming
            SetTheme(GetTheme());

            if (GetTheme() == GlobalConstants.CHRISTMAS_THEME)
            {
                _snowParticles.Play();
            }
            
            // Now initialize banner ad
            _adManager.InitializeBannerAds();
            
            // Initialize leaderboard
            if (Player.GetLeaderboardShouldInitialize())
            {
                var names = Data.GetLeaderboardNames().OrderBy(x => Rng.GetFloat(-1.0f, 1.0f));
                Player.UpdateLeaderboard("You", 0);

                var score = 20;
                var scoreIncrease = 12;
                var scoreIntensifier = 5;
                foreach (var name in names)
                {
                    Player.UpdateLeaderboard(name, score);
                    scoreIncrease += Rng.GetInteger(scoreIntensifier + 1);
                    score += scoreIncrease;
                }
                
                Player.RequestSaveData(true, false, false);
            }
            else
            {
                UpdateLeaderboardAfterLaunch();
                Player.RequestSaveData(true, false, false);
            }
        }

        private void InitializePrimary()
        {
            _primaryManagers.ForEach(x => x.Initialize());
        }

        private void InitializeSecondary()
        {
            _purchaseManager.SetComponents(_dataManager, _playerManager);
            
            _secondaryManagers.ForEach(x => x.Initialize());
        }
    }
}