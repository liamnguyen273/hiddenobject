using com.brg.Common.UI;
using System;
using com.brg.Common.Localization;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourAsk : UIPopupBehaviour
    {
        private Action _yesAction;
        private Action _noAction;

        [SerializeField] private UIButton _yesButton;
        [SerializeField] private UIButton _noButton;
        [SerializeField] private TextLocalizer _title;
        [SerializeField] private TextLocalizer _content;

        internal override void Initialize()
        {
            base.Initialize();

            _yesButton.Event.FunctionalEvent += OnYesButton;
            _noButton.Event.FunctionalEvent += OnNoButton;
        }

        public void SetQuestion(string title, string content, Action onYes, Action onNo)
        {
            _title.RawString = title;
            _content.RawString = content;
            _yesAction = onYes;
            _noAction = onNo;
        }

        public void OnYesButton()
        {
            Popup.Hide();
            _yesAction?.Invoke();
        }

        public void OnNoButton()
        {
            Popup.Hide();
            _noAction?.Invoke();
        }
    }
}