using System;
using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Common.UI;
using com.brg.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class LevelSelectorItem : MonoBehaviour
    {
        public enum State
        {
            UNPLAYABLE,
            PLAYABLE,
            LOCKED
        }

        [Header("Non-playable group")]
        [SerializeField] private Transform _nonPlayableGroup;
        
        [Header("Playable group")]
        [SerializeField] private Transform _playableGroup;
        [SerializeField] private TextLocalizer _levelNumber;
        [SerializeField] private TextLocalizer _levelName;
        [SerializeField] private Image _picture;
        [SerializeField] private Transform _loading;

        [Header("Locked group")] 
        [SerializeField] private Transform _lockedGroup;
        [SerializeField] private TextLocalizer _lockText;
        
        [Header("Complete tag")] 
        [SerializeField] private GameObject _completeTag;
        
        [Header("Progress tag")] 
        [SerializeField] private GameObject _progressTag;
        [SerializeField] private TextLocalizer _progressText;
        
        [Header("Buttons")] 
        [SerializeField] private UIButton _button;
        
        protected State _state;
        protected LevelEntry _entry = null;
        protected bool _completed = false;
        protected bool _hasProgress = false;
        protected LevelProgress _levelProgress;
        protected LevelAssetHandle _assetHandle;
        
        private bool _thumbnailWait = false;

        private void LateUpdate()
        {
            if (_entry != null && _thumbnailWait)
            {
                if (_completed && _assetHandle.FullImageLoaded)
                {
                    _picture.sprite = _assetHandle.FullSprite;
                    OnThumbnailWaitDone();
                }
                else if (!_completed && _assetHandle.ThumbnailLoaded)
                {
                    _picture.sprite = _assetHandle.ThumbnailSprite;
                    OnThumbnailWaitDone();
                }
            }
        }

        public void ActivateWith(LevelEntry entry, bool completed, bool hasProgress, LevelProgress progress)
        {
            gameObject.SetActive(true);
            
            _entry = entry;
            _completed = completed;
            _hasProgress = hasProgress;
            _levelProgress = progress;
            
            Refresh();
        }

        public void Deactivate()
        {
            _entry = null;
            _thumbnailWait = false;
            gameObject.SetActive(false);
        }

        protected virtual void Refresh()
        {
            if (_entry == null) return;
            
            var unlocked = GM.Instance.ResolveUnlockCondition(_entry);
            _state = !_entry.EvaluatedPlayable ? State.UNPLAYABLE : (unlocked ? State.PLAYABLE : State.LOCKED);

            _nonPlayableGroup.SetGOActive(_state == State.UNPLAYABLE);
            _playableGroup.SetGOActive(_state != State.UNPLAYABLE);
            
            _lockedGroup.SetGOActive(_state == State.LOCKED);
            
            // Set unlock condition
            if (_state == State.LOCKED && _lockText != null)
            {
                _lockText.RawString = _entry.GetShowUnlockCondition();
            }
            
            // Number is always available
            _levelNumber.RawString = _entry.SortOrder.ToString();
            
            if (_state != State.UNPLAYABLE)
            {
                _levelName.RawString = _entry.DisplayName;
                
                if (_entry.TotalStickerCount == 0)
                {
                    LogObj.Default.Warn($"Level entry \"{_entry.Id}\"has 0 sticker count, please check.");
                    _progressTag.SetActive(false);
                }
                else
                {
                    var count = _levelProgress.AttachedStickers?.Length ?? 0;
                    var total = _entry.TotalStickerCount;
                    _progressText.RawString = $"{((float)count/total * 100):00}%";
                    _progressTag.SetActive(_hasProgress && count > 0);
                }
                
                if (!_entry.IsMultiplayer)
                {
                    _picture.sprite = null;
                    InitializeAssetLoad();
                }
            }
            
            _completeTag.SetActive(_state == State.PLAYABLE && !_entry.IsMultiplayer && _completed);
            
            // Set button
            _button.Interactable = GM.Instance.IsCheat || _state == State.PLAYABLE;
        }
                
        public void OnPlayButton()
        {
            PlayButtonBehaviour();
        }

        protected virtual void PlayButtonBehaviour()
        {
            if (_entry != null && _entry.EvaluatedPlayable)
            {
                GM.Instance.RequestPlayLevel(_entry.Id);
            }
        }

        private void InitializeAssetLoad()
        {
            _thumbnailWait = true;

            var success = GM.Instance.Data.RequestLoadAssetPack(
                _entry.Id, out _assetHandle, true, true, false);
            
            if (!success)
            {
                _thumbnailWait = false;
            }
            
            // TODO: Load image
            _loading.SetGOActive(_thumbnailWait);
        }

        private void OnThumbnailWaitDone()
        {
            _thumbnailWait = false;
            
            // TODO: Load image
            _loading.SetGOActive(false);
        }
    }
}