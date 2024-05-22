using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Random;
using com.brg.Utilities;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace com.tinycastle.StickerBooker
{
    [ExecuteInEditMode]
    public class PreviewMap : MonoBehaviour
    {
        [InspectorButton("ShowOutline")]
        public bool outline;          
        [InspectorButton("ShowColored")]
        public bool colored;          
        [InspectorButton("ShowFaded")]
        public bool faded;  
        [InspectorButton("Randomize")]
        public bool randomize;          
        
        [SerializeField] private Image _whiteBackground;
        [SerializeField] private Image _fullImage;
        [SerializeField] private Transform _stickerHost;

        private List<PreviewSticker> _stickers;
        private bool _hasMap = false;
        private string _levelName = null;

        public string MapName => _levelName;
        public bool HasMap => _hasMap;

        private void Update()
        {
            _whiteBackground.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _fullImage.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _stickerHost.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        public void SetPreviewMap(string levelName, Sprite map)
        {
            _levelName = levelName;
            _fullImage.sprite = map;
            _fullImage.SetNativeSize();
            _whiteBackground.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                _fullImage.GetComponent<RectTransform>().rect.width);
            _whiteBackground.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                _fullImage.GetComponent<RectTransform>().rect.height);
            _stickers = new List<PreviewSticker>();
            _hasMap = true;
            
            ShowFaded();
        }

        public void AddSticker(PreviewSticker sticker, StickerDefinition definition, Vector2 halfDimension)
        {
            sticker.HalfDimension = halfDimension;
            sticker.transform.SetParent(_stickerHost);
            
            sticker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            sticker.GetComponent<RectTransform>().localScale = Vector2.one;
            sticker.SetSticker(definition);
            
            _stickers.Add(sticker);
        }

        public List<PreviewSticker> GetOrderedStickers()
        {
            var list = _stickers.OrderBy(x => x.Number)
                .ToList();
            return list;
        }

        public void Clean()
        {
            _fullImage.sprite = null;
            
            _stickerHost.DeleteAllChildrenImmediately();

            _stickers = null;
            _hasMap = false;
        }

        private void Randomize()
        {
            if (!_hasMap)
            {
                Debug.Log("Empty, cannot randomize.");
            }

            var rng = RandomEngineFactory.CreateEngine(Engine.STANDARD, Environment.TickCount);
            _stickers.Shuffle(rng);

            foreach (var sticker in _stickers)
            {
                sticker.transform.SetAsLastSibling();
            }
            
            foreach (var sticker in _stickers)
            {
                sticker.Renumber(sticker.transform.GetSiblingIndex() + 1);
            }
        }
        
        private void ShowOutline()
        {
            _fullImage.SetGOActive(false);
            ToggleAllStickersOutline();
        }
        
        private void ShowColored()
        {
            _fullImage.SetGOActive(true);
            _fullImage.GetComponent<CanvasGroup>().alpha = 1f;
            ToggleAllStickersColored();
        }        
        
        private void ShowFaded()
        {
            _fullImage.SetGOActive(true);
            _fullImage.GetComponent<CanvasGroup>().alpha = 0.6f;
            ToggleAllStickersColored();
        }

        private void ToggleAllStickersOutline()
        {
            foreach (var sticker in _stickers)
            {
                sticker.ToggleOutline();
            }
        }
        
        private void ToggleAllStickersColored()
        {
            foreach (var sticker in _stickers)
            {
                sticker.ToggleColor();
            }
        }
    }
}