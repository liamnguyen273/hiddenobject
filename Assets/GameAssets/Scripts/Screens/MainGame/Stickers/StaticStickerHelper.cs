using com.brg.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tinycastle.StickerBooker
{
    public class StaticStickerHelper : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private EventWrapper OnClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }
    }
}