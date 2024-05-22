using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.Utilities;
using com.tinycastle.StickerBooker;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameAssets.Scripts.Screens
{
    
    public partial class MainMenu: MonoManagerBase
    {
        public enum SubScreen
        {
            NONE = 0,
            SHOP = 1,
            ALBUM_SCREEN = 2,
            LEVEL_SELECTOR = 3,
            TIME_ATTACK_SCREEN = 4,
            MULTI_SCREEN = 5,
        }
        
        [Header("Comps")]
        [SerializeField] private RectTransform _overallRect;        
        [SerializeField] private RectTransform _viewHost;        
        [SerializeField] private GameObject _appearance;        
        [SerializeField] private GameObject _noAdButton;        
        
        [Header("Screens")]
        [SerializeField] private ShopMenu _shopMenu;
        [FormerlySerializedAs("_subScreen2")] [SerializeField] private LevelSelector _albumScreen;
        [SerializeField] private LevelSelector _levelSelector;
        [SerializeField] private LevelSelector _timeAttackSelector;
        [SerializeField] private LevelSelector _multiplayerSelector;
        
        [Header("Buttons")]
        [SerializeField] private TabButton _shopButton;
        [FormerlySerializedAs("_screen2Button")] [SerializeField] private TabButton _albumButton;
        [SerializeField] private TabButton _levelSelectorButton;
        [FormerlySerializedAs("_screen5Button")] [SerializeField] private TabButton _buttonTimeAttack;
        [FormerlySerializedAs("_screen4Button")] [SerializeField] private TabButton _buttonMulti;
        
        [Header("Params")] 
        [SerializeField] private float _transitTime = 1.2f;

        private TabButton[] _buttons;

        private SubScreen _lastScreen = SubScreen.LEVEL_SELECTOR;
        private SubScreen _currentScreen;
        private Vector2[] _anchoredOffsets;
        
        private bool _transiting = false;
        private SubScreen _targetScreen;
        private bool _skipAnim;
        private Tween _transitTween;
        private IProgressItem _transitProgress;

        private float _height = 0f;

        public ShopMenu Shop => _shopMenu;
        public LevelSelector Selector => _levelSelector;

        private void Awake()
        {
            _height = _viewHost.rect.height;
        }

        private void LateUpdate()
        {
            if (!_transiting || _transitProgress == null || !_transitProgress.Completed) return;
                
            _transitProgress = null;
            PerformTransit();
        }

        public void GoToShop()
        {
            SetScreen(SubScreen.SHOP);
        }
        
        public void GoToShopFromInGame()
        {
            if (_appearance.activeSelf)
            {
                SetScreen(SubScreen.SHOP);
            }
            else
            {
                Activate();
                SetScreen(SubScreen.SHOP);
            }
        }

        public void GoToSelector()
        {
            SetScreen(SubScreen.LEVEL_SELECTOR);
        }

        public void OnNoAdButton()
        {
            GM.Instance.HandleOnAdFreeButton();
            // GM.Instance.Popups.GetPopup<PopupBehaviourNoAds>().Show();
        }

        public void OnSettingsButton()
        {
            GM.Instance.Popups.GetPopup<PopupBehaviourSettings>().Show();
        }

        private void SetScreen(SubScreen screen, bool immediately = false)
        {
            if (_transiting) return;
            
            Log.Info($"Setting screen to: {screen} (from: {_currentScreen}).");
            
            _transitTween?.Kill();
            
            if (screen != SubScreen.NONE && screen == _currentScreen)
            {
                return;
            }
            
            _targetScreen = screen;

            var currAct = GetActivatable(_currentScreen);
            var nextAct = GetActivatable(_targetScreen);
            
            currAct?.PrepareDeactivate();
            nextAct?.PrepareActivate();
                
            if (currAct == null && nextAct == null) _transitProgress = new ImmediateProgressItem();
            else
            {
                var list = new List<IProgressItem>();
                if (currAct != null) list.Add(currAct.GetPrepareDeactivateProgressItem());
                if (nextAct != null) list.Add(nextAct.GetPrepareActivateProgressItem());
                _transitProgress = new ProgressItemGroup(list);
            }

            _transiting = true;
            _skipAnim = immediately;

            _lastScreen = _currentScreen > SubScreen.NONE ? _currentScreen : _lastScreen;
        }

        private void PerformTransit()
        {
            Log.Info($"Perform transit to: {_targetScreen} (from: {_currentScreen}).");
            
            var currAct = GetActivatable(_currentScreen);
            var nextAct = GetActivatable(_targetScreen);
            
            AnimateButtons(_skipAnim);
            
            if (_skipAnim)
            {
                currAct?.Deactivate();
                nextAct?.Activate();

                SetScreenImmediately(_currentScreen, _targetScreen);
                ConcludeSetScreen();
            }
            else
            {
                nextAct?.Activate();
                _transitTween = GetTransitTween(_currentScreen, _targetScreen)
                    .OnComplete(() =>
                    {
                        currAct?.Deactivate();
                        ConcludeSetScreen();
                    })
                    .Play();
            }
        }

        private void ConcludeSetScreen()
        {
            _transiting = false;
            _transitProgress = null;

            _currentScreen = _targetScreen;
            _targetScreen = SubScreen.NONE;
            
            Log.Info($"Conclude transition.");
        }

        private IActivatable GetActivatable(SubScreen screen)
        {
            return screen switch
            {
                SubScreen.NONE => null,
                SubScreen.SHOP => _shopMenu,
                SubScreen.ALBUM_SCREEN => _albumScreen,
                SubScreen.LEVEL_SELECTOR => _levelSelector,
                SubScreen.MULTI_SCREEN => _multiplayerSelector,
                SubScreen.TIME_ATTACK_SCREEN => _timeAttackSelector,
                _ => null
            };
        }

        private RectTransform GetTransformOf(SubScreen screen)
        {
            return screen switch
            {
                SubScreen.NONE => null,
                SubScreen.SHOP => _shopMenu.GetComponent<RectTransform>(),
                SubScreen.ALBUM_SCREEN => _albumScreen.GetComponent<RectTransform>(),
                SubScreen.LEVEL_SELECTOR => _levelSelector.GetComponent<RectTransform>(),
                SubScreen.TIME_ATTACK_SCREEN => _timeAttackSelector.GetComponent<RectTransform>(),
                SubScreen.MULTI_SCREEN => _multiplayerSelector.GetComponent<RectTransform>(),
                _ => null
            };
        }

        private void AnimateButtons(bool immediately)
        {
            _shopButton.SetState(_targetScreen == SubScreen.SHOP, immediately, _transitTime / 3);
            _albumButton.SetState(_targetScreen == SubScreen.ALBUM_SCREEN, immediately, _transitTime / 3);
            _levelSelectorButton.SetState(_targetScreen == SubScreen.LEVEL_SELECTOR, immediately, _transitTime / 3);
            _buttonTimeAttack.SetState(_targetScreen == SubScreen.TIME_ATTACK_SCREEN, immediately, _transitTime / 3);
            _buttonMulti.SetState(_targetScreen == SubScreen.MULTI_SCREEN, immediately, _transitTime / 3);
        }

        private Tween GetTransitTween(SubScreen from, SubScreen to)
        {
            var sequence = DOTween.Sequence();

            if (to != SubScreen.NONE)
            {
                var toRect = GetTransformOf(to);
                toRect.anchoredPosition = new Vector2(toRect.anchoredPosition.x, -_height - 15);
                sequence.Insert(0f, toRect.DOAnchorPosY(0f, _transitTime))
                    .SetEase(Ease.InOutQuart);

                toRect.SetGOActive(true);
            }

            if (from != SubScreen.NONE)
            {
                var fromRect = GetTransformOf(from);
                fromRect.anchoredPosition = new Vector2(fromRect.anchoredPosition.x, 0);
                sequence.Insert(0f, fromRect.DOAnchorPosY(-_height - 15, _transitTime))
                    .SetEase(Ease.InOutQuart);

                sequence.AppendCallback(() => fromRect.SetGOActive(false));
            }
            
            return sequence;
        }

        private void SetScreenImmediately(SubScreen from, SubScreen to)
        {
            if (to != SubScreen.NONE)
            {
                var toRect = GetTransformOf(to);
                toRect.anchoredPosition = new Vector2(toRect.anchoredPosition.x, 0);
                toRect.SetGOActive(true);
            }

            if (from != SubScreen.NONE)
            {
                var fromRect = GetTransformOf(from);
                fromRect.anchoredPosition = new Vector2(fromRect.anchoredPosition.x, -_height - 15);
                fromRect.SetGOActive(false);
            }
        }
    }
}