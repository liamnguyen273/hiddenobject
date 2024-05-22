using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Logging;
using com.brg.Common.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public enum GameState
    {
        NONE = 0,
        LOADING,
        LOAD_DONE,
        TUTORIAL,
        IN_GAME,
        PAUSED,
        COMPLETING,
        COMPLETED,
        REVIEW,
    }
    
    public partial class MainGameManager : MonoManagerBase
    {
        [Header("Level components")] 
        [SerializeField] private RectTransform _overallRect;
        [SerializeField] private RectTransform _levelRect;
        [SerializeField] private Transform _appearance;
        [SerializeField] private MainGameHud _hud;

        [SerializeField] private StickerMap _map;
        [SerializeField] private Transform[] _itemSlots;
        [SerializeField] private DynamicSticker[] _dynamicStickers;
        [SerializeField] private Transform _completeGroup;
        [SerializeField] private ScrollRect _contentScroll;
        [SerializeField] private Transform _mapParent;
        [SerializeField] private GameObject _replayButton;
        
        [SerializeField] private MainGameTimeAttackModule _timeAttackModule;
        [SerializeField] private MainGameMultiplayerModule _multiplayerModule;

        [Header("Tutorials")]
        [SerializeField] private TutorialCursor _cursor;
        
        // Level book-keepers
        private string _currentId = null;
        private LevelEntry _entry = null;
        private LevelAssetHandle _assetHandle = null;

        private List<StaticSticker> _leftOverStickers;
        private List<StaticSticker> _attachedStickers;
        private List<StaticSticker> _allStickers;
        
        private GameState _state = GameState.NONE;
        private float _gameTimer = -1;
        
        public GameState GameState
        {
            get => _state;
            set
            {
                _state = value;
                if (TimeAttackModule.InUse) TimeAttackModule.OnGameState(_state);
                if (MultiplayerModule.InUse) MultiplayerModule.OnGameState(_state);
            }
        }
        
        public bool Completing => GameState == GameState.COMPLETING;

        private GameState CanonicalState => GameState != GameState.PAUSED ? GameState : _previousState;
        public MainGameTimeAttackModule TimeAttackModule => _timeAttackModule;
        public MainGameMultiplayerModule MultiplayerModule => _multiplayerModule;

        public bool IsPlayingMultiplayer => _state > GameState.LOAD_DONE && _state < GameState.COMPLETING &&
                                            (_entry != null && _entry.IsMultiplayer);
        
        private void Update()
        {
            var dt = Time.deltaTime;

            if (GameState == GameState.IN_GAME)
            {
                UpdateAdTimer(dt);
                UpdateGameTimer(dt);
                UpdatePowerup(dt);

                TimeAttackModule.UpdateTimer(dt);
            }
            
            MultiplayerModule.UpdateTimer(dt);
            
            UpdateLoadAssetPack();
        }

        private void UpdateGameTimer(float dt)
        {
            _gameTimer += dt;
        }

        public void ForcePlayerDropSticker()
        {
            foreach (var sticker in _dynamicStickers)
            {
                if (sticker != null) sticker.ForceDrop();
            }
        }

        public DynamicSticker GetRandomValidPickupableDynamicSticker()
        {
            var list = _dynamicStickers.Where(x => x.HasLink && x.LogicalInteractable && !x.Dragging).ToList();
            return list.Count == 0 ? null : list[GM.Instance.Rng.GetInteger(0, list.Count)];
        }
        
        public void OnStickerStickToTarget(DynamicSticker dynamicSticker, bool byPlayer)
        {
            var index = Array.IndexOf(_dynamicStickers, dynamicSticker);

            if (index > -1)
            {
                if (dynamicSticker.GetLinkedStaticSticker() == null)
                {
                    Log.Warn($"Dynamic sticker index {index} is missing a link but managed to call OnStickerStickerToTarget." +
                             $"Please check. It will disappear as normal.");
                    return;
                }

                var staticSticker = dynamicSticker.GetLinkedStaticSticker();

                staticSticker.SetState(StaticStickerState.TEMP_HIDDEN);
                GM.Instance.Effects.PlayStickingAnimation(staticSticker, () =>
                {
                    // TODO update present bar
                    staticSticker.SetState(StaticStickerState.COLORED);
                    staticSticker.PlayPulseTween();
                });
                    
                // Add to attached
                _attachedStickers.Add(staticSticker);
                    
                _hud.UpdateProgress(_attachedStickers.Count, _allStickers.Count);

                ResolvePowerupOnPlayerStick(dynamicSticker);
                dynamicSticker.ResetSticker();
                
                // Add resource
                if (byPlayer)
                {
                    GM.Instance.Player.AddResource(GlobalConstants.CHEST_PROGRESS_RESOURCE, 1, doNotSave: true);
                }
                
                // Refill
                FillStickerBar();
                SwitchCompassTarget();
                
                // Multiplayer score
                if (_entry.IsMultiplayer)
                {
                    if (byPlayer) MultiplayerModule.AddPlayerScore();
                    else MultiplayerModule.AddOpponentScore();
                }
                
                // Check for endgame
                if (EvaluateCompletedStickers())
                {
                    DOVirtual.DelayedCall(0.5f, () => EndGame(true));
                }
                else
                {
                    SaveProgress();
                }
            }
            else
            {
                Log.Warn("Sticker " + dynamicSticker + " does not exist in the map's bookkeeping.");
            }
        }
        
        public void OnHomeButton()
        {
            if (IsPlayingMultiplayer)
            {
                var popup = GM.Instance.Popups.GetPopup<PopupBehaviourAsk>(out var behaviour);
                behaviour.SetQuestion("Quit level?", "You will lose out on the level rewards!", RequestGoHome, null);
                popup.Show();
            }
            else
            {
                RequestGoHome();
            }
        }
        
        public void RequestGoHome()
        {
            if (Completing)
            {
                Log.Warn("Game ending animation is playing, cannot go home yet.");
                return;
            }

            if (IsPlayingMultiplayer)
            {
                MultiplayerModule.GetStatistics(out var oppoName, out var playerScore, out var opponentScore);
                GM.Instance.UpdateLeaderboardAfterMatch(playerScore, oppoName, opponentScore * 2);
            }
            else
            {
                GM.Instance.UpdateLeaderboardAfterGame();
            }

            GM.Instance.Events.MakeEvent(GameEvents.BACK_LEVEL)
                .Add("level", _currentId)
                .Add("leftover_sticker_number", _leftOverStickers?.Count ?? 0)
                .SendEvent();
            
            // Save progress
            SaveProgress(true);
            
            GM.Instance.RequestGoToMenu();
        }

        public Image GetHinter()
        {
            return _map.GetHinterImage();
        }
        
        public void RequestEndGame()
        {
            EndGame(true);
        }

        public void OnTimeAttackModuleTimerEnd()
        {
            if (EvaluateCompletedStickers())
            {
                EndGame(true);
            }
            else
            {
                EndGame(false);
            }
        }
        
        public void OnMultiplayerModuleTimerEnd()
        {
            if (EvaluateCompletedStickers())
            {
                EndGame(true);
            }
            else
            {
                EndGame(false);
            }
        }

        public bool CheckStickerTarget(Vector3 position, Vector3 targetPosition, float threshold)
        {
            var corners = new Vector3[4];
            _levelRect.GetWorldCorners(corners);

            var bottomLeft = corners[0];
            var topRight = corners[2];

            var isInGroup = bottomLeft.x <= position.x && position.x <= topRight.x
                         && bottomLeft.y <= position.y && position.y <= topRight.y;

            if (!isInGroup) return false;
            
            var dist = Vector2.Distance(position, targetPosition);
            return dist <= threshold;
        }
        
        
        public void OnReplayLevel()
        {
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourAsk>(out var behaviour);
            behaviour.SetQuestion("Replay level?", "Clear the canvas and play again from the start?", () =>
            {
                GM.Instance.Loading.RequestLoad(new ImmediateProgressItem(), () =>
                {
                    // Clear progress
                    GM.Instance.Player.ClearLevelProgress(_currentId);
                    
                    // Restart map
                    _map.Restart();
                
                    // Restart book-keep
                    PopulateStickerBookkeeping(true);
                }, StartGame);
            }, null);
            popup.Show();
        }
        
        private void StartGame()
        {
            _saveProgressMeter = 0;
            
            TimeAttackModule.SetUse(_entry.IsTimeAttack);
            MultiplayerModule.SetUse(_entry.IsMultiplayer, _entry);
            
            if (_entry.IsMultiplayer)
            {
                // Generate random opponent
                MultiplayerModule.SetOpponentRandomly();
            }
            
            if (_leftOverStickers.Count > 0)
            {
                // In game
                EnterPlayMode();
                _replayButton.SetActive(false);
            }
            else
            {
                // Game completed, enter review mode
                EnterReviewMode();
                _replayButton.SetActive(true);
            }
        }

        private void EnterPlayMode()
        {
            _completeGroup.SetGOActive(false);
            
            FillStickerBar(true);

            _hud.SetVisiblePowerupTray(!_entry.IsMultiplayer);

            GM.Instance.Events.MakeEvent(GameEvents.START_LEVEL)
                .Add("level", _currentId)
                .Add("sticker_number", _attachedStickers.Count.ToString())
                .SendEvent();

            if (!GM.Instance.Player.GetTutorialPlayed() && _attachedStickers.Count == 0 && _currentId == "level_1")
            {
                GameState = GameState.TUTORIAL;
                DOVirtual.DelayedCall(0.75f, StartTutorial);
            }
            else
            {
                if (_entry.IsTimeAttack)
                {
                    if (!GM.Instance.Player.GetTutorialTimeAttackPlayed())
                    {
                        var popup = GM.Instance.Popups.GetPopup(PopupNames.TUTORIAL_TIME_ATTACK,
                            out UIPopupBehaviour behaviour);
                        behaviour.OnShowEnd(() =>
                        {
                            GameState = GameState.IN_GAME;
                            GM.Instance.Player.SetTutorialTimeAttackPlayed();
                            TimeAttackModule.PlayCountdownStart();
                        });
                        popup.Show();
                    }
                    else
                    {
                        GameState = GameState.IN_GAME;
                        TimeAttackModule.PlayCountdownStart();
                    }
                }
                else if (_entry.IsMultiplayer)
                {
                    if (!GM.Instance.Player.GetTutorialMultiplayerPlayed())
                    {
                        var popup = GM.Instance.Popups.GetPopup(PopupNames.TUTORIAL_MULTIPLAYER,
                            out UIPopupBehaviour behaviour);
                        behaviour.OnShowEnd(() =>
                        {
                            GameState = GameState.IN_GAME;
                            GM.Instance.Player.SetTutorialMultiplayerPlayed();
                            MultiplayerModule.PlayCountdownStart();
                        });
                        popup.Show();
                    }
                    else
                    {
                        GameState = GameState.IN_GAME;
                        MultiplayerModule.PlayCountdownStart();
                    }
                }
                else
                {
                    GameState = GameState.IN_GAME;
                }
            }
        }

        private void EnterReviewMode()
        {
            GameState = GameState.REVIEW;
            foreach (var sticker in _dynamicStickers)
            {
                sticker.ResetSticker();
            }

            _hud.SetVisiblePowerupTray(false);
            
            _map.ShowFullMap();
            
            _completeGroup.SetGOActive(true);

            if (_entry.IsTimeAttack)
            {
                TimeAttackModule.SetUse(false);
            }
            else if (_entry.IsMultiplayer)
            {
                MultiplayerModule.SetUse(false, _entry);
            }
        }
        
        private void FillStickerBar(bool shiftImmediately = false)
        {
            // Make shift map
            var shiftList = new List<(int i, int j)>();
            var emptyList = new Queue<int>();
            for (int i = 0; i < _dynamicStickers.Length; i++)
            {
                if (!_dynamicStickers[i].HasLink)
                {
                    emptyList.Enqueue(i);
                }
                else if (emptyList.Count > 0)
                {
                    var j = emptyList.Dequeue();
                    shiftList.Add((i, j));
                    emptyList.Enqueue(i);
                }
            }

            // Shift stickers
            foreach (var (i, j) in shiftList)
            {
                var activeSticker = _dynamicStickers[i];
                var emptySticker = _dynamicStickers[j];
                var activeNewSlot = _itemSlots[j];
                var emptyNewSlot = _itemSlots[i];
                
                activeSticker.ShiftTo(activeNewSlot, shiftImmediately);
                emptySticker.ShiftTo(emptyNewSlot, shiftImmediately);
                
                // Swap position
                (_dynamicStickers[j], _dynamicStickers[i]) = (_dynamicStickers[i], _dynamicStickers[j]);
            }

            // Fill
            foreach (var dynamicSticker in _dynamicStickers)
            {
                if (dynamicSticker.HasLink) continue;
                
                if (_leftOverStickers == null || _leftOverStickers.Count == 0)
                {
                    dynamicSticker.ResetSticker();
                    continue;
                }
                    
                // Fill with first of left over
                var sticker = _leftOverStickers[0];
                _leftOverStickers.RemoveAt(0);
                dynamicSticker.Link(sticker);

                // Positioning
                dynamicSticker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                dynamicSticker.PlaySpawn();
            }
        }

        private bool EvaluateCompletedStickers()
        {
            return _leftOverStickers.Count <= 0
                   && _dynamicStickers.All(x => !x.HasLink);
        }

        private void EndGame(bool completedFully)
        {
            if (GameState >= GameState.COMPLETING) return;
            
            GameState = GameState.COMPLETING;
                        
            // Conclude power-ups
            ConcludePowerupSession();

            if (!_entry.IsMultiplayer)
            {
                NormalEndgameBehaviour(completedFully);
            }
            else
            {
                MultiplayerEndgameBehaviour();
            }

            if (_entry.IsTimeAttack) TimeAttackModule.OnCompleteLevel();
            if (_entry.IsMultiplayer) MultiplayerModule.OnCompleteLevel();
        }

        private void NormalEndgameBehaviour(bool isWin)
        {
            if (isWin)
            {
                _completeGroup.SetGOActive(true);
                
                // TODO: Extra VFXs
                AudioManager.PlaySound(LibrarySounds.Congrats);
            }

            if (isWin)
            {
                // Play color image
                _map.AnimateFullMap(
                    () => DOVirtual.DelayedCall(3f, () =>
                    {
                        var request = new AdRequest(
                            AdManager.TYPE_INTERSTITIAL,
                            () => OnEndGameAnimateDone(true),
                            () => OnEndGameAnimateDone(true));
                        GM.Instance.Ad.RequestAd(request);
                    }));
                
                SaveLevelComplete();
                
                GM.Instance.Events.MakeEvent(GameEvents.COMPLETE_LEVEL)
                    .Add("level", _currentId)
                    .SendEvent();
            }
            else
            {
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    var request = new AdRequest(
                        AdManager.TYPE_INTERSTITIAL,
                        () => OnEndGameAnimateDone(false),
                        () => OnEndGameAnimateDone(false));
                    GM.Instance.Ad.RequestAd(request);
                });
            }
            
            GM.Instance.UpdateLeaderboardAfterGame();
        }

        private void MultiplayerEndgameBehaviour()
        {
            MultiplayerModule.GetStatistics(out var opponentName, out var playerScore, out var opponentScore);
            var win = playerScore >= opponentScore;
            GM.Instance.UpdateLeaderboardAfterMatch(win ? playerScore * 2 : playerScore, opponentName, !win ? opponentScore * 2 : opponentScore);
            
            DOVirtual.DelayedCall(0.75f, () =>
            {
                var request = new AdRequest(
                    AdManager.TYPE_INTERSTITIAL,
                    () => OnEndGameMultiplayerAdDone(opponentName, playerScore, opponentScore, win),
                    () => OnEndGameMultiplayerAdDone(opponentName, playerScore, opponentScore, win));
                GM.Instance.Ad.RequestAd(request);
            });
        }

        private void OnEndGameAnimateDone(bool isWin)
        {
            GameState = GameState.COMPLETED;
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourWin>(out var behaviour);
            behaviour.Setup(_currentId);

            behaviour.SetWinMode(_entry.IsTimeAttack ? WinMode.TIME_ATTACK : WinMode.NORMAL, isWin);

            popup.Show();
        }

        private void OnEndGameMultiplayerAdDone(string opponentName, int playerScore, int opponentScore, bool win)
        {
            GameState = GameState.COMPLETED;
            
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourWinMultiplayer>(out var behaviour);
            behaviour.SetInfo(_entry, opponentName, playerScore, opponentScore, win);
            popup.Show();
        }
    }
}