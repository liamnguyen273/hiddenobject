using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.tinycastle.StickerBooker
{
    public partial class DataManager : ManagerBase
    {
        public const string LEVEL_JSON_KEY = "jsons/levels.json";
        public const string PRODUCT_JSON_KEY = "jsons/products.json";
        public const string ICON_LABEL = "icons";
        public const string AVATAR_LABEL = "avatars";

        public const string FULL_IMAGE_KEY = "full_images";
        public const string THUMBNAIL_IMAGE_KEY = "thumbnails";
        public const string STICKERS_KEY = "stickers";
        public const string CSV_KEY = "csvs";
        public const string CSV_NUMBERING_KEY = "numbering_csvs";
        
        private float _progress = 0f;
        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override IProgressItem MakeProgressItem()
        {
            return base.MakeProgressItem();
        }

        protected override void StartInitializationBehaviour()
        {
            InitializeTask().ContinueWith(t => EndInitialize(t.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void EndInitializationBehaviour()
        {
            _levelAssetHandles = new Dictionary<string, LevelAssetHandle>();
        }

        private async Task<bool> InitializeTask()
        {
            _progress = 0f;
            try
            {
                // Load products
                var productHandle = Addressables.LoadAssetAsync<TextAsset>(PRODUCT_JSON_KEY);
                // Load level entries
                var levelEntryHandle = Addressables.LoadAssetAsync<TextAsset>(LEVEL_JSON_KEY);
                // Resource icons
                var iconHandle = Addressables.LoadAssetsAsync<Sprite>(ICON_LABEL, sprite => { });
                var avatarHandle = Addressables.LoadAssetsAsync<Sprite>(AVATAR_LABEL, sprite => { });
                
                Log.Info("Create handles for game assets.");
                
                await Task.WhenAll(productHandle.Task, levelEntryHandle.Task, iconHandle.Task, avatarHandle.Task);
                
                Log.Info("All handles tasks completed.");
            
                Log.Info("a1:", productHandle);
                Log.Info("a2:", productHandle.Result);
                Log.Info("a2-1:", productHandle.Status);
                Log.Info("a3:", productHandle.Result?.text ?? "NO_TEXT");
                var prodText = productHandle.Result.text;

                var levelEntryText = levelEntryHandle.Result.text;
                Log.Info("b:", levelEntryText);
                
                _progress = 0.15f;

                var deserializeProdTask = Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, ProductEntry>>(prodText));
                Log.Info("c:", deserializeProdTask);
                var levelEntryTask = Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, LevelEntry>>(levelEntryText));
                Log.Info("d:", levelEntryTask);
                
                Log.Info("Created tasks for deserialization of jsons.");
                
                await Task.WhenAll(deserializeProdTask, levelEntryTask);

                _products = await deserializeProdTask;
                _levelEntries = await levelEntryTask;
                
                Log.Info("Completed tasks for deserialization of jsons.");

                _resourceIcons = iconHandle.Result.ToDictionary(x => x.name, x => x);
                _avatars = avatarHandle.Result.ToDictionary(x => x.name, x => x);
                _leaderboardNames = _avatars.Select(x => x.Key).Where(x => x != "You").ToList();
                
                Log.Info("Created important fields.");
                
                // Order levels
                var task = Task.Run(() => MakeSortedLevels(in _levelEntries));
                
                Log.Info("Ordered levels.");
                
                // Release handles, since deserialized
                Addressables.Release(productHandle);
                Addressables.Release(levelEntryHandle);
                Addressables.Release(iconHandle);
                Addressables.Release(avatarHandle);
                
                Log.Info("Release handles.");
                
                (_sortedLevels, _sortedLevelsWithDict) = await task;
                _progress = 0.2f;

            }
            catch (Exception e)
            {
                Log.Error("Reading important data failed.");
                Log.Error(e);
                _progress = 1f;
                return false;
            }
            
            _progress = 0.4f;
            Log.Info($"Read product list: {_products.Count} products.");
            Log.Info($"Read level entry list: {_levelEntries.Count} entries.");
            
            // Csv loading
            _levelCsvs = new Dictionary<string, LevelCsv>();
            var taskList = new List<Task<(bool, LevelCsv)>>();

            var total = taskList.Count;
            foreach (var (levelName, entry) in _levelEntries)
            {
                // Default to not playable
                entry.Playable = false;
                taskList.Add(TryLoadLevelCsvs(levelName));
            }

            var successCount = 0;
            while (taskList.Count > 0)
            {
                var task = await Task.WhenAny(taskList);
                var (successful, levelCsv) = await task;

                if (successful)
                {
                    successCount += 1;
                    _levelEntries[levelCsv.Level].Playable = true;
                    _levelEntries[levelCsv.Level].TotalStickerCount = levelCsv.StickerList.Count;
                    
                    _levelCsvs.Add(levelCsv.Level, levelCsv);
                }
                
                taskList.Remove(task);

                _progress = 0.4f + NumberUtilities.LinearLerp(taskList.Count / (float)total, 0f, 0.6f);
            }

            _progress = 1f;
            
            Log.Info($"Read level CSVs list: {successCount}/{_levelEntries.Count} have corresponding CSVs and are playable.");
            
            return true;
        }

        private async Task<(bool success, LevelCsv levelCsv)> TryLoadLevelCsvs(string levelName)
        {                
            var posKey = CSV_KEY + "/" + levelName + ".csv";
            var numKey = CSV_NUMBERING_KEY + "/" + levelName + ".csv";

            var result = new LevelCsv()
            {
                Level = levelName
            };
            
            // 1. Load Position CSV
            bool successful = false;
            Dictionary<string, int> indexedStickers = null;
            Dictionary<string, Vector2> positions = null;
            Dictionary<string, Vector2> numberingPositions = null;
            try
            {
                var posHandle = Addressables.LoadAssetAsync<TextAsset>(posKey);
                var posString = (await posHandle.Task).text;
                
                successful = await Task.Run(() => CsvUtilities.ProcessPositionCsv(posString, out indexedStickers, out positions));

                if (successful)
                {
                    result.StickerList = indexedStickers;
                    result.StickerPositions = positions;
                }
            }
            catch (Exception e)
            { ;
                Log.Warn($"Getting position CSV at {posKey} failed, level should be marked as unplayable.");
                successful = false;
            }

            if (successful)
            {
                // 2. Load numbering CSV (may not exist)
                try
                {
                    var numHandle = Addressables.LoadAssetAsync<TextAsset>(numKey);
                    var numString = (await numHandle.Task).text;
                    
                    await Task.Run(() => CsvUtilities.ProcessNumberingCsv(numString, in indexedStickers, out numberingPositions));
                    result.NumberPositions = numberingPositions;
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    Log.Info($"Getting numbering CSV at {numKey} failed, will use default values");
                    result.NumberPositions = new Dictionary<string, Vector2>();
                }
            }
            
            return (successful, result);
        }

        private (Dictionary<string, List<LevelEntry>> byList, Dictionary<string, Dictionary<int, LevelEntry>> byDict)
            MakeSortedLevels(in Dictionary<string, LevelEntry> entries)
        {
            var resList = new Dictionary<string, List<LevelEntry>>();
            var resDict = new Dictionary<string, Dictionary<int, LevelEntry>>();

            // Add
            foreach (var (key, entry) in entries)
            {
                var bundle = entry.Bundle;

                if (!resList.ContainsKey(bundle))
                {
                    resList.Add(bundle, new List<LevelEntry>());
                }

                if (!resDict.ContainsKey(bundle))
                {
                    resDict.Add(bundle, new Dictionary<int, LevelEntry>());
                }
                
                resList[bundle].Add(entry);
                resDict[bundle][entry.SortOrder] = entry;
            }
            
            // Sort list
            foreach (var (bundle, list) in resList)
            {
                list.Sort((a, b) => a.SortOrder - b.SortOrder);
            }

            return (resList, resDict);
        }
    }
}