using com.brg.Common;
using com.tinycastle.StickerBooker;
using JSAM;
using UnityEngine;

namespace GameAssets.Scripts.Screens
{
    public partial class MainMenu: IActivatable
    {
        public void Activate()
        {
            GM.Instance.Player.OnOwnEvent += OnOwn;
            
            _appearance.SetActive(true);
            SetScreen(_lastScreen, true);
            PlayThemeMusic();

            OnNoBanner();
            
            if (!GM.Instance.Ad.HasBanner)
            {
                GM.Instance.Ad.OnBannerShowEvent += OnHasBanner;
            }
            else
            {
                OnHasBanner();
            }

            if (GM.Instance.Player.Own(GlobalConstants.NO_AD_ITEM_NAME))
            {
                _noAdButton.SetActive(false);
            }
            else
            {
                _noAdButton.SetActive(true);
            }
        }

        public void Deactivate()
        {
            GM.Instance.Player.OnOwnEvent -= OnOwn;
            
            SetScreen(SubScreen.NONE, true);
            _appearance.SetActive(false);
            StopThemeMusic();
            
            GM.Instance.Ad.OnBannerShowEvent -= OnHasBanner;
        }

        public void PrepareActivate()
        {
            // Do nothing
        }

        public void PrepareDeactivate()
        {
            // Do nothing
        }

        public IProgressItem GetPrepareActivateProgressItem()
        {
            return new ImmediateProgressItem();
        }

        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            return new ImmediateProgressItem();
        }

        private void OnOwn(string id)
        {
            if (id == GlobalConstants.NO_AD_ITEM_NAME)
            {
                OnNoBanner();
                _noAdButton.SetActive(false);
            }
        }

        private void OnHasBanner()
        {
            _overallRect.offsetMin = new Vector2(_overallRect.offsetMin.x, 200);
            _overallRect.offsetMax = new Vector2(_overallRect.offsetMax.x, 0);
        }

        private void OnNoBanner()
        {
            _overallRect.offsetMin = new Vector2(_overallRect.offsetMin.x, 0);
            _overallRect.offsetMax = new Vector2(_overallRect.offsetMax.x, 0);
        }

        private void PlayThemeMusic()
        {
            AudioManager.PlayMusic(GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? LibraryMusic.MainMenuXmasMusic : LibraryMusic.MainMenuMusic);
        }
        
        private void StopThemeMusic()
        {
            AudioManager.StopMusic(GM.Instance.GetTheme() == GlobalConstants.CHRISTMAS_THEME ? LibraryMusic.MainMenuXmasMusic : LibraryMusic.MainMenuMusic);
        }
    }
}