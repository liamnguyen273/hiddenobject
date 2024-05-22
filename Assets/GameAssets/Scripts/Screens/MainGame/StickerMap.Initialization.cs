using System.Collections.Generic;
using com.brg.Common;

namespace com.tinycastle.StickerBooker
{
    public partial class StickerMap
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override void StartInitializationBehaviour()
        {
            InitializePool();
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            
        }
    }
}