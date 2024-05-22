using com.brg.Common.Localization;
using com.brg.Common.UI;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public enum WinMode
    {
        NORMAL = 0,
        TIME_ATTACK = 1,
    }
    
    public class PopupBehaviourWin : UIPopupBehaviour
    {
        [SerializeField] private Image _levelImage;
        [SerializeField] private Transform _completeTag;
        [SerializeField] private Transform _failedTag;
        [SerializeField] private TextLocalizer _failedText;
        [SerializeField] private TextLocalizer _nextLevelText;
        [SerializeField] private TextLocalizer _comingSoonText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextLocalizer _continueButtonText;
        [SerializeField] private Animator _fireworkLeft;
        [SerializeField] private Animator _fireworkRight;

        private LevelEntry _currentEntry;
        private LevelEntry _nextEntry;
        private bool _thumbnailWait = false;
        private LevelAssetHandle _handle;

        private Tween _completeTagTween;

        private WinMode _winMode = WinMode.NORMAL;
        private bool _isWin = true;
        
        private void LateUpdate()
        {
            if (_currentEntry != null && _thumbnailWait && _handle != null && _handle.FullImageLoaded)
            {
                _thumbnailWait = false;
                _levelImage.sprite = _handle.FullSprite;
            }
        }

        public void SetWinMode(WinMode mode, bool isWin)
        {
            _winMode = mode;
            _isWin = isWin;
        }

        protected override void InnateOnShowStart()
        {
            Refresh();
            
            base.InnateOnShowStart();
        }

        protected override void InnateOnShowEnd()
        {
            if (_isWin)
            {
                _fireworkLeft.Play("fire");
                _fireworkRight.Play("fire");
            }

            AudioManager.StopSound(LibrarySounds.Congrats, stopInstantly: true);
            
            base.InnateOnShowEnd();
        }

        protected override void InnateOnHideStart()
        {
            _thumbnailWait = false;
            _handle = null;
            _currentEntry = null;
            _nextEntry = null;
            
            base.InnateOnHideStart();
        }

        protected override void InnateOnHideEnd()
        {
            _fireworkLeft.Play("idle");
            _fireworkRight.Play("idle");

            _levelImage.sprite = null;
            
            _completeTagTween?.Kill();
            _completeTagTween = null;
            
            base.InnateOnHideEnd();

            _winMode = WinMode.NORMAL;
            _isWin = true;
        }

        public void Setup(string currentLevel)
        {
            var entry = GM.Instance.Data.GetLevelEntry(currentLevel);
            var nextEntry = GM.Instance.Data.GetNextEntry(currentLevel);

            _currentEntry = entry;
            _nextEntry = nextEntry;
        }

        public void OnContinueButton()
        {
            if (_isWin)
            {            
                if (_nextEntry != null)
                {
                    GM.Instance.RequestPlayLevel(_nextEntry.Id);
                }
                else
                {
                    GM.Instance.RequestGoToMenu();
                }
            }
            else
            {
                if (_currentEntry != null)
                {
                    GM.Instance.RequestPlayLevel(_currentEntry.Id);
                }
                else
                {
                    GM.Instance.RequestGoToMenu();
                }
            }
            
            Popup.Hide();
        }

        public void OnReturnButton()
        {
            GM.Instance.RequestGoToMenu();
            Popup.Hide();
        }
        
        private void Refresh()
        {
            _completeTag.SetGOActive(_isWin);
            _failedTag.SetGOActive(!_isWin);
            PlayTagAnim(_isWin ? _completeTag : _failedTag);

            if (_isWin)
            {
                if (_nextEntry != null)
                {
                    _comingSoonText.SetGOActive(false);
                    _nextLevelText.SetGOActive(true);
                    _failedText.SetGOActive(false);
                    _continueButton.SetGOActive(true);
                    _continueButtonText.RawString = "Next Level";
                }
                else
                {
                    _comingSoonText.SetGOActive(true);
                    _nextLevelText.SetGOActive(false);
                    _failedText.SetGOActive(false);
                    _continueButton.SetGOActive(false);
                }
            }
            else
            {
                _comingSoonText.SetGOActive(false);
                _nextLevelText.SetGOActive(false);
                _failedText.SetGOActive(true);
                _continueButton.SetGOActive(true);
                _continueButtonText.RawString = "Retry Level";
            }
            
            if (_currentEntry != null)
            {
                InitializeWaitThumbnail();
                
                // Anim setup
                _completeTagTween?.Kill();
                _completeTagTween = null;

                var canvas = _completeTag.GetComponent<CanvasGroup>();
                _completeTag.localScale = Vector3.one * 2.5f;
                canvas.alpha = 0f;
            }
        }
        
        private void InitializeWaitThumbnail()
        {
            _thumbnailWait = true;

            var success = GM.Instance.Data.RequestLoadAssetPack(
                _currentEntry.Id, out _handle, true, false, false);

            if (!success)
            {
                _thumbnailWait = false;
            }
        }

        private void PlayTagAnim(Transform tag)
        {
            var canvas = tag.GetComponent<CanvasGroup>();

            _completeTagTween = DOTween.Sequence()
                .Insert(0.25f, tag.DOScale(Vector3.one, 1f)
                    .SetEase(Ease.OutQuad))
                .Insert(0.25f, canvas.DOFade(1f, 1f)
                    .SetEase(Ease.OutQuad))
                .OnComplete(() => _completeTagTween = null)
                .Play();
        }
    }
}