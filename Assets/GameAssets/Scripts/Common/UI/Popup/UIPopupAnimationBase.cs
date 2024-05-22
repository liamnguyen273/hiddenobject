using System;
using com.brg.Common.Logging;
using DG.Tweening;
using UnityEngine;

namespace com.brg.Common.UI
{
    public enum UIAnimState
    {
        NONE,
        SHOW_START,
        SHOW_END,
        HIDE_START,
        HIDE_END
    }
    
    public abstract class UIPopupAnimationBase: MonoBehaviour
    {
        private Tween _showTween;
        private Tween _hideTween;
        private UIAnimState _state;

        public UIAnimState State => _state;
        
        public Action<UIAnimState> OnStateChangeAction { get; set; }
        
        public UIPopup Popup { get; internal set; }

        protected abstract Tween GetShowTween();
        protected abstract Tween GetHideTween();
        protected abstract void PerformShowImmediately();
        protected abstract void PerformHideImmediately();

        internal virtual void Initialize()
        {
            SetState(UIAnimState.NONE);
            PlayHideImmediately();
        }
        
        internal void PlayShow()
        {
            CleanTweens();
            gameObject.SetActive(true);
            
            var tween = GetShowTween();
            _showTween = tween.OnComplete(() =>
            {
                _showTween = null;
                SetState(UIAnimState.SHOW_END);
            }).Play();
            SetState(UIAnimState.SHOW_START);
        }

        internal void PlayHide()
        {            
            CleanTweens();

            var tween = GetHideTween();
            _hideTween = tween.OnComplete(() =>
            {
                _hideTween = null;
                SetState(UIAnimState.HIDE_END);
                gameObject.SetActive(false);
                SetState(UIAnimState.NONE);
            }).Play();
            SetState(UIAnimState.HIDE_START);
        }

        internal void PlayShowImmediately()
        {
            CleanTweens();
            gameObject.SetActive(true);
            PerformShowImmediately();
            SetState(UIAnimState.SHOW_START);
            SetState(UIAnimState.SHOW_END);
        }

        internal void PlayHideImmediately()
        {
            CleanTweens();
            PerformHideImmediately();
            SetState(UIAnimState.HIDE_START);
            SetState(UIAnimState.HIDE_END);
            gameObject.SetActive(false);
            SetState(UIAnimState.NONE);
        }

        private void CleanTweens()
        {
            _showTween?.Kill();
            _hideTween?.Kill();
            _showTween = null;
            _hideTween = null;
        }

        private void SetState(UIAnimState state)
        {
            _state = state;
            LogObj.Default.Info(Popup.ExplicitName, $"Animation: state is set to {_state}");
            OnStateChangeAction?.Invoke(state);
        }
    }
}