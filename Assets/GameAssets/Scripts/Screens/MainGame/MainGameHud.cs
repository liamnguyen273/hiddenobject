using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Common.UI;
using com.brg.Common.UI.Hud;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class MainGameHud : UIHud
    {
        [SerializeField] private MainGameManager _mainGame;
        
        [Header("Level data/progresses")]
        [SerializeField] private TextLocalizer _levelText;
        [SerializeField] private TextLocalizer _progressText;
        [SerializeField] private Image _progressImage;
        // [SerializeField] private GameObject _progressBar;
        
        [Header("Buttons")]
        [SerializeField] private GameObject _noAdButton;
        [SerializeField] private PowerUpButton _searchPowerUpButton;
        [SerializeField] private PowerUpButton _compassPowerUpButton;
        
        [Header("Cheats")]
        [SerializeField] private GameObject _hideUIButton;
        [SerializeField] private GameObject _cheatButton;

        [Header("Groups")] 
        [SerializeField] private GameObject _shopTray;
        [SerializeField] private GameObject _powerupTray;

        private LevelEntry _entry;

        public Transform SearchPowerButtonTransform => _searchPowerUpButton.transform;
        public Transform CompassPowerButtonTransform => _compassPowerUpButton.transform;

        internal override void InitializeBehaviour()
        {
            base.InitializeBehaviour();

            _searchPowerUpButton.GetComponent<UIButton>().Event.FunctionalEvent += OnLookupButton;
            _compassPowerUpButton.GetComponent<UIButton>().Event.FunctionalEvent += OnCompassButton;
        }

        public override void Activate()
        {
            _noAdButton.SetActive(!GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME));
            GM.Instance.Player.OnOwnEvent += OnOwn;
            
            _cheatButton.SetActive(GM.Instance.IsCheat);
            _hideUIButton.SetActive(GM.Instance.IsCheat);
            
            base.Activate();
        }

        public override void Deactivate()
        {
            GM.Instance.Player.OnOwnEvent -= OnOwn;

            _entry = null;
            
            base.Deactivate();
        }

        public void SetVisiblePowerupTray(bool active)
        {
            _powerupTray.SetActive(active);
            _shopTray.SetActive(active);
        }
        
        public void SetEntry(LevelEntry entry)
        {
            _entry = entry;

            SetLevelText(_entry.DisplayName);
        }

        public void OnHomeButton()
        {
            if (GM.Instance.MainGame.IsPlayingMultiplayer)
            {
                var popup = GM.Instance.Popups.GetPopup<PopupBehaviourAsk>(out var behaviour);
                behaviour.SetQuestion("Quit level?", "You will lose out on the level rewards!", () =>
                {
                    _mainGame.RequestGoHome();
                }, null);
                popup.Show();
            }
            else
            {
                _mainGame.RequestGoHome();
            }
        }

        public void OnSettingButtons()
        {
            if (_mainGame.Completing)
            {
                LogObj.Default.Warn("MainGameHud", "Game ending animation is playing, cannot show popup yet.");
                return;
            }

            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourSettings>(out var behaviour);
            behaviour.ShowQuitButton();
            popup.Show();
        }
        
        public void OnNoAdButton()
        {
            if (_mainGame.Completing)
            {
                LogObj.Default.Warn("MainGameHud", "Game ending animation is playing, cannot show popup yet.");
                return;
            }
            
            GM.Instance.HandleOnAdFreeButton();
        }

        public void OnCheatButton()
        {
            if (_mainGame.Completing)
            {
                LogObj.Default.Warn("MainGameHud", "Game ending animation is playing, cannot show popup yet.");
                return;
            }
            
            _mainGame.RequestEndGame();
        }
        
        public void OnHideUIButton()
        {
            if (_mainGame.Completing)
            {
                LogObj.Default.Warn("MainGameHud", "Game ending animation is playing, cannot show popup yet.");
                return;
            }
            
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void OnShopButton()
        {
            GM.Instance.Popups.GetPopup(PopupNames.SHOP_INGAME).Show();
        }

        public void OnLookupButton()
        {
            GM.Instance.MainGame.RequestSearchPowerup();
        }
        
        public void OnCompassButton()
        {
            GM.Instance.MainGame.RequestCompassPowerup();
        }

        public void UpdateProgress(int current, int total)
        {
            total = total == 0 ? 1 : total;
            _progressText.RawString = $"{current:00}/{total:00}\nFound";
            _progressImage.fillAmount = (float)current / total;
        }

        public void SetLevelText(string levelName)
        {
            _levelText.RawString = levelName;
        }
        
        private void OnOwn(string id)
        {
            if (id == GlobalConstants.NO_AD_ITEM_NAME)
            {
                _noAdButton.SetActive(false);
            }
        }
    }
}