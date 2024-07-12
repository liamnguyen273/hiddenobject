using System.Collections.Generic;
using System.Linq;
using com.brg.Common;

namespace com.tinycastle.StickerBooker
{
    public enum GameScreen
    {
        NONE = 0,
        MENU,
        GAMEPLAY
    }
    
    public partial class GM
    {
        private IProgressItem _goToProgress = null;
        private GameScreen _currentScreen = GameScreen.NONE;

        public void RequestPlayLevel(string level)
        {
            MainGame.SetLevel(level);
            RequestGoTo(GameScreen.GAMEPLAY);
        }

        public void RequestGoToMenu()
        {
            RequestGoTo(GameScreen.MENU);
        }
        
        public void RequestGoTo(GameScreen screen, bool append = false)
        {
            Log.Info($"Requested to go to {screen}");

            var from = _currentScreen;
            var to = screen;
            
            var list1 = PrepareDeactivateScreen(from);
            var list2 = PrepareActivateScreen(to);

            _goToProgress = new ProgressItemGroup(list1.Concat(list2));
            _loadingScreen.RequestLoad(_goToProgress, 
                beforeOutAction: () =>
                {
                    DeactivateScreen(from);
                    ActivateScreen(to);
                }, 
                loadAppend: append);

            _currentScreen = to;
        }
        
        private IProgressItem[] PrepareDeactivateScreen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.GAMEPLAY:
                    _mainGameManager.PrepareDeactivate();
                    _mainGameHud.PrepareDeactivate();
                    return new[]
                        { _mainGameManager.GetPrepareDeactivateProgressItem(), _mainGameHud.GetPrepareDeactivateProgressItem() };
                case GameScreen.MENU:
                    _mainMenu.PrepareDeactivate();
                    return new[] { _mainMenu.GetPrepareDeactivateProgressItem() };
                default:
                    return new IProgressItem[0];
            }
        }
        
        private IProgressItem[] PrepareActivateScreen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.GAMEPLAY:
                    _mainGameManager.PrepareActivate();
                    _mainGameHud.PrepareActivate();
                    return new[]
                        { _mainGameManager.GetPrepareActivateProgressItem() };
                case GameScreen.MENU:
                    _mainMenu.PrepareActivate();
                    return new[] { _mainMenu.GetPrepareActivateProgressItem() };
                default:
                    return new IProgressItem[0];
            }
        }
        
        private void ActivateScreen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.GAMEPLAY:
                    _mainGameManager.Activate();
                    _mainGameHud.Activate();
                    break;
                case GameScreen.MENU:
                    _mainMenu.Activate();
                    if (GetTheme() == GlobalConstants.CHRISTMAS_THEME)
                    {
                        _snowParticles.Play();
                    }
                    break;
                default:
                    break;
            }
        }        
        
        private void DeactivateScreen(GameScreen screen)
        {
            switch (screen)
            {
                case GameScreen.GAMEPLAY:
                    _mainGameManager.Deactivate();
                    _mainGameHud.Deactivate();
                    break;
                case GameScreen.MENU:
                    _mainMenu.Deactivate();
                    _snowParticles.Stop();
                    break;
                default:
                    break;
            }
        }
    }
}