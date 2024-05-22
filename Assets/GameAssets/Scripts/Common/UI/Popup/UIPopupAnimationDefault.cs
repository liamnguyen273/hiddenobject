using System;
using DG.Tweening;
using UnityEngine;

namespace com.brg.Common.UI
{
    public class UIPopupAnimationDefault : UIPopupAnimationBase
    {
        [SerializeField] private float _time = 0.45f;
        
        private Transform _panel;
        private CanvasGroup _canvasGroup;

        internal override void Initialize()
        {
            _canvasGroup = transform.Find("Background")?.GetComponent<CanvasGroup>();
            _panel = transform.Find("Panel")?.GetComponent<RectTransform>();
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
            _panel.localScale = Vector3.one;
            _canvasGroup.alpha = 0.75f;
        }
        
        protected override void PerformHideImmediately()
        {
            _panel.localScale = Vector3.zero;
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
            return _panel.DOScale(1, _time)
                .SetEase(Ease.OutBack)
                .SetUpdate(UpdateType.Normal, true);
        }

        protected Tween GetPanelHideTweenOnly()
        {
            return _panel.DOScale(0, _time)
                .SetEase(Ease.InBack)
                .SetUpdate(UpdateType.Normal, true);
        }
    }
}