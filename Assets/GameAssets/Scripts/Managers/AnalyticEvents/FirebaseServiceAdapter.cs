using com.brg.Common;
using Firebase;
using Firebase.Analytics;
using System;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common.AnalyticEvents;
using com.brg.Common.Logging;
using com.tinycastle.StickerBooker;

public class FirebaseServiceAdapter : IAnalyticsServiceAdapter
{
    private InitializationState _state;
    private float _progress;
    
    public InitializationState State => _state;
    public bool Usable => State == InitializationState.SUCCESSFUL;
    public ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
    
    public event Action OnInitializationSuccessfulEvent;
    public event Action OnInitializationFailedEvent;
    
    public IProgressItem GetInitializeProgressItem()
    {
        return new SingleProgressItem((out bool success) =>
            {
                success = State == InitializationState.SUCCESSFUL;
                return State > InitializationState.INITIALIZING;
            },
            () => _progress,
            null,
            10);
    }

    public void Initialize()
    {
        _state = InitializationState.INITIALIZING;
        _progress = 0f;
        
        FirebaseHelper.CheckDependencies().ContinueWith(t1 =>
        {
            var available = t1.Result == DependencyStatus.Available;

            if (available)
            {
                _progress = 1f;
                _state = InitializationState.SUCCESSFUL;
                OnInitializationSuccessfulEvent?.Invoke();
                LogObj.Default.Info("FirebaseServiceAdapter", $"Firebase initialized.");
            }
            else
            {
                LogObj.Default.Warn("FirebaseServiceAdapter", $"Firebase dependencies were not resolved (Result: {t1.Result}). Initialization halted.");
                _state = InitializationState.FAILED;
                OnInitializationFailedEvent?.Invoke();
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    public void SendEvent(AnalyticsEventBuilder eventBuilder)
    {
        if (TranslateGameEventName(eventBuilder.Name, out var name))
        {
            var parameters = eventBuilder.Parameters
                .Select(x => GetParam(x.name, x.type, x.value))
                .Where(x => x != null)
                .ToArray();
            FirebaseAnalytics.LogEvent(name, parameters);
            LogObj.Default.Info("FirebaseServiceAdapter", $"Logged event: {eventBuilder}");
        }
        else
        {
            LogObj.Default.Info("FirebaseServiceAdapter", $"Event not sent. Does not have event {eventBuilder.Name} translation.");
        }
    }

    public bool TranslateGameEventName(string name, out string translatedName)
    {
        return GameEvents.FirebaseTranslations.TryGetValue(name, out translatedName);
    }

    private static Parameter GetParam(string key, Type type, object value)
    {
        if (type == typeof(string)) return GetParam(key, (string)value);
        if (type == typeof(long)) return GetParam(key, (long)value);
        if (type == typeof(int)) return GetParam(key, (int)value);
        if (type == typeof(double)) return GetParam(key, (double)value);
        if (type == typeof(float)) return GetParam(key, (float)value);
        return null;
    }
    
    private static Parameter GetParam(string key, string value)
    {
        return new Parameter(key, value);
    }

    private static Parameter GetParam(string key, double value)
    {
        return new Parameter(key, value);
    }

    private static Parameter GetParam(string key, long value)
    {
        return new Parameter(key, value);
    }
}