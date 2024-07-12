using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.tinycastle.StickerBooker
{
    public partial class DataManager
    {
        private enum LevelAssetType
        {
            FULL_IMAGE = 1,
            THUMBNAIL = 2,
            STICKERS = 3,
        }
        
        // Entries
        private Dictionary<string, ProductEntry> _products;
        private Dictionary<string, LevelEntry> _levelEntries;
        private Dictionary<string, LevelCsv> _levelCsvs;
        private Dictionary<string, Sprite> _resourceIcons;
        private Dictionary<string, Sprite> _avatars;
        private List<string> _leaderboardNames;

        private Dictionary<int, StemPackCsv> _stemDefs;

        private Dictionary<string, List<LevelEntry>> _sortedLevels;
        private Dictionary<string, Dictionary<int, LevelEntry>> _sortedLevelsWithDict;
        
        // Sprites and stuff
        private Dictionary<string, LevelAssetHandle> _levelAssetHandles;

        public StemPackCsv GetStemDefinition(int number)
        {
            return _stemDefs.GetValueOrDefault(number, null);
        }
        
        public LevelEntry GetLevelEntry(string levelName)
        {
            if (!_levelEntries.ContainsKey(levelName))
            {
                return null;
            }
            
            // Set sticker count in case
            var entry = _levelEntries[levelName];

            if (_levelCsvs.ContainsKey(levelName))
            {
                var csv = _levelCsvs[levelName];
                entry.TotalStickerCount = csv.StickerList.Count;
            }
            
            return entry;
        }

        public LevelEntry GetPreviousEntry(string levelName)
        {
            if (!_levelEntries.TryGetValue(levelName, out var entry))
            {
                Log.Warn($"Level \"{levelName}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }

            var order = entry!.SortOrder - 1;
            var bundle = entry!.Bundle;

            if (!_sortedLevelsWithDict.TryGetValue(bundle, out var dict))
            {
                Log.Warn($"Bundle \"{bundle}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }
            
            if (!dict.TryGetValue(order, out var prevEntry))
            {
                Log.Warn($"Bundle \"{bundle}\" does not have level with sort order {order}, cannot get prev entry, returning null.");
                return null;
            }

            return prevEntry;
        }

        public LevelEntry GetNextEntry(string levelName)
        {
            if (!_levelEntries.TryGetValue(levelName, out var entry))
            {
                Log.Warn($"Level \"{levelName}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }

            var order = entry!.SortOrder + 1;
            var bundle = entry!.Bundle;

            if (!_sortedLevelsWithDict.TryGetValue(bundle, out var dict))
            {
                Log.Warn($"Bundle \"{bundle}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }
            
            if (!dict.TryGetValue(order, out var nextEntry))
            {
                Log.Warn($"Bundle \"{bundle}\" does not have level with sort order {order}, cannot get next entry, returning null.");
                return null;
            }

            return nextEntry;
        }
        
        public List<LevelEntry> GetSortedLevelEntries(string bundle)
        {
            return _sortedLevels.TryGetValue(bundle, out var list) ? list : null;
        }

        public ProductEntry GetProductEntry(string id)
        {
            if (!_products.ContainsKey(id))
            {
                Log.Warn($"Product \"{id}\" doesn't exist, returning null.");
                return null;
            }
            
            return _products[id];
        }

        public Dictionary<string, ProductEntry> GetAllProducts()
        {
            return _products;
        }

        public bool RequestLoadAssetPack(string levelName, out LevelAssetHandle assetHandle,
            bool loadFullImage = true,
            bool loadThumbnailImage = true,
            bool loadStickers = true)
        {
            // Validate
            if (!_levelEntries.ContainsKey(levelName))
            {
                Log.Warn($"Cannot load assets from level \"{levelName}\n because the game contains no such entries");
                assetHandle = null;
                return false;
            }
            
            // Create if missing
            CreateAssetPack(levelName);

            var handle = _levelAssetHandles[levelName];

            if (loadFullImage && !handle.RequestedFullImage)
            {
                try
                {
                    handle.FullImageHandle = Addressables.LoadAssetAsync<Sprite>($"{FULL_IMAGE_KEY}/{levelName}.png");
                    handle.RequestedFullImage = true;
                    handle.FullImageHandle.Completed += h => HandleLevelAssetLoadDone(levelName, LevelAssetType.FULL_IMAGE);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Info("Cannot have");
                    throw;
                }
            }

            if (loadThumbnailImage && !handle.RequestedThumbnail)
            {
                handle.ThumbnailHandle = Addressables.LoadAssetAsync<Sprite>($"{THUMBNAIL_IMAGE_KEY}/{levelName}.png");
                handle.RequestedThumbnail = true;
                handle.ThumbnailHandle.Completed += h => HandleLevelAssetLoadDone(levelName, LevelAssetType.THUMBNAIL);
            }
            
            if (loadStickers && !handle.RequestedStickers)
            {
                var levelCsv = _levelCsvs[levelName];
                var stickerKeys = levelCsv.StickerList
                    .Select(x => $"{STICKERS_KEY}/{levelName}/{x.Key}.png").ToList();
                
                handle.StickersHandle = Addressables.LoadAssetsAsync<Sprite>(stickerKeys, _ => { }, Addressables.MergeMode.Union, false);
                handle.RequestedStickers = true;
                handle.StickersHandle.Completed += h => HandleLevelAssetLoadDone(levelName, LevelAssetType.STICKERS);
            }

            assetHandle = handle;
            return true;
        }
        
        public LevelAssetHandle GetAssetLoadHandle(string levelName)
        {
            if (!_levelAssetHandles.ContainsKey(levelName))
            {
                return null;
            }

            return _levelAssetHandles[levelName];
        }

        public void ReleaseLevelAsset(string levelName, 
            bool releaseFullImage = false,
            bool releaseThumbnailImage = false,
            bool releaseStickers = false)
        {
            if (!_levelAssetHandles.ContainsKey(levelName))
            {
                return;
            }

            var handle = _levelAssetHandles[levelName];

            if (releaseFullImage && handle.RequestedFullImage)
            {
                Addressables.Release(handle.FullImageHandle);
                handle.RequestedFullImage = false;
            }
            
            if (releaseThumbnailImage && handle.RequestedThumbnail)
            {
                Addressables.Release(handle.ThumbnailHandle);
                handle.RequestedThumbnail = false;
            }
            
            if (releaseStickers && handle.RequestedStickers)
            {
                Addressables.Release(handle.StickersHandle);
                handle.RequestedStickers = false;
                handle.Stickers = null;
                handle.StickerProcessed = false;
            }

            if (handle.FullyReleased)
            {
                // Remove
                _levelAssetHandles.Remove(levelName);
            }
        }

        public Sprite GetResourceIcon(string name)
        {
            if (_resourceIcons == null || !_resourceIcons.ContainsKey(name))
            {
                Log.Warn($"Resource icon for \"{name}\" does not exist, returning null.");
                return null;
            }

            return _resourceIcons[name];
        }
        
        public Sprite GetAvatar(string name)
        {
            if (_avatars == null || !_avatars.ContainsKey(name))
            {
                Log.Warn($"Avatar for \"{name}\" does not exist, returning null.");
                return null;
            }

            return _avatars[name];
        }

        public List<string> GetLeaderboardNames()
        {
            return _leaderboardNames;
        }

        private void CreateAssetPack(string levelName)
        {
            if (!_levelAssetHandles.ContainsKey(levelName))
            {
                _levelAssetHandles.Add(levelName, new LevelAssetHandle());
            }
        }

        private void HandleLevelAssetLoadDone(string levelName, LevelAssetType type)
        {
            if (type == LevelAssetType.STICKERS)
            {
                var handle = _levelAssetHandles[levelName];
                var entry = _levelEntries[levelName];
                var csv = _levelCsvs[levelName];
                ProcessStickers(ref handle, ref entry, in csv);
            }
        }
        
        public void ProcessStickers(ref LevelAssetHandle handle, ref LevelEntry entry, in LevelCsv levelCsv)
        {
            if (handle.StickerProcessed || !handle.StickersHandleCompleted) return;

            var list = handle.StickersHandle.Result;

            handle.Stickers = new Dictionary<int, StickerDefinition>();
            foreach (var sprite in list)
            {
                if (sprite == null)
                {
                    continue;
                }
                
                var name = sprite.name;

                if (!levelCsv.StickerList.ContainsKey(name))
                {
                    // Doesn't contain the sticker
                    continue;
                }
                
                if (!levelCsv.NumberPositions.TryGetValue(name, out var numberPos))
                {
                    numberPos = Vector2.zero;
                }

                var def = new StickerDefinition()
                {
                    Number = levelCsv.StickerList[name],
                    Name = name,
                    NormalizedPosition = levelCsv.StickerPositions[name],
                    AbsoluteNumberingPosition = numberPos,
                    Sprite = sprite
                };
                
                handle.Stickers.Add(def.Number, def);
            }
            
            // Validating
            var count = handle.Stickers.Count;

            if (count < 50)
            {
                Log.Warn($"Level \"{entry.Id}\" only has {count} stickers, which is smaller that the recommended amount ({50}).");
            }

            if (count != levelCsv.StickerList.Count)
            {
                Log.Warn($"Level \"{entry.Id}\" only loaded {count} stickers, but registered {levelCsv.StickerList.Count} stickers.");
            }

            handle.StickerProcessed = true;
        }
    }
}