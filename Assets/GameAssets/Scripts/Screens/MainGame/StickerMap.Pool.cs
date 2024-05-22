using System.Collections.Generic;
using System.Linq;
using com.brg.Utilities;

namespace com.tinycastle.StickerBooker
{
    public partial class StickerMap
    {
        private void InitializePool()
        {
            ExtendPool(_poolCount);
        }

        private StaticSticker GetStaticSticker()
        {
            if (_stickerPool.Count < 0)
            {
                ExtendPool(3);
            }

            var sticker = _stickerPool.First();
            _stickerPool.Remove(sticker);
            sticker.SetGOActive(true);
            sticker.transform.SetParent(_inUseStickerHost);
            return sticker;
        }

        private void ReturnStaticSticker(StaticSticker sticker)
        {
            sticker.transform.SetParent(_unusedStickerHost);
            sticker.ResetSticker();
            _inUseStickers.Remove(sticker);
            _stickerPool.Add(sticker);
        }

        private void ExtendPool(int count)
        {
            if (_stickerPool == null)
            {
                _stickerPool = new HashSet<StaticSticker>();
            }

            if (_inUseStickers == null)
            {
                _inUseStickers = new HashSet<StaticSticker>();
            }
            
            for (var i = 0; i < count; ++i)
            {
                var obj = Instantiate(_staticStickerPrefab);
                var comp = obj.GetComponent<StaticSticker>();
                comp.transform.SetParent(_unusedStickerHost);
                comp.ResetSticker();
                _stickerPool.Add(comp);
            }
        }
    }
}