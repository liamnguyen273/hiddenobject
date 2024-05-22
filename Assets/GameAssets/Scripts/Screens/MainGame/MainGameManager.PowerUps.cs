using System.Linq;
using com.brg.Utilities;
using com.tinycastle.StickerBooker.Effects;
using DG.Tweening;
using LeTai.TrueShadow;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager
    {
        [Header("Powerup")]
        [SerializeField] private float _baseMovementUnit = 1f;
        [SerializeField] private float _lookupMoveTime = 1f;
        [SerializeField] private CompassPowerUp _compass;
        
        private bool _powerUpActive = false;
        private DynamicSticker _currentSearchDynamicSticker = null;

        private float _powerUpTimer;

        private Vector2 _scrollRangeBottomLeft;
        private Vector2 _scrollRangeTopRight;
        
        private void UpdatePowerup(float dt)
        {
            if (!_powerUpActive) return;

            _powerUpTimer -= dt;

            if (_compass.Active)
            {
                _compass.UpdateTime(_powerUpTimer);
            }

            if (_powerUpTimer <= 0f)
            {
                ResolvePowerupTimeout();
            }
        }
        
        public void RequestCompassPowerup()
        {
            if (GM.Instance.Player.GetResource(GlobalConstants.POWER_COMPASS) == 0)
            {
                var request = new AdRequest(AdManager.TYPE_REWARD, () =>
                {
                    var popup = GM.Instance.Popups.GetPopup<PopupBehaviourExtraPowerUps>(out var behaviour);
                    behaviour.Setup(GlobalConstants.POWER_COMPASS);
                    popup.Show();
                }, () =>
                {
                    GM.Instance.Popups.GetPopup(PopupNames.ERROR).Show();
                }, arbitraryData: new []
                {
                    ("level", typeof(string), _currentId as object),
                    ("reward_type",typeof(string), "compass" as object)
                });
                
                GM.Instance.Ad.RequestAd(request);
                
                return;
            }
            
            if (_powerUpActive)
            {
                Log.Warn("A powerup is already active, cannot perform another.");
                return;
            }

            if (GM.Instance.Player.UseResource(GlobalConstants.POWER_COMPASS, 1))
            {
                PerformCompassHint();
            }
        }
        
        public void RequestSearchPowerup()
        {
            if (GM.Instance.Player.GetResource(GlobalConstants.POWER_LOOKUP) == 0)
            {
                var request = new AdRequest(AdManager.TYPE_REWARD, () =>
                {
                    var popup = GM.Instance.Popups.GetPopup<PopupBehaviourExtraPowerUps>(out var behaviour);
                    behaviour.Setup(GlobalConstants.POWER_LOOKUP);
                    popup.Show();
                }, () =>
                {
                    GM.Instance.Popups.GetPopup(PopupNames.ERROR).Show();
                }, arbitraryData: new []
                {
                    ("level", typeof(string), _currentId as object),
                    ("reward_type",typeof(string), "lookup" as object)
                });
                
                GM.Instance.Ad.RequestAd(request);
                
                return;
            }
            
            if (_powerUpActive)
            {
                Log.Warn("A powerup is already active, cannot perform another.");
                return;
            }
            
            DynamicSticker dynamicSticker = null;

            foreach (var candidate in _dynamicStickers)
            {
                if (candidate != null && candidate.CanShowHint)
                {
                    dynamicSticker = candidate;
                    break;
                }
            }

            if (dynamicSticker != null && GM.Instance.Player.UseResource(GlobalConstants.POWER_LOOKUP, 1))
            {
                PerformSearchHint(dynamicSticker);
            }
        }

        private void ResolvePowerupOnPlayerStick(DynamicSticker dynamicSticker)
        {
            if (_powerUpActive)
            {
                if (dynamicSticker == _currentSearchDynamicSticker)
                {
                    ConcludeSearch();
                    ConcludePowerupSession();
                    _powerUpActive = false;
                }
            }
        }

        private void ResolvePowerupTimeout()
        {
            if (_powerUpActive)
            {
                if (_compass.Active)
                {
                    ConcludeCompass();
                }
                else if (_currentSearchDynamicSticker != null)
                {
                    ConcludeSearch();
                }

                ConcludePowerupSession();
                _powerUpActive = false;
            }
        }
        
        private void PerformSearchHint(DynamicSticker sticker)
        {
            if (sticker == null || !sticker.HasLink) return;
            _currentSearchDynamicSticker = sticker;

            Canvas.ForceUpdateCanvases();
            
            var viewportRect = _contentScroll.viewport.rect;
            var contentRect = _contentScroll.content.rect;
            var marginWidth = viewportRect.width / 2;
            var marginHeight = viewportRect.height / 2;
            var contentWidth = contentRect.width;
            var contentHeight = contentRect.height;

            var boundX = contentWidth - marginWidth;
            var boundY = contentHeight - marginHeight;

            var target = sticker.GetLinkedStaticSticker();
            target.StartHintEffect();
            var itemPos = target.transform.position;

            itemPos = _contentScroll.transform.InverseTransformPoint(itemPos);
            itemPos.x = NumberUtilities.Clamp(itemPos.x, -boundX, boundX);
            itemPos.y = NumberUtilities.Clamp(itemPos.y, -boundY, boundY);
            
            var centerPos = _contentScroll.transform.position;
            var offset = -itemPos;
            offset.z = 0;
            
            var distance = offset.magnitude;
            var time = _lookupMoveTime * (distance / _baseMovementUnit);

            _mapParent.DOBlendableLocalMoveBy(offset, time)
                .SetEase(Ease.OutQuart)
                .Play();
            
            _powerUpActive = true;
            _powerUpTimer = 10f;
        }

        private void PerformCompassHint()
        {
            _powerUpActive = true;
            _powerUpTimer = 15f;
            
            _compass.Activate(_powerUpTimer);
            SwitchCompassTarget();
        }

        private void SwitchCompassTarget()
        {
            if (!_powerUpActive) return;

            var first = _dynamicStickers.FirstOrDefault(x => x.HasLink);
            _compass.SetTarget(first != null ? first.GetLinkedStaticSticker().transform : null);
        }

        private void ConcludeCompass()
        {
            _compass.Deactivate();
        }

        private void ConcludeSearch()
        {
            _currentSearchDynamicSticker?.GetLinkedStaticSticker()?.EndHintEffect();
        }

        private void ConcludePowerupSession()
        {
            _powerUpActive = false;
            _currentSearchDynamicSticker = null;
            _compass.Deactivate();  // just in case
        }

        private void CalculateScrollRange()
        {
            Canvas.ForceUpdateCanvases();
            
            var viewport= _contentScroll.viewport;
            var content = _contentScroll.content;
            var viewportCorners = new Vector3[4];
            var contentCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);
            content.GetWorldCorners(contentCorners);
            var viewportBottomLeft = viewportCorners[0];
            var viewportTopRight = viewportCorners[2];
            var contentBottomLeft = contentCorners[0];
            var contentTopRight = contentCorners[2];
            var viewportW2 = (viewportTopRight.x - viewportBottomLeft.x) / 2;
            var viewportH2 = (viewportTopRight.y - viewportBottomLeft.y) / 2;
            var contentW2 = (contentTopRight.x - contentBottomLeft.x) / 2;
            var contentH2 = (contentTopRight.y - contentBottomLeft.y) / 2;

            _scrollRangeBottomLeft = new Vector2(-(contentW2 - viewportW2), -(contentH2 - viewportH2));
            _scrollRangeTopRight = new Vector2(contentW2 - viewportW2, contentH2 - viewportH2);
        }
    }
}