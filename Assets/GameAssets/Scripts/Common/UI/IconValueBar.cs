using com.brg.Common.Localization;
using UnityEngine;

namespace com.brg.Common.UI
{
    public class IconValueBar : MonoBehaviour
    {
        [SerializeField] private TextLocalizer _valueText;
        public void SetValue(int value)
        {
            _valueText.RawString = value.ToString();
        }

        public void SetString(string value)
        {
            _valueText.RawString = value;
        }
    }
}
