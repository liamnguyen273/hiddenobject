using com.brg.Common.Logging;
using UnityEngine;

namespace com.brg.Common.UI.Hud
{
    public class UIHud : MonoBehaviour, IInitializable, IActivatable
    {
        [SerializeField] private string _explicitName = "";
	    
        private RectTransform _rect;

        public InitializationState State { get; protected set; } = InitializationState.NOT_INITIALIZED;
        public bool Usable => State == InitializationState.SUCCESSFUL;
        public ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        public IProgressItem GetInitializeProgressItem()
        {
            return new SingleProgressItem((out bool success) =>
            {
                success = State == InitializationState.SUCCESSFUL;
                return State > InitializationState.INITIALIZING;
            }, null, null, 100);
        }

        public IProgressItem GetPrepareActivateProgressItem()
        {
            return new ImmediateProgressItem();
        }        
        
        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            return new ImmediateProgressItem();
        }

        public void Initialize()
        {
            if (_explicitName == string.Empty || _explicitName == "")
            {
                _explicitName = name;
                LogObj.Default.Warn(_explicitName, $"UIPopup is missing explicit name, will use name of game object ({name})");
            }

            _rect = GetComponent<RectTransform>();
            
            _rect.anchoredPosition = new Vector2(0f, 0f);
            
            InitializeBehaviour();
			
            State = InitializationState.SUCCESSFUL;
        }

        public virtual void Activate()
        {
            gameObject.SetActive(true);
            LogObj.Default.Info(_explicitName, "Activated.");
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            LogObj.Default.Info(_explicitName, "Deactivated.");
        }

        public virtual void PrepareActivate()
        {
            // Do nothing   
            LogObj.Default.Info(_explicitName, "Prepare activating.");
        }
        
        public virtual void PrepareDeactivate()
        {
            // Do nothing
            LogObj.Default.Info(_explicitName, "Prepare deactivating.");
        }

        internal virtual void InitializeBehaviour()
        {
            
        }
    }
}