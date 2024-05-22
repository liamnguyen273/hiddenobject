using System;

namespace com.tinycastle.StickerBooker
{
    public partial class PlayerManager
    {
        public event Action UsableEvent;
        public event Action<string> OnOwnEvent;
        public event Action<string, int, int> OnResourceChangeEvent;
    }
}