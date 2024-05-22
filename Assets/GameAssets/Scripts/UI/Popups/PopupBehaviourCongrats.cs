using com.brg.Common.UI;
using System;
using com.brg.Common.Localization;
using TMPro;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourCongrats : UIPopupBehaviour
    {
        [SerializeField] private TMP_Text _titleCoreText;
        [SerializeField] private TextLocalizer _titleText;
        [SerializeField] private TMP_Text _contentCoreText;
        [SerializeField] private TextLocalizer _contentText;

        private string _baseTitleText;
        private string _baseContentText;

        internal override void Initialize()
        {
            base.Initialize();
            _baseTitleText = _titleCoreText.text;
            _baseContentText = _contentCoreText.text;
        }

        public void SetContentText(string value)
        {
            _contentText.RawString = value;
        }

        public void SetTitleText(string value)
        {
            _titleText.RawString = value;
        }

        protected override void InnateOnHideEnd()
        {
            _titleText.RawString = _baseTitleText;
            _contentText.RawString = _baseContentText;
        }
    }
}