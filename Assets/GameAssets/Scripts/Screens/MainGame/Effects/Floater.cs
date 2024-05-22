using System;
using com.brg.Common.Localization;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker.Effects
{
    public class Floater : MonoBehaviour
    {
        [SerializeField] private TextLocalizer _text;

        private float _delay = 0.75f;
        private float _upDist = 150f;
        private Tween _tween;
        
        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            var y = rect.anchoredPosition.y;
            _tween = DOTween.Sequence()
                .Append(rect.DOAnchorPosY(y + _upDist, 0.25f).SetEase(Ease.OutCubic))
                .AppendInterval(_delay)
                .AppendCallback(() =>
                {
                    _tween = null;
                    Destroy(gameObject);
                });
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            _tween = null;
        }

        public void Set(string text, float upDist = 150f, float delay = 0.75f)
        {
            _text.RawString = text;
            _upDist = upDist;
            _delay = delay;
        }
    }
}