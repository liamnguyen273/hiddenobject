using com.brg.Common.UI.Hud;

namespace com.brg.Common.UI
{
    public partial class PopupManager
    {
        public UIPopup GetPopup<T>(out T behaviour) where T : UIPopupBehaviour
        {
            if (!_popupsByType.ContainsKey(typeof(T)))
            {
                behaviour = null;
                return null;
            }

            var popup = _popupsByType[typeof(T)];
            behaviour = popup.Behaviour as T;
            return popup;
        }
        
        public UIPopup GetPopup<T>(string popupName, out T behaviour) where T : UIPopupBehaviour
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                behaviour = null;
                return null;
            }

            var popup = _popupsByName[popupName];
            behaviour = popup.Behaviour as T; // Note: can be null
            return popup;
        }
        
        public UIPopup GetPopup<T>() where T : UIPopupBehaviour
        {
            return GetPopup<T>(out _);
        }
                
        public UIPopup GetPopup(string popupName)
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            return popup;
        }

        public T GetPopupBehaviour<T>() where T : UIPopupBehaviour
        {
            if (!_popupsByType.ContainsKey(typeof(T)))
            {
                return null;
            }

            var popup = _popupsByType[typeof(T)];
            var behaviour = popup.Behaviour as T;
            return behaviour;
        }
        
        public T GetPopupBehaviour<T>(string popupName) where T : UIPopupBehaviour
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            var behaviour = popup.Behaviour as T;
            return behaviour;
        }
        
        public UIPopupBehaviour GetPopupBehaviour(string popupName)
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            var behaviour = popup.Behaviour;
            return behaviour;
        }
    }
}