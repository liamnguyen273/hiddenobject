using System;
using com.brg.Common;
using com.brg.Common.Logging;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public abstract class ResourceWatcher : MonoBehaviour
    {
        [Header("Params")] 
        [SerializeField] protected string _resource;
        
        protected int _cachedValue = 0;

        protected bool _started = false;
        protected bool _enabled = false;

        protected virtual void OnEnable()
        {
            _enabled = true;
            if (!_started)
            {
                return;
            }

            EnableHelper();
        }

        private void Start()
        {
            _started = true;
            if (!_enabled)
            {
                return;
            }

            EnableHelper();
        }

        protected virtual void OnDisable()
        {
            _enabled = false;
            
            GM.Instance.Player.UsableEvent -= InitializeWatcher;
            GM.Instance.Player.OnResourceChangeEvent -= OnResourceChangeEvent;
        }
        
        private void EnableHelper()
        {
            if (GM.Instance.Player.Usable)
            {
                InitializeWatcher();
            }
            else
            {
                OnResourceChange(0, 0);
                GM.Instance.Player.UsableEvent += InitializeWatcher;
            }
        }

        protected virtual void InitializeWatcher()
        {
            var value = GM.Instance.Player.GetResource(_resource);
            OnResourceChange(value, 0);
            GM.Instance.Player.OnResourceChangeEvent += OnResourceChangeEvent;
        }

        protected void OnResourceChangeEvent(string resource, int newValue, int change)
        {
            if (resource == _resource)
            {
                OnResourceChange(newValue, change);
            }
        }

        protected virtual void OnResourceChange(int newValue, int change)
        {
            _cachedValue = newValue;
        }
    }
}