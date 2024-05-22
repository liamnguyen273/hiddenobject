using com.brg.Common;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    // Do nothing for now
    public class ShopMenu: MonoManagerBase, IActivatable
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        public void OnRestorePurchaseButton()
        {
            GM.Instance.Purchases.RestorePurchases();
        }
        
        protected override void StartInitializationBehaviour()
        {
            gameObject.SetActive(false);
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            
        }

        public void PrepareActivate()
        {
            
        }

        public void PrepareDeactivate()
        {
           
        }

        public IProgressItem GetPrepareActivateProgressItem()
        {
            return new ImmediateProgressItem();
        }

        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            return new ImmediateProgressItem();
        }
    }
}