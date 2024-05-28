using System;
using com.brg.Common;
using com.brg.Common.Random;
using com.brg.Common.UI;
using com.brg.Utilities;
using JSAM;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.AnalyticEvents;
using com.tinycastle.StickerBooker.Effects;
using com.tinycastle.StickerBooker.RemoteConfig;
using GameAssets.Scripts.Screens;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

namespace com.tinycastle.StickerBooker
{
    public partial class GM : MonoManagerBase
    {
        public static GM Instance { get; private set; }
        
        [Header("Debug")] 
        [SerializeField] private UIButton _maxDebuggerButton;

        [Header("Params")] 
        [SerializeField] private bool _shouldLog;
        [SerializeField] private bool _isCheat;  
        [SerializeField] private string _forcedTheme = "default";
        [SerializeField] private bool _showMaxDebugger;

        [Header("Components")]
        [SerializeField] private WaitScreenHelper _waitHelper;
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private EffectMaker _effectMaker;
        [SerializeField] private ParticleSystem _snowParticles;

        // Managers
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private MainGameManager _mainGameManager;
        [SerializeField] private MainGameHud _mainGameHud;
        
        [SerializeField] private MainMenu _mainMenu;

        [SerializeField] private AdManager _adManager;
        [SerializeField] private FacebookManager _facebookManager;
        private PlayerManager _playerManager;
        private DataManager _dataManager;
        private PurchaseManager _purchaseManager;
        private AnalyticsEventManager _analyticsEventManager;
        private RemoteConfigManager _remoteConfigManager;

        private List<IInitializable> _primaryManagers;
        private List<IInitializable> _secondaryManagers;

        private IRandomEngine _rng;

#if FORCED_CHEAT
        public bool IsCheat => true;
#elif !UNITY_EDITOR
        public bool IsCheat => false;
#else
        public bool IsCheat => _isCheat;
#endif
        
        // Manager accessors
        public LoadingScreen Loading => _loadingScreen;  
        public WaitScreenHelper WaitHelper => _waitHelper;
        
        public PopupManager Popups => _popupManager;
        public MainGameManager MainGame => _mainGameManager;
        public MainGameHud MainGameHud => _mainGameHud;
        public MainMenu Menu => _mainMenu;
        
        public AdManager Ad => _adManager;
        public FacebookManager Facebook => _facebookManager;
        public PlayerManager Player => _playerManager;
        public DataManager Data => _dataManager;
        public PurchaseManager Purchases => _purchaseManager;
        public AnalyticsEventManager Events => _analyticsEventManager;
        public RemoteConfigManager RemoteConfigs => _remoteConfigManager;
        
        public IRandomEngine Rng => _rng;
        public EffectMaker Effects => _effectMaker;

        public event Action<string> OnThemeChangeEvent;
        
        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 120;
            CLog.ShouldLog = _shouldLog;
            
            _playerManager = new PlayerManager();
            _dataManager = new DataManager();

            var seed = (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds;
            _rng = RandomEngineFactory.CreateEngine(Engine.STANDARD, seed);
            Debug.Log("seed: " + seed);
            // Debug.Log("test: " + _rng.GetInteger());
            
            _analyticsEventManager =
                new AnalyticsEventManager(new AppFlyerServiceAdapter(), new FirebaseServiceAdapter());
            _purchaseManager = new PurchaseManager();
            _remoteConfigManager = new RemoteConfigManager();
        }

        private void Start()
        {
            _primaryManagers = new List<IInitializable>()
            {
                _dataManager,
                _playerManager,
            };

            _secondaryManagers = new List<IInitializable>()
            {
                _analyticsEventManager,
                _remoteConfigManager,
                _purchaseManager,
                _adManager,
                _facebookManager,
                _mainMenu,
                _mainGameManager,
                _mainGameHud,
                _popupManager,
            };

            Initialize();
        }
        
        private void Update()
        {
            var dt = Time.deltaTime;

            UpdateInitialization(dt);
            UpdateCheckInternet(dt);
        }

