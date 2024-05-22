using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Utilities;
using UnityEngine;

namespace com.brg.Common.UI
{
    public static class Popup
    {
        public const string POPUP_NAME_HERE = "popup_dummy";
    }
    
    public partial class PopupManager : MonoManagerBase
    {
        public event Action AllPopupsHiddenEvent;
        public event Action HasPopupEvent;
        
        [SerializeField] private Transform _popupHost;
        
        private Dictionary<Type, UIPopup> _popupsByType;
        private Dictionary<string, UIPopup> _popupsByName;
        
        private List<List<UIPopup>> _activePopupChains;
        private int _activePopupCount;

        public bool HasPopups => _activePopupCount > 0;
        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        
        protected override void StartInitializationBehaviour()
        {
            _popupsByType = new Dictionary<Type, UIPopup>();
            _popupsByName = new Dictionary<string, UIPopup>();
            
            var popupList = _popupHost.GetDirectOrderedChildComponents<UIPopup>();

            var z = 1;
            foreach (var popup in popupList)
            {
                popup.SetManager(this, Log);
                popup.Initialize();
                popup.PopupZ = z;

                var type = popup.Behaviour.GetType();

                if (type != typeof(UIPopupBehaviour))
                {
                    _popupsByType.Add(type, popup);
                }
                
                _popupsByName.Add(popup.ExplicitName, popup);
                
                ++z;
            }
            
            // TODO: Validate
            
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            // Do nothing
            _activePopupChains = new List<List<UIPopup>>();
            _activePopupCount = 0;
        }

        internal void ShowPopup(UIPopup popup, bool immediately, Type chainTo)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort showing.");
                return;
            }
            
            if (popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already shown or transiting, cannot show popup again.");
                return;
            }

            ++_activePopupCount;
            popup.PopupZ = _activePopupCount;
            foreach (var list in _activePopupChains)
            {
                if (list.Count > 0 && list.Last().Behaviour.GetType() == chainTo)
                {
                    Log.Info($"Popup {popup.ExplicitName} chained to list leading with {list[0].ExplicitName}");
                    list.Add(popup);
                    return;
                }
            }

            Log.Info($"Popup {popup.ExplicitName} should show.");
            popup.ManagerShow(immediately);
            _activePopupChains.Add(new List<UIPopup> { popup });
            
            HasPopupEvent?.Invoke();
        }

        internal void ShowPopup(UIPopup popup, bool immediately, string chainTo)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort showing.");
                return;
            }
            
            if (popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already shown or transiting, cannot show popup again.");
                return;
            }

            ++_activePopupCount;
            popup.PopupZ = _activePopupCount;
            popup.transform.SetAsLastSibling();
            foreach (var list in _activePopupChains)
            {
                if (list.Count > 0 && list.Last().ExplicitName == chainTo)
                {
                    Log.Info($"Popup {popup.ExplicitName} chained to list leading with {list[0].ExplicitName}");
                    list.Add(popup);
                    return;
                }
            }

            Log.Info($"Popup {popup.ExplicitName} should show.");
            popup.ManagerShow(immediately);
            _activePopupChains.Add(new List<UIPopup> { popup });
            HasPopupEvent?.Invoke();
        }
        
        internal void HidePopup(UIPopup popup, bool immediately = false, bool forced = false, bool hideChain = true)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort hiding.");
                return;
            }
            
            if (!popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already hidden or transiting, cannot hide again.");
                return;
            }
            
            // Find chain
            int chainIndex = -1;
            int popupIndex = -1;
            foreach (var list in _activePopupChains)
            {
                ++chainIndex;
                popupIndex = -1;
                var found = false;
                foreach (var otherPopup in list)
                {
                    ++popupIndex;
                    if (otherPopup == popup)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (chainIndex >= 0 && popupIndex >= 0 && chainIndex < _activePopupChains.Count && popupIndex < _activePopupChains[chainIndex].Count)
            {
                if (hideChain)
                {
                    Log.Info("Hiding the popup chain...");
                    
                    foreach (var pop in _activePopupChains[chainIndex])
                    {
                        Log.Info($"Popup {pop.ExplicitName} should hide.");
                        pop.ManagerHide(immediately);
                    }

                    _activePopupCount -= _activePopupChains[chainIndex].Count;
                    _activePopupChains.RemoveAt(chainIndex);
                }
                else
                {
                    _activePopupChains[chainIndex].RemoveAt(popupIndex);
                    _activePopupCount -= 1;
                                
                    Log.Info($"Popup {popup.ExplicitName} should hide.");
                    popup.ManagerHide(immediately);
                }
            }
            else
            {
                Log.Warn("Popup does not belong to any chain, this is unexpected.");
                RecalculatePopupCount();
                            
                Log.Info($"Popup {popup.ExplicitName} should hide.");
                popup.ManagerHide(immediately);
            }

            if (_activePopupCount == 0)
            {
                Log.Info($"Invoke hidden popup event.");
                AllPopupsHiddenEvent?.Invoke();
            }
        }

        private void RecalculatePopupCount()
        {
            var before = _activePopupCount;
            
            var count = 0;
            foreach (var chain in _activePopupChains)
            {
                foreach (var popup in chain)
                {
                    ++count;
                }
            }

            _activePopupCount = count;
        }
        
        private bool VerifyExistence(UIPopup popup)
        {
            var exist = _popupsByType.ContainsKey(popup.Behaviour.GetType()) ||
                   _popupsByName.ContainsKey(popup.ExplicitName);

            if (exist && !popup.HasManager)
            {
                popup.SetManager(this, Log);
            }
            
            return exist;
        }
    }
}