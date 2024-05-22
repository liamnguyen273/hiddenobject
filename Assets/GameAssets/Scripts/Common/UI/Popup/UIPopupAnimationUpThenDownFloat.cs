using System;
using DG.Tweening;
using UnityEngine;

namespace com.brg.Common.UI
{
    public class UIPopupAnimationUpThenDownFloat : UIPopupAnimationBase
    {
        [SerializeField] private float _time = 0.45f;
        
        private RectTransform _panel;
        private CanvasGroup _canvasGroup;
        private float _height;

        internal override void Initialize()
        {
            _canvasGroup = transform.Find("Background")?.GetComponent<CanvasGroup>();
            _panel = transform.Find("Panel")?.GetComponent<RectTransform>();

            _height = _panel.rect.height;
            
            base.Initialize();
        }

        protected override Tween GetShowTween()
        {
            return DOTween.Sequence()
                .Insert(0f, GetShowBackgroundTweenOnly())
                .Insert(0f, GetPanelShowTweenOnly());
        }

        protected override Tween GetHideTween()
        {
            return DOTween.Sequence()
                .Insert(0f, GetHideBackgroundTweenOnly())
                .Insert(0f, GetPanelHideTweenOnly());
        }

        protected override void PerformShowImmediately()
        {
            _panel.anchoredPosition = Vector2.zero;
            _canvasGroup.alpha = 0.75f;
        }
        
        protected override void PerformHideImmediately()
        {
            _panel.anchoredPosition = new Vector2(_panel.anchoredPosition.x, -_height - 15);
            _canvasGroup.alpha = 0f;
        }

        protected Tween GetShowBackgroundTweenOnly()
        {
            return DOTween.To(
                () => _canvasGroup.alpha,
                (x) => _canvasGroup.alpha = x,
                1f, _time);
        }        
        
        protected Tween GetHideBackgroundTweenOnly()
        {
            return DOTween.To(
                () => _canvasGroup.alpha,
                (x) => _canvasGroup.alpha = x,
                0f, _time);
        }
        
        protected Tween GetPanelShowTweenOnly()
        {
            _panel.anchoredPosition = new Vector2(_panel.anchoredPosition.x, -_height - 15);
            return _panel.DOAnchorPosY(0f, _time)
                .SetEase(Ease.OutSine)
                .SetUpdate(UpdateType.Normal, true);
        }

        protected Tween GetPanelHideTweenOnly()
        {
            return _panel.DOAnchorPosY(-_height - 15, _time)
                .SetEase(Ease.OutSine)
                .SetUpdate(UpdateType.Normal, true);
        }
    }
}