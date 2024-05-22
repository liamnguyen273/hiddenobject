using System;
using com.brg.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    [ExecuteInEditMode]
    public class PreviewSticker : MonoBehaviour
    {
       
        [SerializeField] private RectTransform _outlineStickerRect;
        [SerializeField] private Image _outlineSticker;
        
        [SerializeField] private RectTransform _stickerRect;
        [SerializeField] private Image _sticker;
        [SerializeField] private RectTransform _numbering;
        [SerializeField] private TMP_Text _numberText;

        private string _stickerName;
        private int _number;
        private Vector2 _pos;
        
        public string Name => _stickerName;
        public int Number => _number;

        public Vector2 HalfDimension { get; set; }

        private void LateUpdate()
        {
            _outlineStickerRect.anchoredPosition = Vector2.zero;
            _stickerRect.anchoredPosition = Vector2.zero;
            _numberText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Renumber(transform.GetSiblingIndex() + 1);
        }

        public void SetSticker(StickerDefinition def)
        {
            _stickerName = def.Name;
            _number = def.Number;
            
            _sticker.sprite = def.Sprite;
            _outlineSticker.sprite = def.Sprite;
            
            _sticker.SetNativeSize();
            _outlineSticker.SetNativeSize();
            
            _sticker.transform.localScale = Vector3.one;
            _outlineSticker.transform.localScale = Vector3.one;
            
            _stickerRect.anchoredPosition = Vector3.zero;
            _outlineStickerRect.anchoredPosition = Vector2.zero;
            
            _pos = new Vector2(def.NormalizedPosition.x * HalfDimension.x,
                def.NormalizedPosition.y * HalfDimension.y);

            gameObject.GetComponent<RectTransform>().anchoredPosition = _pos;
                
            _numbering.anchoredPosition = def.AbsoluteNumberingPosition;

            name = $"Sticker {def.Number:00} [{def.Name}]";
            _numberText.text = def.Number.ToString();
        }

        public void Renumber(int newNumber)
        {
            _number = newNumber;
            name = $"Sticker {_number:00} [{Name}]";
            _numberText.text = newNumber.ToString();
        }

        public void ToggleColor()
        {
            _outlineSticker.SetGOActive(false);
            _sticker.SetGOActive(true);
        }

        public void ToggleOutline()
        {
            _outlineSticker.SetGOActive(true);
            _sticker.SetGOActive(false);
        }

        public Vector2 ExportStickerNumberingPosition()
        {
            _sticker.SetNativeSize();
            _sticker.transform.localScale = Vector3.one;
            var numberAnchor = _numbering.anchoredPosition;
            return new Vector2(numberAnchor.x, numberAnchor.y);
        }        
        
        public Vector2 ExportStickerPosition()
        {
            _sticker.SetNativeSize();
            _sticker.transform.localScale = Vector3.one;
            var stickerAnchor = gameObject.GetComponent<RectTransform>().anchoredPosition;
            return new Vector2(stickerAnchor.x / HalfDimension.x , stickerAnchor.y / HalfDimension.y);
        }
    }
}