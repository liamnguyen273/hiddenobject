using System;
using UnityEngine;

namespace com.brg.Common.UI
{
    public class UIPopupBehaviour : MonoBehaviour
    {
        [SerializeField] private EventWrapper _showStartEvent;
        [SerializeField] private EventWrapper _showEndEvent;
        [SerializeField] private EventWrapper _hideStartEvent;
        [SerializeField] private EventWrapper _hideEndEvent;

        public EventWrapper ShowStartEvent => _showStartEvent;
        public EventWrapper ShowEndEvent => _showEndEvent;
        public EventWrapper HideStartEvent => _hideStartEvent;
        public EventWrapper HideEndEvent => _hideEndEvent;

        private Action _oneTimeShowStart;
        private Action _oneTimeShowEnd;
        private Action _oneTimeHideStart;
        private Action _oneTimeHideEnd;

        public UIPopup Popup { get; internal set; }

        internal virtual void Initialize()
        {
            _showStartEvent ??= new EventWrapper();
            _showEndEvent ??= new EventWrapper();
            _hideStartEvent ??= new EventWrapper();
            _hideEndEvent ??= new EventWrapper();
            
            _showStartEvent += InnateOnShowStart;
            _showEndEvent += InnateOnShowEnd;
            _hideStartEvent += InnateOnHideStart;
            _hideEndEvent += InnateOnHideEnd;
        }

        public UIPopupBehaviour OnShowStart(Action action)
        {
            _oneTimeShowStart = action;
            _showStartEvent += _oneTimeShowStart;
            return this;
        }
        
        public UIPopupBehaviour OnShowEnd(Action action)
        {
            _oneTimeShowEnd = action;
            _showEndEvent += _oneTimeShowEnd;
            return this;
        }
        
        public UIPopupBehaviour OnHideStart(Action action)
        {
            _oneTimeHideStart = action;
            _hideStartEvent += _oneTimeHideStart;
            return this;
        }
        
        public UIPopupBehaviour OnHideEnd(Action action)
        {
            _oneTimeHideEnd = action;
            _hideEndEvent += _oneTimeHideEnd;
            return this;
        }

        internal void CleanUpOnShowSessionCompleted()
        {
            _showStartEvent -= _oneTimeShowStart;
            _showEndEvent -= _oneTimeShowEnd;
            _hideStartEvent -= _oneTimeHideStart;
            _hideEndEvent -= _oneTimeHideEnd;
        }

        protected virtual void InnateOnShowStart() { }
        protected virtual void InnateOnShowEnd() { }
        protected virtual void InnateOnHideStart() { }
        protected virtual void InnateOnHideEnd() { }
    }
}