using com.brg.Common;
using DG.Tweening;
using System;
using Newtonsoft.Json;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public struct AdRequest
    {
        public readonly string Type;
        public readonly bool AllowSkippingIfAdFree;
        [JsonIgnore] public readonly Action AdSuccessfulCallback;
        [JsonIgnore] public readonly Action AdFailedCallback;
        public readonly float WaitTime;
        public readonly (string name, Type type, object value)[] ArbitraryData;

        public AdRequest(string type, 
            Action adSuccessfulCallback, 
            Action adFailedCallback, 
            float waitTime = 4f,
            bool allowSkippingIfAdFree = true,
            (string name, Type type, object value)[] arbitraryData = null)
        {
            Type = type;
            AllowSkippingIfAdFree = allowSkippingIfAdFree && type == AdManager.TYPE_INTERSTITIAL;
            AdSuccessfulCallback = adSuccessfulCallback;
            AdFailedCallback = adFailedCallback;
            WaitTime = waitTime;
            ArbitraryData = arbitraryData;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    
    [DisallowMultipleComponent]
    public partial class AdManager : MonoManagerBase
    {
        private enum AdState
        {
            IDLE = 0,
            REQUESTED,
            WAITING,
            SHOWING,
            SHOW_FAILED,
            SHOW_SUCCESSFUL,
        }
        
        
#if UNITY_IOS
        private const string SDK_KEY = "zq2X1FfdfeIMOsmepidyMNbeqvHKzJyNwy6EI2lT_14Ns_yAy-XVUTSsThDAZ5-AqDJ2OU0CLSeP7euEiV4wor";
        private const string REWARD_AD_UNIT = "d1033909fdcfebae";
        private const string INTERSTITIAL_AD_UNIT = "2f7121f432ab8747";
        private const string BANNER_AD_UNIT = "e380e1c3c5d0c1f2";
#elif UNITY_ANDROID
        private const string SDK_KEY = "VG8UsnJLt1rXH1FMWShaVrSsxV8Gxw8RBGw12EMwFBRXDgul7aJdQnULHcbCXkdi_Tl1n5y1HNZU2SDA1XpRIH";
        private const string REWARD_AD_UNIT = "3720273d445651e6";
        private const string INTERSTITIAL_AD_UNIT = "726804f1d8ec9758";
        private const string BANNER_AD_UNIT = "fcf066d498933aeb";
#endif

        public const string TYPE_INTERSTITIAL = "inter";
        public const string TYPE_REWARD = "reward";
        
        [SerializeField] private CanvasGroup _canvas;
        [SerializeField] private GameObject _interAdIcon;
        
        private Tween _fadeTween;
        private bool _appearanceShown = false;

        private AdState _adState = AdState.IDLE;
        private AdRequest _request;
        private float _waitTimer;
        
        public bool IsAdFree => GM.Instance.Player.GetAdFree();

        public bool Active => _appearanceShown;


        public event Action OnAdFadedInEvent;
        public event Action OnAdFadedOutEvent;
        
        private void Awake()
        {
            _canvas.alpha = 0;
            _canvas.gameObject.SetActive(false);

            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
        }
        
        private void Update()
        {
            if (_adState != AdState.WAITING) return;
            
            _waitTimer -= Time.deltaTime;

            if (IsAdReady())
            {
                ShowAd();
            }
            else if (_waitTimer <= 0f)
            {
                OnAdWaitTimeout();
            }
        }

        public void RequestAd(AdRequest request)
        {
            if (_adState != AdState.IDLE)
            {
                Log.Warn("An ad request is already in progress. The request will complete as FAILED");
                SetAdRequestAsFailed();
            }

            // TODO Log
            if (request.Type == TYPE_INTERSTITIAL || request.Type == TYPE_REWARD)
            {
                GM.Instance.Events.MakeEvent(request.Type == TYPE_INTERSTITIAL
                    ? GameEvents.INTERSTITIAL_AD_REQUEST
                    : GameEvents.REWARD_AD_REQUEST)
                    .SendEvent();
            }
            
            _request = request;
            _adState = AdState.REQUESTED;
                        
            Log.Info($"Ad request received:\n{_request}");

            if (request.AllowSkippingIfAdFree && IsAdFree)
            {
                Log.Info("User has Ad Free, ad will be completed immediately");
                SetAdRequestAsSuccessful();
                ConcludeAdRequest();
            }
            else
            {
                FadeIn();
            }
        }

        private void FadeIn()
        {
            _canvas.gameObject.SetActive(true);
            _interAdIcon.SetActive(_request.Type == TYPE_INTERSTITIAL);
            _fadeTween?.Kill();
            _fadeTween = _canvas.DOFade(1, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    StartAdWaitTimer();
                })
                .Play();
            
            _appearanceShown = true;
            OnAdFadedInEvent?.Invoke();
        }

        private void FadeOut()
        {
            _fadeTween?.Kill();
            _fadeTween = _canvas.DOFade(0, 0.5f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    _canvas.gameObject.SetActive(false);
                })
                .Play();

            _appearanceShown = false;
            OnAdFadedOutEvent?.Invoke();
            
        }

        private void OnAdFailedToShow()
        {
            Log.Warn($"Ad failed to show. Will resolve request as FAILED");
            SetAdRequestAsFailed();
            ConcludeAdRequest();
        }

        private void OnAdCompleted(bool result)
        {
            Log.Info($"Ad showing successfully as {result}. Will check for ad result to resolve.");
            if (result) SetAdRequestAsSuccessful();
            else SetAdRequestAsFailed();
            ConcludeAdRequest();
        }

        private void OnAdWaitTimeout()
        {
            Log.Warn($"Ad failed to show in timeout allowed by request ({_request.WaitTime}). Will resolve request as FAILED");
            SetAdRequestAsFailed();
            ConcludeAdRequest();
        }

        private bool IsAdReady()
        {
            var type = _request.Type;
            return type switch
            {
                TYPE_INTERSTITIAL => MaxSdk.IsInterstitialReady(INTERSTITIAL_AD_UNIT),
                TYPE_REWARD => MaxSdk.IsRewardedAdReady(REWARD_AD_UNIT),
                _ => false
            };
        }

        private void StartAdWaitTimer()
        {
            _waitTimer = _request.WaitTime;
            _adState = AdState.WAITING;
        }

        private void ShowAd()
        {
            _adState = AdState.SHOWING;
            
            switch (_request.Type)
            {
                case TYPE_INTERSTITIAL:
                    ShowInterstitial();
                    break;
                case TYPE_REWARD:
                    ShowRewardAd();
                    break;
                default:
                    Log.Warn($"Unknown ad type {_request.Type}, will resolve ad as FAILED.");
                    SetAdRequestAsFailed();
                    break;
            }
        }

        private void SetAdRequestAsSuccessful()
        {
            _adState = AdState.SHOW_SUCCESSFUL;
            ResolveAdResult(true);
        }
        
        private void SetAdRequestAsFailed()
        {
            _adState = AdState.SHOW_FAILED;
            ResolveAdResult(false);
        }

        private void ResolveAdResult(bool result)
        {
            if (result)
            {
                _request.AdSuccessfulCallback?.Invoke();
            }
            else
            {
                _request.AdFailedCallback?.Invoke();
            }
        }

        private void ConcludeAdRequest()
        {
            _adState = AdState.IDLE;

            if (_appearanceShown)
            {
                FadeOut();
            }
            
            Log.Info($"Request {_request} concluded.");
        }
        
        private void OnAdRevenuePaidEvent(string s, MaxSdkBase.AdInfo adInfo)
        {
            Log.Info($"OnAdRevenuePaidEvent, unit: {adInfo}, revenue: {adInfo.Revenue}");
            
            GM.Instance.Events.MakeEvent(GameEvents.AD_IMPRESSION_OR_REVENUE)
                .Add("ad_platform", "AppLovin")
                .Add("ad_source", adInfo.NetworkName)
                .Add("ad_unit_name", adInfo.AdUnitIdentifier)
                .Add("ad_format", adInfo.AdFormat)
                .Add("value", adInfo.Revenue)
                .Add("currency", "USD")
                .SendEvent();
        }
    }
}
