using System;
using com.brg.Common;
using com.brg.Common.UI;
using DG.Tweening;
using UnityEngine;

namespace GameAssets.Scripts.Screens
{
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _moveGroup;
        [SerializeField] private UIButton _button;

        public EventWrapper Event => _button.Event;
        
        private bool _isActive = false;
        private Tween _animTween;

        private void Awake()
        {
            _isActive = false;
            _canvasGroup.alpha = 0f;
            _moveGroup.anchoredPosition = new Vector2(0f, -74f);
        }

        public void SetState(bool active, bool immediately, float time)
        {
            _animTween?.Kill();
            _animTween = null;

            if (_isActive == active) return;
            
            if (immediately)
            {
                _canvasGroup.alpha = active ? 1f : 0f;
                _moveGroup.anchoredPosition = new Vector2(0f, active ? 0 : -74f);
            }
            else
            {
                _animTween = DOTween.Sequence()
                    .Insert(0f, _canvasGroup.DOFade(active ? 1f : 0f, time))
                    .Insert(0f, _moveGroup.DOAnchorPosY(active ? 0f : -74f, time)
                        .SetEase(Ease.InOutQuart))
                    .OnComplete(() =>
                    {
                        _animTween = null;
                    }).Play();
            }

            _isActive = active;
        }
    }
}