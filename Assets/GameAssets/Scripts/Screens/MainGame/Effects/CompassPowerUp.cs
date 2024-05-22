using System;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker.Effects
{
    public class CompassPowerUp : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Transform _rotator;
        [SerializeField] private TextLocalizer _timeText;
        [SerializeField] private float _minimumAlpha = 0.1f;
        [SerializeField] private float _closeUpRadius = 2f;

        private Tween _tween;
        private Transform _target;

        public bool Active => gameObject.activeInHierarchy;

        private void LateUpdate()
        {
            UpdateAppearance();
            UpdateAlpha();
        }

        private void OnDisable()
        {
            _tween?.Kill();
            _tween = null;
        }

        public void Activate(float initialTime)
        {
            _target = null;
            
            gameObject.SetActive(true);
            UpdateTime(initialTime);

            UpdateAppearance();
            UpdateAlpha();
            
            transform.localScale = Vector3.zero;
            
            _tween?.Kill();
            _tween = transform.DOScale(1f, 0.75f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _tween = null)
                .Play();
        }

        public void SetTarget(Transform target)
        {
            if (!gameObject.activeInHierarchy) return;
            _target = target;
            UpdateAppearance();
        }

        public void Deactivate()
        {
            if (!Active) return;
            
            _tween?.Kill();
            _tween = transform.DOScale(0f, 0.75f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _tween = null;
                    _target = null;
                    gameObject.SetActive(false);
                })
                .Play();
        }
        
        public void UpdateTime(float time)
        {
            _timeText.RawString = FormatTime(time);
        }

        private string FormatTime(float timeInSeconds)
        {
            return $"{((int)Math.Ceiling(timeInSeconds)):00}";
        }

        private void UpdateAppearance()
        {
            if (_target == null)
            {
                _rotator.gameObject.SetActive(false);
                return;
            }
            
            _rotator.gameObject.SetActive(true);
            
            var pos = _rotator.position;
            var target = _target.position;
            var offset = (Vector2)(target - pos);
            var rotate = offset.DegRotation();
            
            _rotator.rotation = Quaternion.Euler(0f, 0f, rotate); 
        }

        private void UpdateAlpha()
        {
            var dist = _target == null ? _minimumAlpha : ((Vector2)(transform.position - _target.position)).magnitude;
            var alpha = NumberUtilities.Clamp01(NumberUtilities.InverseLinearLerp(dist, 0f, _closeUpRadius));
            _group.alpha = Mathf.Max(_minimumAlpha, alpha);
        }
    }
}