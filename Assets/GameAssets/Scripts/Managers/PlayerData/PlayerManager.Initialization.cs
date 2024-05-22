using com.brg.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace com.tinycastle.StickerBooker
{
    public partial class PlayerManager : ManagerBase
    {
        private float _customProgress = 0f;
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;

        protected override IProgressItem MakeProgressItem()
        {
            return new SingleProgressItem(
                (out bool success) =>
                {
                    success = State == InitializationState.SUCCESSFUL;
                    return State > InitializationState.INITIALIZING;
                },
                () => _customProgress,
                null,
                Priority);
        }

        protected override void StartInitializationBehaviour()
        {
            _customProgress = 0f;
            StartInitializationHelper().ContinueWith(_ => EndInitialize(true), TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void EndInitializationBehaviour()
        {
            _customProgress = 1f;
            
            // Create progress items
            LoadProgressItem = new SingleProgressItem((out bool success) =>
            {
                success = true;
                return !_loadFlag;
            }, null, null, Priority);     

            SaveProgressItem = new SingleProgressItem((out bool success) =>
            {
                success = true;
                return !_saveFlagPlayer && !_saveFlagProgress && !_saveFlagPref;
            }, null, null, Priority);
            
            UsableEvent?.Invoke();
        }

        private async Task StartInitializationHelper()
        {
            // Find version 2 data
            var loadPlayerTask = ReadFromDisk<PlayerData>(PlayerPath);
            var loadLevelTask = ReadFromDisk<PlayerProgressData>(LevelProgressPath);
            var loadPreferenceTask = ReadFromPref<PlayerPreference>(PREFERENCE_KEY);
            var loadVer1Task = ReadFromPref<PlayerDataVersion1>(PLAYER_DATA_KEY_VERSION_1);

            var tasks = new List<Task> { loadLevelTask, loadPlayerTask, loadPreferenceTask, loadVer1Task };
            var total = tasks.Count;
            var portion = 0.9f / total;
            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                await finishedTask;
                _customProgress = (total - tasks.Count) * portion;
                tasks.Remove(finishedTask);
            }

            var (foundPlayer, player) = await loadPlayerTask;
            var (foundLevel, level) = await loadLevelTask;
            var (foundPref, pref) = await loadPreferenceTask;
            var (foundVer1, ver1) = await loadVer1Task;
            
            // Resolve data
            if (!foundPlayer && foundVer1)
            {
                ConvertVersion1(ver1, out _playerData, out _playerProgressData);
                
                // Remove pref
                PlayerPrefs.DeleteKey(PLAYER_DATA_KEY_VERSION_1);
            }
            else
            {
                _playerData = player;
                _playerProgressData = level;
            }
            
            _preference = foundPref ? pref : new PlayerPreference();
            _customProgress = 1f;
        }
    }
}