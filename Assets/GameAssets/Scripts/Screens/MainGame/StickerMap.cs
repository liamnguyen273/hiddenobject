using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.Common.Logging;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public partial class StickerMap : MonoManagerBase
    {
        [Header("Sticker things")] 
        [SerializeField] private Transform _unusedStickerHost;
        [SerializeField] private Transform _inUseStickerHost;
        [SerializeField] private GameObject _staticStickerPrefab;
        [SerializeField] private Image _hinter;
        
        [FormerlySerializedAs("_content")]
        [Header("Full image things")] 
        [SerializeField] private RectTransform _scrollContent;
        // [SerializeField] private RectTransform _fullMapGroup;
        [SerializeField] private ParticleSystem _fullMapParticleSystem;
        [SerializeField] private Image _outlineImage;
        // [SerializeField] private Image _fullMapImage;

        [Header("Params")] 
        [SerializeField] private float _contentPadX = 252f;
        [SerializeField] private float _contentPadY = 402f;
        [SerializeField] private float _fillTime = 10f;
        [SerializeField] private int _poolCount = 60;

        private HashSet<StaticSticker> _stickerPool;
        private HashSet<StaticSticker> _inUseStickers;

        private Dictionary<int, StaticSticker> _allStickers;

        private Tween _fillTween;

        private float _mapSizeX;
        private float _mapSizeY;

        public void SetMap(Sprite thumbnail, Sprite fullImageSprite, Dictionary<int, StickerDefinition> defs)
        {
            _mapSizeX = fullImageSprite.bounds.extents.x * 200;
            _mapSizeY = fullImageSprite.bounds.extents.y * 200;
            
            LogObj.Default.Info("Map", $"Map {fullImageSprite.name} has content " +
                                       $"size {_mapSizeX}x{_mapSizeY} and pad {_mapSizeX + _contentPadX}x{_mapSizeY + _contentPadY}");
            
            _scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _mapSizeX + _contentPadX);
            _scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mapSizeY + _contentPadY);
            
            _outlineImage.sprite = fullImageSprite;
            // _fullMapImage.sprite = fullImageSprite;
            transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _mapSizeX);
            transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mapSizeY);

            // var rect = _fullMapImage.GetComponent<RectTransform>().rect;
            // var vec2 = new Vector2(rect.width, rect.height) / 2;

            var shape = _fullMapParticleSystem.shape;
            shape.radius = _mapSizeX / 2;

            var vec2 = new Vector2(_mapSizeX, _mapSizeX) / 2;
            _allStickers = new Dictionary<int, StaticSticker>();

            foreach (var (number, def) in defs)
            {
                var sticker = GetStaticSticker();
                sticker.SetSticker(vec2, def);
                _allStickers[number] = sticker;
            }
            
            ShowFullMap();
        }

        public void Restart()
        {
            // Hide full map
            HideFullMap();
            _fullMapParticleSystem.Stop();
        }

        public void CleanUp()
        {
            // Remove all stickers
            foreach (var (number, sticker) in _allStickers)
            {
                sticker.ResetSticker();
                ReturnStaticSticker(sticker);
            }
            
            // Hide full map
            HideFullMap();
            _fullMapParticleSystem.Stop();
            
            // Remove sprites
            _outlineImage.sprite = null;
            // _fullMapImage.sprite = null;
            
            _allStickers = null;

            _fillTween?.Kill();
        }

        public List<StaticSticker> GetStickerList()
        {
            return _allStickers
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();
        }

        public void AnimateFullMap(Action onComplete)
        {
            onComplete?.Invoke();
            // if (_fillTween != null)
            // {
            //     _fillTween.Kill();
            //     _fillTween = null;
            // }
            //
            // _fillTween = DOTween.To(() => _fullMapGroup.rect.height,
            //         (x) => { _fullMapGroup.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, x); }, _mapSizeY,
            //         _fillTime)
            //     .SetEase(Ease.Linear)
            //     .OnStart(() => { _fullMapParticleSystem.Play(); })
            //     .OnComplete(() =>
            //     {
            //         _fullMapParticleSystem.Stop();
            //         _fillTween = null;
            //         onComplete?.Invoke();
            //     });
        }

        public Image GetHinterImage()
        {
            return _hinter;
        }

        public void ShowFullMap()
        {
            // _fullMapGroup.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mapSizeY);
        }

        public void HideFullMap()
        {
            // _fullMapGroup.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }
    }
}