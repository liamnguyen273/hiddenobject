using com.brg.Common;
using com.brg.Common.Localization;
using System;
using System.Linq;
using com.brg.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public partial class LevelSelector
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        protected override void StartInitializationBehaviour()
        {
            _orderedItems = _itemHost.GetDirectOrderedChildComponents<LevelSelectorItem>().ToList();
            gameObject.SetActive(false);
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            var rect = GetComponent<RectTransform>();
			
            rect.anchoredPosition = new Vector2(0f, 0f);
        }
    }
}