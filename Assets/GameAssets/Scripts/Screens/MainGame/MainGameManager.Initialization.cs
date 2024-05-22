using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override void StartInitializationBehaviour()
        {
            _map.Initialize();
            TimeAttackModule.Manager = this;
            MultiplayerModule.Manager = this;
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            _appearance.SetGOActive(false);
        }
    }
}