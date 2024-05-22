using com.brg.Common.UI;
using System;
using com.brg.Common;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourInternetChecker : UIPopupBehaviour
    {
        [SerializeField] private GameObject _checkScreen;
        [SerializeField] private Transform _panel;
        
        private float _checkTimer = -1f;
        private bool _checking = false;
        
        private Tween _panelTween;

        private void Awake()
        {
            _checkScreen.SetActive(false);
        }

        private void Update()
        {
            if (!_checking) return;

            _checkTimer -= Time.unscaledDeltaTime;

            if (_checkTimer <= 0)
            {
                _checking = false;
                _checkScreen.SetActive(false);
                PerformCheck();
            }
        }
        
        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
            _checking = false;
        }

        public void OnRetryButton()
        {
            _checkScreen.SetActive(true);
            HidePanel();
            _checking = true;
            _checkTimer = 2.5f;
        }
        
        private void ShowPanel()
        {
            _panelTween?.Kill();
            _panelTween = GetPanelShowTweenOnly().Play();
        }
        
        private void HidePanel()
        {
            _panelTween?.Kill();
            _panelTween = GetPanelHideTweenOnly().Play();
        }

        private void PerformCheck()
        {
            _checking = false;
            
            if (GM.CheckInternet())
            {
                Popup.Hide();
            }
            else
            {
                ShowPanel();
            }
        }
        
        private Tween GetPanelShowTweenOnly()
        {
            return _panel.DOScale(1, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        private Tween GetPanelHideTweenOnly()
        {
            return _panel.DOScale(0, 0.5f).SetUpdate(UpdateType.Normal, true);;
        }
    }
}