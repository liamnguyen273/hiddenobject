using System;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using Lean.Transition;
using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class NumberBar : NumberWatcher
    {
        [Header("Bar Components")]
        [SerializeField] private Image _fillImage;

        [Header("Additional")]
        [SerializeField] protected bool _constantAnim = true;
        [SerializeField] protected float _animTime = 0.5f;
        [SerializeField] protected int _upperBound = 1;

        [Header("Filled Components")]
        [SerializeField] private GameObject _fillGroup;
        [SerializeField] private GameObject _normalGroup;

        private Tween _fillTween;
        private float _animValue = 0f;

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _fillTween?.Kill();
            _fillTween = null;
        }

        protected override void OnResourceChange(int newValue, int change)
        {
            var oldVar = _cachedValue;
            
            base.OnResourceChange(newValue, change);
            
            if (change != 0)
            {
                _animValue = oldVar;
                _fillTween = DOTween.To(
                        () => _animValue,
                        x => _animValue = x,
                        newValue, _constantAnim ? _animTime : GetTime(change, _upperBound, _animTime))
                    .SetEase(Ease.OutQuart)
                    .OnUpdate(() =>
                    {
                        var fillValue = GetFillValue(_animValue, _upperBound);
                        _fillImage.fillAmount = fillValue;

                    })
                    .OnComplete(() =>
                    {
                        _fillTween = null;
                        ResolveBarUpdateDone();
                    })
                    .Play();
            }
            else
            {
                _animValue = newValue;
                var fillValue = GetFillValue(_animValue, _upperBound);
                _fillImage.fillAmount = fillValue;
                ResolveBarUpdateDone();
            }
        }

        protected override string FormatNumber(int value)
        {
            return $"{value}/{_upperBound}";
        }

        private void ResolveBarUpdateDone()
        {
            var filled = _cachedValue >= _upperBound;
            
            _normalGroup.SetActive(!filled);
            _fillGroup.SetActive(filled);
            
            OnBarUpdateDone();
        }

        protected virtual void OnBarUpdateDone()
        {
            // Do nothing
        }

        private static float GetFillValue(float value, float upperBound)
        {
            return NumberUtilities.Clamp01(NumberUtilities.InverseLinearLerp(value, 0, upperBound));
        }

        private static float GetTime(float range, float max, float maxTime)
        {
            return System.Math.Abs(range) * (maxTime / max);
        }
    }
}