using com.brg.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public enum BasicInitializePriority
    {
        ESSENTIAL = -100,
        SECONDARY = -10,
        UI_POPUP = 10,
        UI_POPUP_CALLBACK = 100,
    }
}
