using com.brg.Common.UI;
using System;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourNoAds : UIPopupBehaviour
    {
        public void OnBuyButton()
        {
            GM.Instance.Purchases.RequestPurchase(GlobalConstants.NO_AD_ITEM_NAME,
                () =>
                {
                    
                });
            Popup.Hide();
        }
    }
}