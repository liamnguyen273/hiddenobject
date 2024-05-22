using System;
using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class MainGameTimeAttackModule : MonoBehaviour
    {
        public const float LEVEL_TIME = 60;

        public MainGameManager Manager { get; set; }

        [SerializeField] private GameObject _readyGroup;
        [SerializeField] private GameObject _blocker;
        [SerializeField] private CanvasGroup _readyCanvas;
        [SerializeField] private Image _clockFill;
        [SerializeField] private Transform[] _readyTimings;
        [SerializeField] private Transform _startText;
        [SerializeField] private MainGameHudTimeAttackModule _hud;

        private bool _inUse = false;
        private bool _isPaused = false;
        private bool _active = false;
        private float _timer = -1f;

        private Tween _startCountdownSequence;

        public bool InUse => _inUse;
        
        public void SetUse(bool use)
        {
            _inUse = use;
            gameObject.SetActive(_inUse);   
            _hud.SetGOActive(_inUse);
        }

        public void UpdateTimer(float dt)
        {
            if (!_inUse || !_active || _isPaused) return;
            
            _timer -= dt;
            
            _hud.SetHud(_timer);

            if (_timer < 0)
            {
                Manager.OnTimeAttackModuleTimerEnd();
            }
        }

        public void PlayCountdownStart()
        {
            if (!_inUse) return;
            
            // Init
            _hud.SetGOActive(false);
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
                _active = true;
            });

            _timer = LEVEL_TIME;
            _startCountdownSequence = sequence.Play();
        }

        public void OnCompleteLevel()
        {
            if (_inUse)
            {
                _active = false;
                _timer = -1;
            }
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
    }
}