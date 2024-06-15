using com.brg.Common;
using com.brg.Utilities;
using JSAM;
using System.Collections.Generic;
using com.tinycastle.StickerBooker.RemoteConfig;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager : IActivatable
    {
        [SerializeField] private ScrollSnap _itemBarScroll;
        
        public void SetLevel(string id)
        {
            _currentId = id;
            Log.Info($"Level set to {_currentId}.");
        }

        public void Activate()
        {
            _appearance.SetGOActive(true);
            _stemManager.MuteAll();
            
            FetchTimings();
            ResetAdCounter();
            
            // If first time playing the game, set this
            var firstLevel = _entry.Bundle == GlobalConstants.NORMAL_LEVEL_BUNDLE && _entry.SortOrder == 1;
            var firstPlay = GM.Instance.Player.CheckCompletedWithoutExistence(_entry.Id);

            var shouldLockAd = GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME) || (firstLevel && firstPlay);
            shouldLockAd |= _entry.IsTimeAttack || _entry.IsMultiplayer;
            SetMasterAdLock(shouldLockAd);
            
            // Set ad timer?
            _interTimer = _adInitialInterDelay;
            
            GM.Instance.Player.OnOwnEvent += OnOwn;
            GM.Instance.HasBlockingElementsEvent += OnHasBlockingElements;
            GM.Instance.NoBlockingElementsEvent += OnNoBlockingElements;
            
            // Put scroll to center
            // _contentScroll.horizontalNormalizedPosition = 0f;
            // _contentScroll.verticalNormalizedPosition = 0f;

            PlayThemeMusic();
            Log.Info("Activated.");

            CalculateScrollRange();

            OnNoBanner();
            
            if (!GM.Instance.Ad.HasBanner)
            {
                GM.Instance.Ad.OnBannerShowEvent += OnHasBanner;
            }
            else
            {
                OnHasBanner();
            }

            _popupPerInterCount = (int)GM.Instance.RemoteConfigs.GetValue(GameRemoteConfigs.REMOVEAD_POPUPFREQUENCY, 10);
            
            _itemBarScroll.ChangePage(0);
            
            _stemManager.Initialize();
            StartGame();
        }

        public void Deactivate()
        {
            _stemManager.Deinitialize();
            _appearance.SetGOActive(false);
            GM.Instance.Player.OnOwnEvent -= OnOwn;
            GM.Instance.HasBlockingElementsEvent -= OnHasBlockingElements;
            GM.Instance.NoBlockingElementsEvent -= OnNoBlockingElements;
            
            StopThemeMusic();
            
            GM.Instance.Ad.OnBannerShowEvent -= OnHasBanner;

            GM.Instance.Effects.StopHintParticles();
            GM.Instance.Effects.StopStickingParticles();
            GM.Instance.Effects.StopStickingAnimation();
            
            Log.Info("Deactivated.");
        }
        
        public IProgressItem GetPrepareActivateProgressItem()
        {
            var list = new List<IProgressItem>();
            var progress1 = new SingleProgressItem((out bool success) =>
            {
                success = GameState >= GameState.LOAD_DONE;
                return GameState >= GameState.LOAD_DONE;
            }, null, null, 10);
            
            list.Add(progress1);
            list.Add(_assetHandle.MakeProgressItem(true, true, true));
            return new ProgressItemGroup(list);
        }

        public void PrepareActivate()
        {
            Log.Info("Prepare activation...");
            GameState = GameState.LOADING;
            PerformLevelLoad();
        }
        
        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            // TODO
            return new ImmediateProgressItem();
        }

        public void PrepareDeactivate()
        {
            Log.Info("Prepare deactivation...");
            
            // Remove tutorial, if any
            if (_tutorialDynamicSticker != null)
            {
                _tutorialDynamicSticker.OnDragStart -= OnTutorialStartDrag;
                _tutorialDynamicSticker.OnStickEvent -= OnTutorialSticker;
                _tutorialDynamicSticker.OnStickFailedEvent -= OnTutorialStickerFailed;

                _tutorialDynamicSticker = null;
            }
            
            // Remove dynamic sticker book-keep
            foreach (var sticker in _dynamicStickers)
            {
                sticker.ResetSticker();
            }
            
            // Remove static sticker book-keep
            _allStickers = null;
            _findStickers = null;
            _foundStickers = null;
            _allFindStickers = null;
            // _leftOverStickers = null;
            // _attachedStickers = null;
            
            // Remove power-ups
            ResolvePowerupTimeout();
            
            // Clear map
            _map.CleanUp();
            
            // Release assets
            GM.Instance.Data.ReleaseLevelAsset(_currentId, false, false, true);
            
            // Reset other book-keep
            GameState = GameState.NONE;
            _previousState = GameState.NONE;
            _gameTimer = -1;
            _assetHandle = null;
        }
   
        private void OnOwn(string id)
        {
            if (id == GlobalConstants.NO_AD_ITEM_NAME)
            {
                SetMasterAdLock(true);
                OnNoBanner();
            }
        }

        private GameState _previousState = GameState.NONE;
        private void OnHasBlockingElements()
        {
            if (GameState < GameState.TUTORIAL)
            {
                return;
            }
            
            Log.Info("Has blocking elements, timers stopped.");

            _previousState = GameState;
            GameState = GameState.PAUSED;
        }

        private void OnNoBlockingElements()
        {
            if (_previousState == GameState.NONE) return;

            Log.Info("No blocking elements, timers resume.");
            
            GameState = _previousState;
        }

        private void OnHasBanner()
        {
            _overallRect.offsetMin = new Vector2(_overallRect.offsetMin.x, 200);
            _overallRect.offsetMax = new Vector2(_overallRect.offsetMax.x, 0);
        }

        private void OnNoBanner()
        {
            _overallRect.offsetMin = new Vector2(_overallRect.offsetMin.x, 0);
            _overallRect.offsetMax = new Vector2(_overallRect.offsetMax.x, 0);
        }
        
        
        private void PlayThemeMusic()
        {
            // AudioManager.PlayMusic(GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? LibraryMusic.LevelXmasMusic : LibraryMusic.LevelMusic);
        }
        
        private void StopThemeMusic()
        {
            // AudioManager.StopMusic(GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? LibraryMusic.LevelXmasMusic : LibraryMusic.LevelMusic);
        }
    }
}