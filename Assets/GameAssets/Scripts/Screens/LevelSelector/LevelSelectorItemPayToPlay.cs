using System;
using System.Diagnostics;
using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Common.UI;
using com.brg.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class LevelSelectorItemPayToPlay : LevelSelectorItem
    {
        [SerializeField] private TextLocalizer _priceText;

        protected override void Refresh()
        {
            base.Refresh();

            _priceText.RawString = _entry.GetPlayPrice().ToString();
        }

        protected override void PlayButtonBehaviour()
        {
            if (_entry == null) return;
            
            var price = GM.Instance.IsCheat ? 0 : _entry.GetPlayPrice();
            
            if (_entry != null && GM.Instance.Player.UseResource(GlobalConstants.STAMP_RESOURCE, price))
            {
                GM.Instance.RequestPlayLevel(_entry.Id);
            }
            else
            {
                var floater = GM.Instance.Effects.MakeFloater(transform);
                floater.Set("Not enough\nkeys!", 200f);
            }
        }
    }
}