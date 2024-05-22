using System;

namespace com.tinycastle.StickerBooker
{
    public partial class GM
    {
        public void HandleOnAdFreeButton()
        {
            if (Player.GetAdFree())
            {
                Log.Warn("Already has ad free, this method should not be invoked in the first place.");
            }
            else
            {
                var popup = Popups.GetPopup<PopupBehaviourNoAds>(out var behaviour);
                popup.Show();
            }
        }
    }
}