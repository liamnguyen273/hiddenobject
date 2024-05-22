using com.brg.Common;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class ParticleCallbackHelper : MonoBehaviour
    {
        [SerializeField] private EventWrapper _event;

        public EventWrapper Event => _event;
        
        private void OnParticleSystemStopped()
        {
            _event?.Invoke();
        }
    }
}

