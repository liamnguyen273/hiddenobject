using com.brg.Common.UI;
using JSAM;
using System;
using com.brg.Common.Localization;
using com.brg.Utilities;
using Lean.Gui;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourSettings : UIPopupBehaviour
    {
        [SerializeField] private TextLocalizer _version;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        // [SerializeField] private LeanToggle _vibrationToggle;
        [SerializeField] private UIButton _quitButton;

        private PlayerPreference _cachedPref;
        
        protected override void InnateOnShowStart()
        {
            var pref = GM.Instance.Player.GetPreference();
            _cachedPref = pref;

            _musicSlider.value = (pref.MusicVolume / 50f);
            _sfxSlider.value = (pref.SfxVolume / 50f);

            _musicSlider.onValueChanged.AddListener(OnMusicSlide);
            _sfxSlider.onValueChanged.AddListener(OnSfxSlide);
            // _vibrationToggle.Set(pref.Vibration);

            _version.RawString = $"V{Application.version}";
            
            base.InnateOnShowStart();
        }

        protected override void InnateOnHideStart()
        {
            GM.Instance.Player.SetPreference(_cachedPref);
            base.InnateOnHideStart();
        }

        protected override void InnateOnHideEnd()
        {
            _quitButton.SetGOActive(false);
            base.InnateOnHideEnd();
        }

        public void ShowQuitButton()
        {
            _quitButton.SetGOActive(true);
        }

        public void OnQuitButton()
        {
            Popup.Hide();
            GM.Instance.MainGame.OnHomeButton();
        }

        public void OnCreditsButton()
        {
            GM.Instance.Popups.GetPopup(PopupNames.CREDITS).Show();
        }

        public void OnMusicSlide(float value)
        {
            _cachedPref.MusicVolume = Mathf.RoundToInt(value * 50);
            AudioManager.MusicVolume = value * 0.5f;
        }

        public void OnSfxSlide(float value)
        {
            _cachedPref.SfxVolume = Mathf.RoundToInt(value * 50);
            AudioManager.SoundVolume = value * 0.5f;
        }

        public void OnVibrationToggle(bool value)
        {
            _cachedPref.Vibration = value;
        }
    }
}