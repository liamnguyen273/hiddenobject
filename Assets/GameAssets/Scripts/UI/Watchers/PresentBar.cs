using System;
using System.Linq;
using com.brg.Common.UI;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PresentBar : NumberBar
    {
        [SerializeField] private bool _animateOnUpdate = false;
        [SerializeField] private UIButton _button;

        private Vector2 _originalPos;

        private Tween _updateTween;
        private Tween _filledTween;

        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            _originalPos = rect.anchoredPosition;
        }

        protected override void OnEnable()
        {
            _updateTween?.Kill();
            _updateTween = null;
            _filledTween?.Kill();
            _filledTween = null;
            
            var rect = GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _updateTween?.Kill();
            _updateTween = null;
            _filledTween?.Kill();
            _filledTween = null;
        }

        public void OnClick()
        {
            if (_cachedValue >= _upperBound)
            {
                ResolveReceive();
            }
        }

        protected override void OnResourceChange(int newValue, int change)
        {
            base.OnResourceChange(newValue, change);
            
            if (change != 0 && _animateOnUpdate)
            {
                _updateTween?.Kill();
                
                var rect = GetComponent<RectTransform>();
                _updateTween = DOTween.Sequence()
                    .Append(rect.DOAnchorPosY(_originalPos.y - 180, 0.35f)
                        .SetEase(Ease.OutBack))
                    .AppendInterval(2f)
                    .Append(rect.DOAnchorPosY(_originalPos.y, 0.35f))
                    .OnComplete(() => _updateTween = null)
                    .Play();
            }
        }

        protected override void OnBarUpdateDone()
        {
            base.OnBarUpdateDone();
            
            var filled = _cachedValue >= _upperBound;

            var rect = GetComponent<RectTransform>();
            if (filled)
            {
                _button.Interactable = true;
                
                // Kill update tween
                _updateTween?.Kill();
                _updateTween = null;
                
                // Move down
                if (_animateOnUpdate)
                {                
                    _updateTween = rect.DOAnchorPosY(_originalPos.y - 180, 0.75f)
                    .OnComplete(() => _updateTween = null)
                    .Play();
                }
                
                // Shake tween
                _filledTween?.Kill();
                _filledTween = null;
                
                _filledTween = DOTween.Sequence()
                    .Append(rect.DOScale(1.1f, 0.75f).SetEase(Ease.InOutSine))
                    .Append(rect.DOScale(1f, 0.75f).SetEase(Ease.InOutSine))
                    .SetLoops(-1, LoopType.Restart)
                    .Play();
            }
            else
            {
                _button.Interactable = false;
                
                // Kill update tween
                _updateTween?.Kill();
                _updateTween = null;
                
                // Move up
                if (_animateOnUpdate)
                {                
                    _updateTween = rect.DOAnchorPosY(_originalPos.y, 0.75f)
                        .OnComplete(() => _updateTween = null)
                        .Play();
                }
                
                _filledTween?.Kill();
                _filledTween = null;

                _filledTween = rect.DOScale(1f, 0.75f).SetEase(Ease.InBack)
                    .OnComplete(() => _filledTween = null)
                    .Play();
            }
        }

        private void ResolveReceive()
        {
            var progress = GM.Instance.Player.GetResource(GlobalConstants.CHEST_PROGRESS_RESOURCE);
            if (progress >= 60)
            {
                var rewards = RollRewards();
                GM.Instance.Player.UseResource(GlobalConstants.CHEST_PROGRESS_RESOURCE, 60);

                var from = Camera.main.WorldToScreenPoint(transform.position);
                
                if (rewards.Length == 1 && rewards[0] == GlobalConstants.STAMP_RESOURCE)
                {
                    var stampCount = GM.Instance.Rng.GetInteger(10, 40);
                    GM.ResourceSplit(GlobalConstants.STAMP_RESOURCE, stampCount, out var items, out var counts);
                    GM.Instance.ResolveAnimateAddItems(from, items, counts, false);
                }
                else
                {
                    GM.Instance.ResolveAnimateAddItems(from, rewards, false);
                }
            }
            
            // Do animations
        }

        private static string[] RollRewards()
        {
            var rollCase = GM.Instance.Rng.GetInteger(0, 6);

            switch (rollCase)
            {
                case 1:
                    return new[] { GlobalConstants.POWER_LOOKUP, GlobalConstants.POWER_LOOKUP };  
                case 2:
                    return new[] { GlobalConstants.POWER_COMPASS, GlobalConstants.POWER_COMPASS };
                case 3:
                    return new[] { GlobalConstants.POWER_LOOKUP, GlobalConstants.POWER_COMPASS };
                default:
                    return new[] { GlobalConstants.STAMP_RESOURCE };
            }
        }
    }
}