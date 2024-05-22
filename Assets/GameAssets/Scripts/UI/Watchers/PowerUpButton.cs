using com.brg.Common.Localization;
using com.brg.Common.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.tinycastle.StickerBooker
{
    public class PowerUpButton : NumberWatcher
    {
        [Header("Powerup Settings")]
        [SerializeField] private GameObject _normalGroup;
        [SerializeField] private GameObject _zeroGroup;

        protected override void OnResourceChange(int newValue, int change)
        {
            base.OnResourceChange(newValue, change);
            SetCount(newValue);
        }

        private void SetCount(int count)
        {
            _normalGroup.SetActive(count > 0);
            _zeroGroup.SetActive(count <= 0);
        }
    }
}