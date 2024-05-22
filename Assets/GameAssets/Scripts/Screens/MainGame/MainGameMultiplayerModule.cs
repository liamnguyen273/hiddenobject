using System;
using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class MainGameMultiplayerModule : MonoBehaviour
    {
        public const float HALF_TIME = 60f;

        public MainGameManager Manager { get; set; }
        
        [SerializeField] private GameObject _readyGroup;
        [SerializeField] private GameObject _blocker;
        [SerializeField] private CanvasGroup _readyCanvas;
        [SerializeField] private Image _clockFill;
        [SerializeField] private Transform[] _readyTimings;
        [SerializeField] private Transform _startText;
        [SerializeField] private MainGameHudMultiplayerModule _hud;
        [SerializeField] private GameObject _firstHalfText;
        [SerializeField] private GameObject _secondHalfText;
        
        [SerializeField] private Image _opponentAvatar;
        [SerializeField] private TextLocalizer _opponentNameText;

        private bool _inUse = false;
        private LevelEntry _entry = null;
        private bool _isPaused = false;
        private int _halfIndex = 0;
        private bool _active = false;
        private float _timer = -1f;

        private string _opponentName;
        private Sprite _opponentAvatarSprite;

        private int _youScore;
        private int _opponentScore;

        private Tween _startCountdownSequence;

        public bool InUse => _inUse;
        
        public void SetUse(bool use, LevelEntry entry)
        {
            _inUse = use;
            gameObject.SetActive(_inUse);   
            _hud.SetGOActive(_inUse);

            _entry = entry;
            ResetCursor();
        }

        public void SetOpponentRandomly()
        {
            var allNames = GM.Instance.Data.GetLeaderboardNames();
            var randIndex = GM.Instance.Rng.GetInteger(0, allNames.Count);
            _opponentName = allNames[randIndex];
        }

        public void GetStatistics(out string opponentName, out int playerScore, out int opponentScore)
        {
            opponentName = _opponentName;
            playerScore = _youScore;
            opponentScore = _opponentScore;
        }

        public void UpdateTimer(float dt)
        {
            // Cannot be paused
            if (!_inUse || !_active /* || _isPaused */) return;
            
            _timer -= dt;
            
            _hud.SetHud(_timer);

            if (_timer < 0 && _halfIndex >= 1)
            {
                DropSticker();
                Manager.OnMultiplayerModuleTimerEnd();
                return;
            }
            else if (_timer < 0 && _halfIndex < 1)
            {
                DropSticker();
                OnConcludeFirstHalf();
                return;
            }
            
            // Cursor mover
            TargetSticker();
            FindDelay(dt);
            Delay(dt);
            MoveTowardsGoal();
        }

        public void PlayCountdownStart()
        {
            if (!_inUse) return;
            
            // Set start info
            var _opponentAvatarSprite = GM.Instance.Data.GetAvatar(_opponentName);
            _opponentAvatar.sprite = _opponentAvatarSprite;
            _opponentNameText.RawString = _opponentName;

            _youScore = 0;
            _opponentScore = 0;
            
            _hud.SetInfo(_opponentName);
            _hud.SetScorePlayer(_youScore);
            _hud.SetScoreOpponent(_opponentScore);
            
            // Init
            _hud.SetGOActive(false);
            _firstHalfText.SetActive(true);
            _secondHalfText.SetActive(false);
            _readyGroup.SetActive(true);
            _blocker.SetActive(true);
            _readyCanvas.alpha = 0f;
            foreach (var timing in _readyTimings)
            {
                timing.SetGOActive(false);
            }
            _startText.SetGOActive(false);
            
            var sequence = DOTween.Sequence().AppendInterval(1.25f)
            // 1) Fade _readyGroup
            .Append(_readyCanvas.DOFade(1f, 0.25f));
            // 2) Tick timer

            _clockFill.fillAmount = 0f;

            var subSequence = DOTween.Sequence();
            foreach (var timing in _readyTimings)
            {
                timing.localScale = Vector3.one;
                subSequence.AppendCallback(() => timing.SetGOActive(true))
                    .Append(timing.DOScale(1.2f, 1).SetEase(Ease.OutBack))
                    .AppendCallback(() => timing.SetGOActive(false));
            }

            sequence.Append(_clockFill.DOFillAmount(0.8f, 3f))
                .Join(subSequence);
            
            // 3) Show start
            sequence.AppendCallback(() =>
                {
                    _startText.SetGOActive(true);
                })
                .Append(_startText.DOMoveY(1.5f, 0.15f).SetEase(Ease.OutExpo))
                // 4) Fade _readyGroup (quickly)
                .Join(_readyCanvas.DOFade(0f, 0.25f))
                .AppendCallback(() =>
                {
                    _blocker.SetActive(false);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() => _startText.SetGOActive(false));

            sequence.AppendCallback(() =>
            {
                _startCountdownSequence = null;
                _readyGroup.SetActive(false);
                _hud.SetGOActive(true);
                
                _findDelayTimer = _findInterval;
                _delayTimer = GetDelayTime();

                
                _active = true;
            });

            _timer = HALF_TIME;
            _halfIndex = 0;
            _startCountdownSequence = sequence.Play();
        }

        public void PlayCountdownSecondHalf()
        {
            if (!_inUse) return;
            
            // Init
            _hud.SetTimeNode(false);
            _firstHalfText.SetActive(false);
            _secondHalfText.SetActive(true);
            _readyGroup.SetActive(true);
            _blocker.SetActive(true);
            _readyCanvas.alpha = 0f;
            foreach (var timing in _readyTimings)
            {
                timing.SetGOActive(false);
            }
            _startText.SetGOActive(false);
            
            var sequence = DOTween.Sequence().AppendInterval(1.25f)
            // 1) Fade _readyGroup
            .Append(_readyCanvas.DOFade(1f, 0.25f));
            // 2) Tick timer

            _clockFill.fillAmount = 0f;

            var subSequence = DOTween.Sequence();
            foreach (var timing in _readyTimings)
            {
                timing.localScale = Vector3.one;
                subSequence.AppendCallback(() => timing.SetGOActive(true))
                    .Append(timing.DOScale(1.2f, 1).SetEase(Ease.OutBack))
                    .AppendCallback(() => timing.SetGOActive(false));
            }

            sequence.Append(_clockFill.DOFillAmount(0.8f, 3f))
                .Join(subSequence);
            
            // 3) Show start
            sequence.AppendCallback(() =>
                {
                    _startText.SetGOActive(true);
                })
                .Append(_startText.DOMoveY(1.5f, 0.15f).SetEase(Ease.OutExpo))
                // 4) Fade _readyGroup (quickly)
                .Join(_readyCanvas.DOFade(0f, 0.25f))
                .AppendCallback(() =>
                {
                    _blocker.SetActive(false);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() => _startText.SetGOActive(false));

            sequence.AppendCallback(() =>
            {
                _startCountdownSequence = null;
                _readyGroup.SetActive(false);
                _hud.SetTimeNode(true);

                _findDelayTimer = _findInterval;
                _delayTimer = GetDelayTime();
                
                _active = true;
            });

            _timer = HALF_TIME;
            _halfIndex = 1;
            _startCountdownSequence = sequence.Play();
        }

        public void AddPlayerScore(int score = 1)
        {
            _youScore += score;
            _hud.SetScorePlayer(_youScore);
        }
        
        public void AddOpponentScore(int score = 1)
        {
            _opponentScore += score;
            _hud.SetScoreOpponent(_opponentScore);
        }

        public void OnCompleteLevel()
        {
            if (_inUse)
            {
                _active = false;
                _timer = -1;
            }
            
            HideCursor();
        }
        
        public void OnGameState(GameState state)
        {
            if (!_inUse) return;
            
            switch (state)
            {
                case GameState.NONE:
                    _timer = -1f;
                    _active = false;
                    _isPaused = false;
                    
                    _readyGroup.SetActive(false);
                    _startText.SetGOActive(false);
                    _blocker.SetActive(false);
                    
                    _startCountdownSequence?.Kill();
                    _startCountdownSequence = null;
                    break;
                case GameState.LOADING:
                    break;
                case GameState.LOAD_DONE:
                    break;
                case GameState.TUTORIAL:
                    break;
                case GameState.IN_GAME:
                    _isPaused = false;
                    break;
                case GameState.PAUSED:
                    _isPaused = true;
                    break;
                case GameState.COMPLETING:
                    _blocker.SetActive(true);
                    break;
                case GameState.COMPLETED:
                    _active = false;
                    break;
                case GameState.REVIEW:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void OnHalftimeAdOver()
        {
            PlayCountdownSecondHalf();
        }

        private void OnConcludeFirstHalf()
        {
            // TODO: Drop
            Manager.ForcePlayerDropSticker();
            Manager.RequestGameplayAd();
            _active = false;
            _entry = null;
        }

        [Header("Cursor")]
        [SerializeField] private Transform _cursor;
        [SerializeField] private Transform _cursorMimic;
        [SerializeField] private float _baseMovementUnit = 1000f;
        [SerializeField] private float _moveTime = 2f;
        [SerializeField] private float _findInterval = 0.25f;
        [SerializeField] private float _delay = 1f;
        [SerializeField] private float _cursorInOutTime = 0.35f;
        

        private float GetMoveTime()
        {
            var moveTime = _moveTime / _entry?.GetSpeedMod() ?? 1f;
            return GM.Instance.Rng.GetFloat(0.75f, 1.25f) * moveTime;
        }
        
        private float GetDelayTime()
        {
            var delayTime = _delay / _entry?.GetSpeedMod() ?? 1f;
            return _cursorInOutTime + GM.Instance.Rng.GetFloat(0.65f, 1.35f) * delayTime;
        }

        private DynamicSticker _currentSticker;
        private float _findDelayTimer;
        private float _delayTimer;
        private bool _shouldStartMove;
        
        private Tween _cursorTween;
        private Tween _moverTween;

        private void SyncCursorMimic()
        {
            var pos = _cursor.transform.position;
            pos.z = _cursorMimic.position.z;
            _cursorMimic.position = pos;
        }
        
        private void TargetSticker()
        {
            if (_currentSticker != null || _findDelayTimer > 0f) return;
            
            var sticker = Manager.GetRandomValidPickupableDynamicSticker();
            if (sticker == null) return;

            _currentSticker = sticker;
            _shouldStartMove = false;

            var pos = _currentSticker.transform.position;
            pos.z = _cursor.transform.position.z;
            _cursor.transform.position = pos;
            
            LogObj.Default.Info("Multi", "Target picked");
            
            ShowCursor();
            _shouldStartMove = true;
        }

        private void MoveTowardsGoal()
        {
            if (_currentSticker == null || _moverTween != null || !_shouldStartMove) return;

            LogObj.Default.Info("Multi", "Start move towards goal");

            var target = _currentSticker.GetLinkedStaticSticker().transform.localPosition;
            target.z = _cursor.localPosition.z;
            
            var distance = Vector2.Distance(transform.localPosition, Vector3.zero);
            var time = GetMoveTime() * (distance / _baseMovementUnit);
            
            _moverTween?.Kill();

            Ease GetEase()
            {
                return GM.Instance.Rng.GetInteger(0, 4) switch
                {
                    0 => Ease.OutCubic,
                    1 => Ease.OutSine,
                    2 => Ease.OutQuad,
                    3 => Ease.OutQuart,
                    _ => Ease.Linear
                };
            }

            var dragging = _currentSticker.SimulateBeginDrag(false);

            if (dragging)
            {
                _moverTween = _cursor.DOLocalMove(target, GetMoveTime())
                    .SetEase(GetEase())
                    .OnUpdate(() =>
                    {
                        var pos = _cursor.transform.position;
                        pos.z = _currentSticker.transform.position.z;
                        _currentSticker.transform.position = pos;
                        SyncCursorMimic();
                    })
                    .OnComplete(() =>
                    {
                        LogObj.Default.Info("Multi", "End move towards goal.");
                        _moverTween = null;
                        DropSticker();
                    });
            }
            else
            {
                LogObj.Default.Warn("Multi", "Multiplayer's bot cannot pick up a dynamic sticker.");
                _currentSticker = null;
            }
            
            _shouldStartMove = false;
        }

        private void DropSticker()
        {
            if (_currentSticker == null) return;
            
            LogObj.Default.Info("Multi", "Dropped sticker.");

            _moverTween?.Kill();
            _moverTween = null;
            
            _currentSticker.SimulateEndDrag(false, true);
            
            _currentSticker = null;
            _shouldStartMove = false;
            
            _delayTimer = GetDelayTime();
            _findDelayTimer = 0.1f;
            
            HideCursor();
        }

        private void FindDelay(float dt)
        {
            if (_delayTimer > 0 || _findDelayTimer <= 0f) return;

            _findDelayTimer -= dt;
        }
        
        private void Delay(float dt)
        {
            if (_delayTimer <= 0f) return;

            _delayTimer -= dt;
        }

        private void ShowCursor()
        {
            _cursorTween?.Kill();
            _cursorMimic.localScale = Vector3.zero;

            // In case of misalignment
            SyncCursorMimic();
            
            _cursorTween = _cursorMimic.DOScale(Vector3.one, _cursorInOutTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _cursorTween = null;
                })
                .Play();
        }

        private void HideCursor()
        {
            _cursorTween?.Kill();

            _cursorTween = _cursorMimic.DOScale(Vector3.zero, _cursorInOutTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _cursorTween = null)
                .Play();
        }

        private void ResetCursor()
        {
            DropSticker();
            
            _cursor.localPosition = Vector3.zero;
            _cursorMimic.localScale = Vector3.zero;
            
            _cursorTween?.Kill();
            _cursorTween = null;
            _moverTween?.Kill();
            _moverTween = null;

            _currentSticker = null;
            _findDelayTimer = -1f;
            _delayTimer = -1f;
            _shouldStartMove = false;
        }
    }
}