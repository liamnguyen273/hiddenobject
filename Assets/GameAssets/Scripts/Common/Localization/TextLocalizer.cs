using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.brg.Common.Localization
{
    public class TextLocalizer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMeshPro;

        private string _stringKey;
        private string _rawString;

        private Dictionary<string, string> _params;

        public string RawString
        {
            get => _rawString;
            set
            {
                _stringKey = TextLocalizationManager.STR_IS_RAW;
                _rawString = value;

                RefreshAppearance();
            }
        }

        public string Key
        {
            get => _stringKey;
            set
            {
                _stringKey = value;
                _rawString = TextLocalizationManager.Instance.Localize(_stringKey);

                RefreshAppearance();
            }
        }

        public void SetParams(string paramName, object value)
        {
            if (_params == null)
            {
                _params = new Dictionary<string, string>();
            }

            _params[$"{{{paramName}}}"] = value.ToString();

            RefreshAppearance();
        }

        private void RefreshAppearance()
        {
            var text = _rawString;

            if (_params != null)
            {
                foreach (var pair in _params)
                {
                    text = text.Replace(pair.Key, pair.Value);
                }
            }

            _textMeshPro.text = text;
        }
    }
}