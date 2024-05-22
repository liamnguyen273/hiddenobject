using System;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public abstract class ThemeableComponent<TResource>: MonoBehaviour
    {
        public abstract void SetTheme(string themeName);
        public abstract TResource GetThemedResource(string themeName);
        
        protected bool _started = false;
        protected bool _enabled = false;

        protected void OnEnable()
        {
            _enabled = true;
            if (!_started)
            {
                return;
            }

            EnableHelper();
        }

        protected void Start()
        {
            _started = true;
            if (!_enabled)
            {
                return;
            }

            EnableHelper();
        }

        protected void OnDisable()
        {
            _enabled = false;
            
            GM.Instance.OnThemeChangeEvent -= SetTheme;
        }
        
        private void EnableHelper()
        {
            if (GM.Instance.Usable)
            {
                SetTheme(GM.Instance.GetTheme());
            }

            GM.Instance.OnThemeChangeEvent += SetTheme;
        }
    }
}