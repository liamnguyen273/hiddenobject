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
        [SerializeField] private LeanToggle _musicToggle;
        [SerializeField] private LeanToggle _sfxToggle;
        // [SerializeField] private LeanToggle _vibrationToggle;
        [SerializeField] private UIButton _quitButton;

        private PlayerPreference _cachedPref;
        
        protected override void InnateOnShowStart()
        {
            var pref = GM.Instance.Player.GetPreference();
            _cachedPref = pref;

            _musicToggle.Set(pref.MusicVolume > 0);
            _sfxToggle.Set(pref.SfxVolume > 0);
            // _vibrationToggle.Set(pref.Vibration);

            _version.RawString = $"Version {Application.version}";
            
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

        public void OnMusicToggle(bool value)
        {
            _cachedPref.MusicVolume = value ? 50 : 0;
            AudioManager.MusicVolume = _cachedPref.MusicVolume > 0 ? 0.5f : 0f;
        }

        public void OnSfxToggle(bool value)
        {
            _cachedPref.SfxVolume = value ? 50 : 0;
            AudioManager.SoundVolume = _cachedPref.SfxVolume > 0 ? 0.5f : 0f;
        }

        public void OnVibrationToggle(bool value)
        {
            _cachedPref.Vibration = value;
        }
    }
}