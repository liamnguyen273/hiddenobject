using com.brg.Common;
using UnityEngine;

namespace GameAssets.Scripts.Screens
{
    public partial class MainMenu
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        
        protected override void StartInitializationBehaviour()
        {
            // make menu
            _shopMenu.Initialize();
            _albumScreen.Initialize();
            _levelSelector.Initialize();
            _timeAttackSelector.Initialize();
            _multiplayerSelector.Initialize();

            _buttons = new[]
            {
                _shopButton,
                _albumButton,
                _levelSelectorButton,
                _buttonMulti,
                _buttonTimeAttack
            };

            _shopButton.Event.FunctionalEvent += () => SetScreen(SubScreen.SHOP);
            _albumButton.Event.FunctionalEvent += () => SetScreen(SubScreen.ALBUM_SCREEN);
            _levelSelectorButton.Event.FunctionalEvent += () => SetScreen(SubScreen.LEVEL_SELECTOR);
            _buttonTimeAttack.Event.FunctionalEvent += () => SetScreen(SubScreen.TIME_ATTACK_SCREEN);
            _buttonMulti.Event.FunctionalEvent += () => SetScreen(SubScreen.MULTI_SCREEN);
            
            _shopMenu.gameObject.SetActive(false);
            _albumScreen.gameObject.SetActive(false);
            _levelSelector.gameObject.SetActive(false);
            _timeAttackSelector.gameObject.SetActive(false);
            _multiplayerSelector.gameObject.SetActive(false);
            
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            _currentScreen = SubScreen.NONE;
        }
    }
}