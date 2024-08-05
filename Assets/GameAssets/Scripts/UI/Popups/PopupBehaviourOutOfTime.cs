using com.brg.Common.UI;
using System;
using com.brg.Common.Localization;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourOutOfTime : UIPopupBehaviour
    {
        [SerializeField] private UIButton _yesButton;
        [SerializeField] private UIButton _noButton;
        [SerializeField] private UIButton _retryButton;

        private Action _onYes;
        private Action _onNo;
        private Action _retryAction;
        
        public void SetOnYesCallback(Action onYes, Action retry)
        {
            _onYes = onYes;
            _retryAction = retry;
        }
        
        public void SetOnNoCallback(Action onNo)
        {
            _onNo = onNo;
        }

        protected override void InnateOnHideEnd()
        {
            _onYes = null;
            _onNo = null;
            _retryButton = null;
            base.InnateOnHideEnd();
        }

        internal override void Initialize()
        {
            base.Initialize();

            _yesButton.Event.FunctionalEvent += OnYesButton;
            _noButton.Event.FunctionalEvent += OnNoButton;
            _retryButton.Event.FunctionalEvent += OnRetryButton;
        }

        public void OnYesButton()
        {
            GM.Instance.Ad.RequestAd(new AdRequest(AdManager.TYPE_REWARD, () =>
            {
                _onYes?.Invoke();
                Popup.Hide();
            }, () =>
            {
                GM.Instance.Popups.GetPopup(PopupNames.ERROR).Show();
            }));   
        }

        public void OnNoButton()
        {
            Popup.Hide();
            GM.Instance.MainGame.RequestGoHome();
        }

        public void OnRetryButton()
        {
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourAsk>(out var behaviour);
            behaviour.SetQuestion("Retry level", "Are you sure want to retry the level?", OnYes, OnNo);
            popup.Show();
        }

        private void OnYes()
        {
            _retryAction?.Invoke();
            Popup.Hide();
        }
        
        private void OnNo()
        {
            
        }
    }
}