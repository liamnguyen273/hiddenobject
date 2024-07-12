using com.brg.Common.Localization;
using DG.Tweening;
using JSAM;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class DynamicSticker : MonoBehaviour
    {
        [Header("Sticker")]
        [SerializeField] private RectTransform _appearanceRect;
        [SerializeField] private Image _appearanceImage;
        
        // [Header("Numbering")]
        // [SerializeField] private RectTransform _numbering;
        // [SerializeField] private TextLocalizer _number;

        [Header("Params")]
        [SerializeField] private float _baseMovementUnit = 1000f;
        [SerializeField] private float _thresholdRadius = 0.5f;

        [Header("Params - Timings")]
        [SerializeField] private float _moveBackTime = 0.7f;
        [SerializeField] private float _pickupTime = 0.1f;
        [SerializeField] private float _shiftTime = 0.35f;
        [SerializeField] private float _spawnTime = 0.35f;
        [SerializeField] private float _moveFinalizeTime = 0.3f;

        private StickerDefinition _definition;
        private StaticSticker _staticSticker;
        
        // Movement book-keeping
        private Vector3 _offset;
        private Vector3 _originalPosition;

        private bool _manualInteractable;
        private bool _dragging = false;
        private bool _dragByPlayer = false;
        
        private Tween _moveFinalizeTween;
        private Tween _spawnTween;
        private Tween _pickupTween;
        private Tween _shiftTween;

        public bool LogicalInteractable => 
            gameObject.activeSelf
            && _moveFinalizeTween == null 
            && _spawnTween == null 
            && _shiftTween == null
            && _staticSticker != null    // Cannot move if there is no sticker linked
            && _manualInteractable;
        
        public bool ManualInteractable
        {
            get => _manualInteractable;
            set => _manualInteractable = value;
        }

        public bool Dragging => _dragging;

        public bool CanShowHint => LogicalInteractable;

        public bool HasLink => _staticSticker != null;

        public StaticSticker LinkedStaticSticker => _staticSticker;

        public event Action OnStickEvent;
        public event Action OnStickFailedEvent;
        public event Action OnDragStart;

        private void Awake()
        {
            _appearanceImage.sprite = null;
        }

        private void OnDisable()
        {
            
        }

        public void Link(StaticSticker staticSticker)
        {
            gameObject.SetActive(true);
            _definition = staticSticker.GetDefinition();
            _staticSticker = staticSticker;
            
            RefreshAppearance();
            
            if (_staticSticker != null)
            {
                _staticSticker.SetInteractable(true);
            }
            
            ManualInteractable = true;
        }

        public void ResetSticker()
        {
            _appearanceImage.sprite = null;
            
            if (_staticSticker != null)
            {
                _staticSticker.SetInteractable(false);
            }
            
            _staticSticker = null;
            
            gameObject.SetActive(false);
        }

        public StaticSticker GetLinkedStaticSticker()
        {
            return _staticSticker;
        }

        public Sprite GetSprite()
        {
            return _appearanceImage.sprite;
        }

        public void SetAppearanceToFit()
        {
            var vScale = GetFitAppearanceScale();
            _appearanceRect.localScale = vScale;
        }

        public Vector3 GetFitAppearanceScale()
        {
            var rect = _appearanceRect.rect;
            var width = rect.width;
            var height = rect.height;
            var size = Mathf.Max(width, height);

            var fitSize = GetComponent<RectTransform>().rect.width;
            var scale = Mathf.Min(fitSize / size, 1f);

            return new Vector3(scale, scale, 1);
        }

        public void SetBaseAppearanceSize()
        {
            _appearanceRect.localScale = Vector3.one;
        }

        public void PlaySpawn()
        {
            _spawnTween.Kill();
            _spawnTween = null;

            SetAppearanceToFit();
            transform.localScale = Vector3.zero;
            _spawnTween = transform.DOScale(1f, _spawnTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _spawnTween = null;
                })
                .Play();
        }

        public void GoToTarget()
        {
            StickToTarget();
        }

        public void ShiftTo(Transform parent, bool immediately = false)
        {
            _shiftTween?.Kill();

            transform.SetParent(parent, true);
            
            if (immediately)
            {
                transform.localPosition = Vector3.zero;
                return;
            }
            
            _shiftTween = transform
                .DOLocalMove(Vector3.zero, _shiftTime)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _shiftTween = null;
                })
                .Play();
        }

        public void OnBeginDrag()
        {
            if (BeginDragHelper())
            {
                _dragByPlayer = true;
            }
        }

        public bool SimulateBeginDrag(bool byPlayer)
        {
            _dragByPlayer = byPlayer;
            var dragging = BeginDragHelper();

            return dragging;
        }

        public void SimulateEndDrag(bool byPlayer, bool forceStickToTarget)
        {
            // StickToTarget();
            // if (!_dragging || (_dragByPlayer != byPlayer) || !LogicalInteractable) return;
            //
            // if (forceStickToTarget || CheckAlignTarget())
            // {
            //     StickToTarget();
            // }
            // else
            // {
            //     MoveBackToOriginalPosition();
            // }
        }

        private bool BeginDragHelper()
        {
            if (!LogicalInteractable || Dragging) return false;

            // _originalPosition = transform.position;
            // _offset = _originalPosition - eventData.pointerCurrentRaycast.worldPosition;
            // _offset.z = 0;

            // Make pick up tween
            _pickupTween?.Kill();
            _pickupTween = _appearanceRect.DOScale(Vector3.one, _pickupTime)
                .SetEase(Ease.OutSine)
                .OnComplete(() => _pickupTween = null)
                .Play();

            _dragging = true;
            OnDragStart?.Invoke();

            // GetComponent<Image>().raycastTarget = !_dragByPlayer;

            return true;
        }

        public void OnEndDrag()
        {
            if (!_dragging || !_dragByPlayer || !LogicalInteractable) return;
            
            if (CheckAlignTarget())
            {
                StickToTarget();
            }
            else
            {
                MoveBackToOriginalPosition();
            }
        }

        public void ForceDrop()
        {
            if (!_dragging) return;
            
            MoveBackToOriginalPosition();
        }
        
        private void RefreshAppearance()
        {
            _appearanceImage.sprite = _definition.Sprite;
            // _appearanceImage.SetNativeSize();
            // _numbering.anchoredPosition = _definition.AbsoluteNumberingPosition;
            // _number.RawString = _definition.Number.ToString();
        }

        private bool CheckAlignTarget()
        {            
            var pos = transform.position;
            var tarPos = _staticSticker.transform.position;
            return GM.Instance.MainGame.CheckStickerTarget(pos, tarPos, _thresholdRadius);
        }

        private void MoveBackToOriginalPosition()
        {
            _moveFinalizeTween?.Kill();
            _moveFinalizeTween = null;
            
            _pickupTween?.Kill();
            _pickupTween = null;

            var distance = Vector2.Distance(transform.localPosition, Vector3.zero);
            var time = _moveBackTime * (distance / _baseMovementUnit);

            _moveFinalizeTween = DOTween.Sequence()
                .Insert(0f, transform.DOLocalMove(Vector3.zero, time).SetEase(Ease.OutExpo))
                .Insert(0f, _appearanceRect.DOScale(GetFitAppearanceScale(), time).SetEase(Ease.InExpo))
                .OnComplete(() =>
                {
                    _moveFinalizeTween = null;
                    OnStickFailedEvent?.Invoke();
                })
                .Play();
            
            // GetComponent<Image>().raycastTarget = true;
            _dragging = false;
        }

        private void StickToTarget()
        {
            _pickupTween?.Kill();
            _pickupTween = null;

            AudioManager.PlaySound(LibrarySounds.Sticker);
            
            GM.Instance.Vibrate();
            // GM.Instance.MainGame.OnStickerStickToTarget(this, _dragByPlayer);
            
            // GetComponent<Image>().raycastTarget = true;
            
            _dragging = false;
            OnStickEvent?.Invoke();
        }
    }
}