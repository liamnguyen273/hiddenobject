using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class GM
    {
        private bool _checkingInternet = false;
        private float _internetCheckTimer = 0f;
        
        /// <summary>
        /// Temporary internet flag
        /// </summary>
        public bool HasInternet { get; protected set; }

        public void PerformInternetCheck()
        {
            _checkingInternet = true;
            HasInternet = CheckInternet();

            if (HasInternet) return;
            
            var popup = Popups.GetPopup<PopupBehaviourInternetChecker>(out var behaviour);
            behaviour.OnHideEnd(OnHasInternet);
            popup.Show();
        }

        private void UpdateCheckInternet(float dt)
        {
            if (Usable && !_checkingInternet)
            {
                _internetCheckTimer -= dt;

                if (_internetCheckTimer <= 0f)
                {
                    PerformInternetCheck();
                }
            }
        }
        
        private void OnHasInternet()
        {
            _checkingInternet = false;
            _internetCheckTimer = 15f;
            HasInternet = true;
        }

        public static bool CheckInternet()
        {
#if UNITY_EDITOR
            return true;
#else
            return Instance.IsCheat || Application.internetReachability != NetworkReachability.NotReachable;
#endif
        }
    }
}