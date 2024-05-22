using UnityEngine;
using UnityEngine.UI;

namespace com.brg.Common.UI
{
    public class UIToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject _onObject;
        [SerializeField] private GameObject _offObject;

        [SerializeField] private EventWrapper<bool> _valueChangedEvent;

        public bool ToggleValue
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        public bool ToggleValueNoEvent
        {
            get => _toggle.isOn;
            set
            {
                _toggle.SetIsOnWithoutNotify(value);
                SetToggleSprite(value);
            }
        }

        public void OnToggleValue(bool isOn)
        {
            SetToggleSprite(isOn);
            _valueChangedEvent?.Invoke(isOn);
        }

        private void SetToggleSprite(bool isOn)
        {
            _offObject.SetActive(!isOn);
            _onObject.SetActive(isOn);
        }
    }
}
