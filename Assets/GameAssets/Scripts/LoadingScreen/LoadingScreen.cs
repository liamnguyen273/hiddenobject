using com.brg.Common;
using JSAM;
using System;
using com.brg.Common.Logging;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public enum LoadState
    {
        IDLE = 0,
        TRANSIT_IN,
        LOADING,
        TRANSIT_OUT
    }
    
    public class LoadingScreen : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject _initialLoadBlocker;
        [SerializeField] private GameObject _inputBlocker;
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _mask;      
        [SerializeField] private float _transitTime = 1f;
        [SerializeField] private float _naturalDelayTime = 0.5f;

        private LoadState _loadState = LoadState.IDLE;
        private IProgressItem _progressItem;
        private bool _hasAppend;
        private Action _beforeOutAction;
        private Action _completeAction;
        private Tween _transitTween;

        private float _width;
        
        public bool Active => _progressItem != null;

        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 0f);
            UpdateProgressBar(out _);

            _width = GetComponent<RectTransform>().rect.width;
            _mask.anchoredPosition = new Vector2(-_width - 15, 0f);
        }

        private void LateUpdate()
        {
            if (!Active) return;

            // Always update if Active
            UpdateProgressBar(out var isDone);
            
            if (_loadState == LoadState.LOADING)
            {
                // Wait for done and trigger here
                if (isDone)
                {
                    OnLoadingDone();
                }
            }
        }

        public void RequestLoad(IProgressItem item,  
            Action beforeOutAction = null, 
            Action completeAction = null,
            bool loadAppend = false)
        {
            if (Active && loadAppend)
            {
                if (_loadState >= LoadState.TRANSIT_OUT)
                {
                    LogObj.Default.Warn("LoadingScreen", "Load append is requested too late, and will be skipped.");
                    return;
                }
                else
                {
                    _hasAppend = true;
                    LogObj.Default.Info("LoadingScreen", "Appended a load.");
                }
            }

            _progressItem = item;
            _beforeOutAction = beforeOutAction;
            _completeAction = completeAction;
            
            gameObject.SetActive(true);
            _inputBlocker.SetActive(true);

            if (_loadState < LoadState.TRANSIT_IN)
            {
                TransitIn();
            }
            else
            {
                ProperLoading();
            }
        }

        private void TransitIn()
        {
            _loadState = LoadState.TRANSIT_IN;

            _transitTween = DOTween.Sequence()
                .Append(_mask.DOAnchorPosX(0, _transitTime)
                .SetEase(Ease.InOutSine))
                .AppendInterval(_naturalDelayTime)
                .OnComplete(() =>
                {
                    _transitTween = null;
                    ConcludeTransitIn();
                });

            _slider.value = 0f;
            
            // Hide banner ad
            GM.Instance.Ad.RequestHideBanner();
        }

        private void ConcludeTransitIn()
        {
            ProperLoading();
        }

        private void ProperLoading()
        {
            _loadState = LoadState.LOADING;
            
            // Do nothing
        }

        private void OnLoadingDone()
        {
            _hasAppend = false;
            
            // Perform callback
            _beforeOutAction?.Invoke();
            _initialLoadBlocker.SetActive(false);

            if (!_hasAppend)
            {
                TransitOut();
            }
        }

        private void TransitOut()
        {
            _loadState = LoadState.TRANSIT_OUT;
            
            _transitTween = DOTween.Sequence()
                .AppendInterval(_naturalDelayTime)
                .Append(_mask.DOAnchorPosX(_width + 15, _transitTime)
                    .SetEase(Ease.InOutSine))
                    
                .OnComplete(() =>
                {
                    _transitTween = null;
                    ConcludeTransitOut();
                });
        }

        private void ConcludeTransitOut()
        {
            // Perform callback
            _completeAction?.Invoke();
            ConcludeLoading();
        }

        private void UpdateProgressBar(out bool isDone)
        {
            var progress = _progressItem?.Progress ?? 0f;
            var completed = _progressItem != null && _progressItem.Completed;
            
            // LogObj.Default.Info("Loading", $"Progress: {progress}. Complete: {completed}");

            _slider.value = completed ? 1f : progress;
            isDone = completed;
        }

        private void ConcludeLoading()
        {
            _progressItem = null;
            _beforeOutAction = null;
            _completeAction = null;

            _loadState = LoadState.IDLE;
            _hasAppend = false;

            _mask.anchoredPosition = new Vector2(-_width - 15, 0f);
            
            gameObject.SetActive(false);
            _inputBlocker.SetActive(false);
            
            GM.Instance.Ad.RequestShowBanner();
        }
    }
}