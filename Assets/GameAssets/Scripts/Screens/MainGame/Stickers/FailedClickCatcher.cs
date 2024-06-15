using com.brg.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinycastle.StickerBooker
{
    public class FailedClickCatcher: MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private EventWrapper<PointerEventData> OnClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(eventData);
        }
    }
}