        public bool ResolveUnlockCondition(LevelEntry level)
        {
            var condition = level.UnlockCondition;
            if (IsCheat) return true;
            
            return condition switch
            {
                GlobalConstants.UNLOCK_CONDITION_NONE => true,
                GlobalConstants.UNLOCK_CONDITION_SEQUENTIAL => ResolveSequentialUnlockCondition(level),
                GlobalConstants.UNLOCK_CONDITION_OWN => Player.Own(level.Id),
                _ => ResolveLevelUnlockConditionByParsing(level)
            };
        }

        public Transform ResolveFlyerTarget(string item)
        {
            return item switch
            {
                GlobalConstants.POWER_COMPASS => GM.Instance.MainGameHud.CompassPowerButtonTransform,
                GlobalConstants.POWER_LOOKUP => GM.Instance.MainGameHud.SearchPowerButtonTransform,
                _ => null
            };
        }
        
        public void ResolveAnimateAddItems(string[] items, int[] counts, bool usePopupCongrats, float delay = 0f)
        {
            var popup = Popups.GetPopup<PopupBehaviourCongrats>(out var behaviour);
            
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                int i = -1;
                var actions = items.Select<string, Action>(x =>
                {
                    ++i;
                    return () => Player.AddResource(x, counts[i], true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                
                Effects.PlayFlyThings(popup.transform.position, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, true, false);
                }, 160, delay: delay);

                AudioManager.PlaySound(LibrarySounds.Sticker);
            };

            if (usePopupCongrats)
            {
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }

        public void ResolveAnimateAddItems(Vector3 from, string[] items, bool usePopupCongrats)
        {
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                var actions = items.Select<string, Action>(x =>
                {
                    return () => Player.AddResource(x, 1, true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                var counts = Enumerable.Repeat(1, items.Length).ToArray();
                Effects.PlayFlyThings(from, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, true, false);
                }, 160);
            };

            if (usePopupCongrats)
            {
                var popup = Popups.GetPopup<PopupBehaviourCongrats>(out var behaviour);
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }
        
        public void ResolveAnimateAddItems(Vector3 from, string[] items, int[] counts, bool usePopupCongrats, float delay = 0f)
        {
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                int i = -1;
                var actions = items.Select<string, Action>(x =>
                {
                    ++i;
                    return () => Player.AddResource(x, counts[i], true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                
                Effects.PlayFlyThings(from, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, true, false);
                }, 160, delay: delay);

                AudioManager.PlaySound(LibrarySounds.Sticker);
            };

            if (usePopupCongrats)
            {
                var popup = Popups.GetPopup<PopupBehaviourCongrats>(out var behaviour);
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }

        public void SetTheme(string themeName)
        {
            if (IsCheat) themeName = _forcedTheme;
            Ad.SetTheme(themeName);
            OnThemeChangeEvent?.Invoke(themeName);
        }

        public string GetTheme()
        {
            if (IsCheat) return _forcedTheme;
            return RemoteConfigs.GetValue(GameRemoteConfigs.GAME_THEME, GlobalConstants.DEFAULT_THEME);
        }

        private bool ResolveSequentialUnlockCondition(LevelEntry level)
        {
            var previousEntry = Data.GetPreviousEntry(level.Id);

            Player.GetLevelState(level.Id, out var _, out var hasProgress, out var _);

            return previousEntry == null || Player.CheckCompletedWithoutExistence(previousEntry.Id)
                || Player.CheckCompletedWithoutExistence(level.Id)
                || hasProgress;
        }

        private bool ResolveLevelUnlockConditionByParsing(LevelEntry level)
        {
            var condition = level.UnlockCondition;
            var tokens = condition.Split(':',StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                Log.Warn($"Unlock condition \"{condition}\" failed to parse. Will return false");
                return false;
            }

            var type = tokens[0];
            switch (type)
            {
                case GlobalConstants.UNLOCK_CONDITION_BEAT_LEVEL:
                    var levelToCheckBeat = tokens[1];
                    return Player.CheckCompletedWithoutExistence(levelToCheckBeat);
                default:
                    Log.Warn($"Unlock condition \"{condition}\" is not recognized. Will return false.");
                    return false;
            }
        }
    }
}