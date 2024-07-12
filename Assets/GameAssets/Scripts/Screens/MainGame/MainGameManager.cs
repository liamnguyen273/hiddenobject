using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private const float TIME_ATTACK_TIME = 105;
        private const float TIME_ATTACK_TIME_BONUS = 45;
        private const int REVIVE_COUNT = 2;
        private const int REVIVE_COUNT_TIME_ATTACK = 1;
        
        [Header("Level components")] 
        [SerializeField] private RectTransform[] _overallRects;
        [SerializeField] private RectTransform _bannerRect;
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

        [Header("Others")] 
        [SerializeField] private StemManager _stemManager;
        [SerializeField] private Slider _stemProgress;
        [SerializeField] private RectTransform _stemIcon;
        
        // Level book-keepers
        private string _currentId = null;
        private LevelEntry _entry = null;
        private LevelAssetHandle _assetHandle = null;

        // private List<StaticSticker> _leftOverStickers;
        // private List<StaticSticker> _attachedStickers;
        private List<StaticSticker> _findStickers;
        private List<StaticSticker> _foundStickers;
        private List<StaticSticker> _allStickers;
        private List<StaticSticker> _allFindStickers;
        
        private GameState _state = GameState.NONE;
        private float _gameTimer = -1;

        [SerializeField] private TextLocalizer _tryCountText;
        private int _tryCount;
        private int _reviveCount;

        public int TryCount
        {
            get => _tryCount;
            set
            {
                _tryCount = value;
                _tryCountText.RawString = $"TRIES: {_tryCount}";
            }
        }
        
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

            if (GameState == GameState.LOADING && (_hasAssetLoad || _hasStemLoad))
            {
                UpdateLoadAssetPack();
                UpdateLoadStemPack();

                if (!_hasAssetLoad && !_hasStemLoad)
                {
                    OnAssetFullyLoaded();
                }
            }

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
            return list.Count == 0 ? null : list[GM.Instance.Rng.GetInteger(0, Mathf.Min(3, list.Count))];
        }

        public void OnStickerClickFailed(PointerEventData eventData)
        {
            // Do nothing if time or multi
            if (TimeAttackModule.InUse || MultiplayerModule.InUse)
            {
                return;
            }
            
            if (TryCount == 0)
            {
                ShowPopupOutOfTries();
                return;
            }
            
            GM.Instance.Effects.MakeXMark(eventData.pressPosition);
            TryCount = Mathf.Max(0, TryCount - 1);
            
            if (TryCount == 0)
            {
                ShowPopupOutOfTries();
            }
        }

        private void ShowPopupOutOfTries()
        {
            if (_reviveCount <= 0)
            {
                var completed = EvaluateCompletedStickers();
                EndGame(completed);
                return;
            }
            
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourOutOfTries>(out var behaviour);
            behaviour.SetContent(_reviveCount == 1 ? "Final Revive?" : "Revive?", $"You ran out of tries!\nWatch an ad to get 3 more?{(_reviveCount == 1 ? "\n(This is your final revive)" : "")}");
            behaviour.SetOnYesCallback(() =>
            {
                TryCount += TRY_COUNT;
                _reviveCount -= 1;
            }, PerformReplayLevel);
            popup.Show();
        }
        
        private void ShowPopupOutOfTime()
        {
            if (_reviveCount <= 0)
            {
                var completed = EvaluateCompletedStickers();
                EndGame(completed);
                return;
            }
            
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourOutOfTime>(out var behaviour);
            behaviour.SetOnYesCallback(() =>
            {
                TimeAttackModule.PlayCountdownStart(TIME_ATTACK_TIME_BONUS);
                _reviveCount -= 1;
            }, PerformReplayLevel);
            behaviour.SetOnNoCallback(() =>
            {
                EndGame(false);
            });
            popup.Show();
        }

        // public void OnStickerStickToTarget(DynamicSticker dynamicSticker, bool byPlayer)
        public void OnStickerFound(StaticSticker staticSticker, bool byPlayer)
        {
            if (TryCount == 0)
            {
                ShowPopupOutOfTries();
                return;
            }
            
            var index = Array.FindIndex(_dynamicStickers, x => x.LinkedStaticSticker == staticSticker);
            
            if (index > -1)
            {
                var dynamicSticker = _dynamicStickers[index];
                
                // if (dynamicSticker.GetLinkedStaticSticker() == null)
                // {
                //     Log.Warn($"Dynamic sticker index {index} is missing a link but managed to call OnStickerFound." +
                //              $"Please check. It will disappear as normal.");
                //     return;
                // }
                
                // staticSticker.SetState(StaticStickerState.TEMP_HIDDEN);
                staticSticker.SetInteractable(false);
                staticSticker.SetState(StaticStickerState.COLORED);
                staticSticker.PlayPulseTween();
                
                _foundStickers.Add(staticSticker);
                
                GM.Instance.Effects.PlayStickingParticles(staticSticker.transform, Vector3.zero);
                    
                _hud.UpdateProgress(_foundStickers.Count, _allFindStickers.Count);

                ResolvePowerupOnPlayerStick(dynamicSticker);
                dynamicSticker.ResetSticker();
                
                // Add resource
                if (byPlayer)
                {
                    GM.Instance.Player.AddResource(GlobalConstants.CHEST_PROGRESS_RESOURCE, 1, doNotSave: true);
                }
                
                // Play sound
                var stemIndex = UpdateStemIndex(_foundStickers.Count, _allFindStickers.Count);
                CLog.Log($"Stem index: {stemIndex}");
                if (stemIndex >= 0)
                {
                    AudioManager.PlaySound(LibrarySounds.Unlock);
                    _stemManager.Play(false, stemIndex);
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
                // Log.Warn("Sticker " + dynamicSticker + " does not exist in the map's bookkeeping.");
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
                .Add("leftover_sticker_number", _findStickers?.Count ?? 0)
                .SendEvent();
            
            // Save progress
            // SaveProgress(true);
            
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
                ShowPopupOutOfTime();
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
            behaviour.SetQuestion("Replay level?", "Clear the canvas and play again from the start?", PerformReplayLevel, null);
            popup.Show();
        }

        public void PerformReplayLevel()
        {
            GM.Instance.Loading.RequestLoad(new ImmediateProgressItem(), () =>
            {
                // Clear progress
                GM.Instance.Player.ClearLevelProgress(_currentId);
                    
                // Restart map
                _map.Restart();
                
                // Restart book-keep
                PopulateStickerBookkeeping(true);
                
                // Remove dynamic sticker book-keep
                foreach (var sticker in _dynamicStickers)
                {
                    sticker.ResetSticker();
                }
                
                FillStickerBar();

            }, StartGame);
        }

        private const int TRY_COUNT = 3;
        private void StartGame()
        {
            _saveProgressMeter = 0;
            TryCount = TRY_COUNT;
            _reviveCount = !_entry.IsTimeAttack ? REVIVE_COUNT : REVIVE_COUNT_TIME_ATTACK;
            
            TimeAttackModule.SetUse(_entry.IsTimeAttack);
            MultiplayerModule.SetUse(_entry.IsMultiplayer, _entry);
            
            if (!TimeAttackModule.InUse && !MultiplayerModule.InUse)
            {
                _tryCountText.SetGOActive(true);
            }
            else
            {
                _tryCountText.SetGOActive(false);
            }
            
            if (_entry.IsMultiplayer)
            {
                // Generate random opponent
                MultiplayerModule.SetOpponentRandomly();
            }
            
            if (_findStickers.Count > 0)
            {
                // In game
                _stemManager.Play(true, 0);
                UpdateStemIndex(0, _allFindStickers.Count);
                EnterPlayMode();
                _replayButton.SetActive(false);
            }
            else
            {
                // Game completed, enter review mode
                _stemManager.PlayAll();
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
                .Add("sticker_number", _findStickers.Count.ToString())
                .SendEvent();

            // TODO
            // if (!GM.Instance.Player.GetTutorialPlayed() && _attachedStickers.Count == 0 && _currentId == "level_1")
            if (false)
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
                            TimeAttackModule.PlayCountdownStart(TIME_ATTACK_TIME);
                        });
                        popup.Show();
                    }
                    else
                    {
                        GameState = GameState.IN_GAME;
                        TimeAttackModule.PlayCountdownStart(TIME_ATTACK_TIME);
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
                
                if (_findStickers == null || _findStickers.Count == 0)
                {
                    dynamicSticker.ResetSticker();
                    continue;
                }
                    
                // Fill with first of left over
                var sticker = _findStickers[0];
                _findStickers.RemoveAt(0);
                dynamicSticker.Link(sticker);

                // Positioning
                dynamicSticker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                dynamicSticker.PlaySpawn();
            }
        }

        private bool EvaluateCompletedStickers()
        {
            return _findStickers.Count <= 0
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

        private int UpdateStemIndex(int filledCount, int total)
        {
            // First one is always played
            const int incomingStemCount = 7;
            var step = total / (incomingStemCount);
            
            var d = filledCount / step;
            var m = filledCount % step;

            var low = d * step;
            var high = Mathf.Min((d + 1) * step, total);
            _stemProgress.value = Mathf.InverseLerp(low, high, filledCount);
            _stemProgress.SetGOActive(d <= incomingStemCount);
            
            if (m != 0) return -1;
            if (d > incomingStemCount) return -1;

            return d;
        }
    }
}