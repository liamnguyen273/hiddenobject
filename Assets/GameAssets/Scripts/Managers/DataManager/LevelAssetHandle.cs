using System.Collections.Generic;
using com.brg.Common;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinycastle.StickerBooker
{
    public class LevelAssetHandle
    {
        internal bool RequestedThumbnail;
        internal AsyncOperationHandle<Sprite> ThumbnailHandle;
        internal bool RequestedFullImage;
        internal AsyncOperationHandle<Sprite> FullImageHandle;
        internal bool RequestedStickers;
        internal AsyncOperationHandle<IList<Sprite>> StickersHandle;

        public Sprite ThumbnailSprite => ThumbnailHandle.Result;
        public Sprite FullSprite => FullImageHandle.Result;
        public Dictionary<int, StickerDefinition> Stickers { get; internal set; }
        
        internal bool StickerProcessed = false;

        public bool FullyLoaded => ThumbnailLoaded && FullImageLoaded && StickersLoaded;

        public bool ThumbnailLoaded => RequestedThumbnail && ThumbnailHandle.IsDone &&
                                       ThumbnailHandle.Status == AsyncOperationStatus.Succeeded;

        public bool FullImageLoaded => RequestedFullImage && FullImageHandle.IsDone &&
                                       FullImageHandle.Status == AsyncOperationStatus.Succeeded;

        public bool StickersLoaded => StickersHandleCompleted && StickerProcessed && Stickers != null;

        public bool FullyReleased => !RequestedFullImage && !RequestedThumbnail && !RequestedStickers;

        internal bool StickersHandleCompleted => RequestedStickers && StickersHandle.IsDone &&
                                                 StickersHandle.Status == AsyncOperationStatus.Succeeded;

        public IProgressItem GetProgressItemForFullImage()
        {
            return new SingleProgressItem((out bool success) =>
            {
                success = FullImageLoaded;
                return FullImageHandle.IsDone;
            }, () => FullImageHandle.PercentComplete, null, 1000);
        }
        
        public IProgressItem GetProgressItemForThumbnail()
        {
            return new SingleProgressItem((out bool success) =>
            {
                success = ThumbnailLoaded;
                return ThumbnailHandle.IsDone;
            }, () => ThumbnailHandle.PercentComplete, null, 1000);
        }

        public IProgressItem GetProgressItemForStickers()
        {
            return new SingleProgressItem((out bool success) =>
            {
                success = Stickers != null;
                return StickersHandleCompleted && StickerProcessed;
            }, () => ThumbnailHandle.PercentComplete, null, 1000);
        }

        public IProgressItem MakeProgressItem(bool loadFullImage, bool loadThumbnailImage, bool loadStickers)
        {
            var progresses = new List<IProgressItem>();
            if (loadFullImage) progresses.Add(GetProgressItemForFullImage());
            if (loadThumbnailImage) progresses.Add(GetProgressItemForThumbnail());
            if (loadStickers) progresses.Add(GetProgressItemForStickers());

            return new ProgressItemGroup(progresses);
        }
    }
}