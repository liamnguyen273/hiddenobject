using com.brg.Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourExtraPowerUps : UIPopupBehaviour
    {
        [SerializeField] private Image _image;
        
        private string _type = null;
        private int _resolveCount = -1;

        protected override void InnateOnHideStart()
        {
            ResolveAddPowerUps();
            
            _type = null;
            _resolveCount = -1;
            base.InnateOnHideStart();
        }

        public void Setup(string type)
        {
            if (type == GlobalConstants.POWER_LOOKUP || type == GlobalConstants.POWER_COMPASS)
            {
                _image.sprite = GM.Instance.Data.GetResourceIcon(type);
            
                _type = type;
                _resolveCount = 1;
            }
        }
        
        public void OnSureButton()
        {
            var request = new AdRequest(AdManager.TYPE_REWARD, () =>
            {
                _resolveCount = 3;
                Popup.Hide();
            }, () =>
            {
                GM.Instance.Popups.GetPopup(PopupNames.ERROR).Show();
            });
            
            GM.Instance.Ad.RequestAd(request);
        }

        public void OnNoButton()
        {
            _resolveCount = 1;
            Popup.Hide();
        }

        private void ResolveAddPowerUps()
        {
            var type = _type;
            if (_type == GlobalConstants.POWER_LOOKUP || _type == GlobalConstants.POWER_COMPASS)
            {
                GM.Instance.Effects
                    .PlayFlyThings(transform.position, GM.Instance.Data.GetResourceIcon(type),
                        _type == GlobalConstants.POWER_LOOKUP
                            ? GM.Instance.MainGameHud.SearchPowerButtonTransform
                            : GM.Instance.MainGameHud.CompassPowerButtonTransform,
                        _resolveCount,
                    () =>
                    {
                        GM.Instance.Player.AddResource(type, 1, allowCreateEntry: true, doNotSave: false);
                    }, () =>
                    {
                        GM.Instance.Player.RequestSaveData(true, false, false);
                    }, initialFlyDist: 100);
            }
        }
    }
}