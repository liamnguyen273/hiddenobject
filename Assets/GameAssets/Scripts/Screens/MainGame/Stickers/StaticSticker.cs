using System;
using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public enum StaticStickerState
    {
        OUTLINE = 1,
        TEMP_HIDDEN = 2,
        COLORED = 3,
    }
    
    public class StaticSticker: MonoBehaviour
    {
        private const float Z_STEP = -10;
        
        // [SerializeField] private Image _outlineImage; 
        [SerializeField] private Image _filledImage; 
        [SerializeField] private CanvasGroup _canvasGroup; 
        // [SerializeField] private RectTransform _numbering; 
        // [SerializeField] private TextLocalizer _numberText;

        private RectTransform _rect;
        private StickerDefinition _cachedDef;
        private StaticStickerState _state = StaticStickerState.COLORED;
        
        private Tween _hintTween;
        private Tween _pulsedTween;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            // _outlineImage.sprite = null;
            _filledImage.sprite = null;
        }

        private void OnDisable()
        {
            _hintTween?.Kill();
            _hintTween = null;
            _pulsedTween?.Kill();
            _pulsedTween = null;
        }

        public void ResetSticker()
        {
            _rect.anchoredPosition3D = Vector3.zero;
            
            _rect.localScale = Vector3.one;
            // _outlineImage.sprite = null;
            _filledImage.sprite = null;
            SetState(StaticStickerState.OUTLINE);
            _canvasGroup.interactable = false;
            
            gameObject.SetActive(false);
        }

        public void SetSticker(Vector2 mapDimension, StickerDefinition definition)
        {
            _cachedDef = definition;

            var pos = new Vector3(definition.NormalizedPosition.x * mapDimension.x,
                definition.NormalizedPosition.y * mapDimension.y,
                Z_STEP * definition.Number);

            transform.localScale = Vector3.one;

            _rect.anchoredPosition3D = pos;
            // _outlineImage.sprite = definition.Sprite;
            // _outlineImage.SetNativeSize();
            _filledImage.sprite = definition.Sprite;
            _filledImage.SetNativeSize();
            // _numbering.anchoredPosition = definition.AbsoluteNumberingPosition;
            // _numberText.RawString = definition.Number.ToString();

            SetInteractable(false);
            
            name = $"Sticker {definition.Number:00} [{definition.Name}]";
        }

        public StickerDefinition GetDefinition()
        {
            return _cachedDef;
        }

        public void SetState(StaticStickerState state)
        {
            _state = state;

            if (_hintTween != null)
            {
                EndHintEffect();
            }
            else
            {
                switch (_state)
                {
                    case StaticStickerState.COLORED:
                        gameObject.SetActive(true);
                        // _outlineImage.SetGOActive(false);
                        _filledImage.SetGOActive(true);
                        // _numbering.SetGOActive(false);
                        break;
                    case StaticStickerState.TEMP_HIDDEN:
                        gameObject.SetActive(false);
                        break;
                    case StaticStickerState.OUTLINE:
                        gameObject.SetActive(true);
                        // _outlineImage.SetGOActive(true);
                        _filledImage.SetGOActive(false);
                        // _numbering.SetGOActive(true);
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetInteractable(bool value)
        {
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }
                
        public void StartHintEffect()
        {
            var hinter = GM.Instance.MainGame.GetHinter();
            var pos = transform.position;
            pos.z = hinter.transform.position.z;
            hinter.transform.position = pos;
            
            hinter.sprite = _cachedDef.Sprite;
            hinter.SetNativeSize();
            hinter.gameObject.SetActive(true);
            
            // Tween
            hinter.SetGOActive(true);
            var color = hinter.color;
            color.a = 0.2f;
            hinter.color = color;
            _hintTween = DOTween.Sequence()
                .Append(hinter.DOFade(0.8f, 1f).SetEase(Ease.InOutSine))
                .AppendInterval(0.5f)
                .Append(hinter.DOFade(0.2f, 1f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Restart)
                .Play();

            GM.Instance.Effects.PlayHintParticles(hinter.transform, Vector3.zero);
        }

        public void EndHintEffect()
        {
            _hintTween?.Kill();
            _hintTween = null;
            
            var hinter = GM.Instance.MainGame.GetHinter();
            hinter.sprite = null;
            hinter.gameObject.SetActive(false);
            
            GM.Instance.Effects.StopHintParticles();
            
            SetState(_state);
        }

        public void PlayPulseTween()
        {
            _pulsedTween?.Kill();
            _pulsedTween = null;

            _pulsedTween = DOTween.Sequence()
                .Append(transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutQuart))
                .Append(transform.DOScale(1f, 0.5f).SetEase(Ease.InQuart))
                .OnComplete(() =>
                {
                    _pulsedTween = null;
                }).Play();
        }

        public void OnPointerClick()
        {
            Debug.Log($"CLICKED {gameObject.name}");
            // return;
            AudioManager.PlaySound(LibrarySounds.Sticker);
            
            GM.Instance.Vibrate();
            
            GM.Instance.MainGame.OnStickerFound(this, true);
        }
    }
}