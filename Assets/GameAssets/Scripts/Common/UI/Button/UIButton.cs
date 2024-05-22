using System;
using com.brg.Common.Localization;
using JSAM;
using Lean.Transition;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.Common.UI
{
    public class UIButton : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextLocalizer _textLocalizer;
        [SerializeField] private Image _icon;
        [SerializeField] private LeanPlayer _player;
        
        [Header("Params")]
        [SerializeField] private LibrarySounds _tapSound = LibrarySounds.Button;
        [SerializeField] private EventWrapper _buttonClickedEvent;

        private Button _unityButton;
        
        public string LabelKey
        {
            get => _textLocalizer.Key;
            set
            {
                if (_textLocalizer != null)
                {
                    _textLocalizer.Key = value;
                }
            }
        }

        public string LabelRawString
        {
            get => _textLocalizer.RawString;
            set
            {
                if (_textLocalizer != null)
                {
                    _textLocalizer.RawString = value;
                }
            }
        }

        public Sprite Icon
        {
            get => _icon.sprite;
            set
            {
                if (_icon != null)
                {
                    _icon.sprite = value;
                }
            }
        }

        public bool Interactable
        {
            get => _unityButton.interactable;
            set => _unityButton.interactable = value;
        }

        public Button UnityButton => _unityButton;
        public Image IconImage => _icon;

        public EventWrapper Event => _buttonClickedEvent;

        private void Awake()
        {
            _unityButton = GetComponent<Button>();
            _unityButton.onClick.AddListener(OnUnityButtonClick);
        }

        public void OnUnityButtonClick()
        {
            // other things
            AudioManager.PlaySound(_tapSound, gameObject.transform);

            _player.Begin();
            _buttonClickedEvent?.Invoke();
        }
    }
}
