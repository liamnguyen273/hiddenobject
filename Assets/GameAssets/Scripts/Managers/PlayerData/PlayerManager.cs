using com.brg.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;

namespace com.tinycastle.StickerBooker
{
    public partial class PlayerManager
    {
        private PlayerData _playerData;
        private PlayerProgressData _playerProgressData;
        private PlayerPreference _preference;

        private SyncPlayerData _cachedSyncData;

        public bool CheckCompletedWithoutExistence(string level)
        {
            return _playerData.CompletedLevels.ContainsKey(level) && _playerData.CompletedLevels[level].Completed;
        }

        public bool CheckHasProgress(string level)
        {
            return _playerProgressData.InProgressLevels.ContainsKey(level);
        }

        public void GetLevelState(string level, out CompletedLevelData completion, out bool hasProgress, out LevelProgress progress)
        {
            // Get completion
            if (!_playerData.CompletedLevels.ContainsKey(level))
            {
                completion = new CompletedLevelData(completed: false, bestTime: int.MaxValue);
            }
            else
            {
                completion = _playerData.CompletedLevels[level];
            }
            
            // Get progress, if has progress but no attached stickers, might as well start anew
            hasProgress = _playerProgressData.InProgressLevels.TryGetValue(level, out progress);
            if (!hasProgress || progress.AttachedStickers == null || progress.AttachedStickers.Length == 0)
            {
                if (hasProgress && (progress.AttachedStickers == null || progress.AttachedStickers.Length == 0))
                {
                    Log.Warn($"Attached sticker in progress of level {level} is null or has 0 elements, will return as if no progress is found.");
                }
                
                progress = new LevelProgress();
                hasProgress = false;
            }
            else
            {
                var other = _playerProgressData.InProgressLevels[level];
                progress = new LevelProgress()
                {
                    CurrentTime = other.CurrentTime,
                    AttachedStickers = other.AttachedStickers.ToArray()
                };
            }
        }

        public void SetLevelAsComplete(string level, int time)
        {
            if (GM.Instance.Data.GetLevelEntry(level) == null)
            {
                Log.Warn($"Level \"{level}\" doesn't exist. Cannot add completion.");
                return;
            }
            
            Log.Info($"Now marking level {level} as completed.");
            
            // Set completion struct
            var bestTime = int.MinValue;
            if (_playerData.CompletedLevels.TryGetValue(level, out var completedLevel))
            {
                bestTime = completedLevel.BestTime;
            }

            if (time > bestTime)
            {
                _playerData.CompletedLevels[level] = new CompletedLevelData(completed: true, bestTime: time);
            }
            
            // Clear level progress
            if (_playerProgressData.InProgressLevels.ContainsKey(level))
            {
                _playerProgressData.InProgressLevels.Remove(level);
            }
            
            RequestSaveData(true, true, false);
        }

        public void ClearLevelProgress(string level)
        {
            _playerProgressData.InProgressLevels.Remove(level);
            RequestSaveData(false, true, false);
        }
        
        public void SetLevelProgress(string level, LevelProgress progress)
        {
            if (GM.Instance.Data.GetLevelEntry(level) == null)
            {
                Log.Warn($"Level \"{level}\" doesn't exist. Cannot add progress.");
                return;
            }

            // Check completion
            // if (_playerData.CompletedLevels.ContainsKey(level))
            // {
            //     Log.Warn($"Level \"{level}\"is completed. Will not add progress.");
            //     return;
            // }

            // Don't set if no attached stickers
            if (progress.AttachedStickers != null && progress.AttachedStickers.Length != 0)
            {
                _playerProgressData.InProgressLevels[level] = new LevelProgress()
                {
                    CurrentTime = progress.CurrentTime,
                    AttachedStickers = progress.AttachedStickers.ToArray()
                };
            }

            RequestSaveData(false, true, false);
        }

        public bool SetAsOwn(string id)
        {
            // TODO: Validate

            if (!_playerData.Ownerships.Add(id)) return false;
            RequestSaveData(true, false, false);
            
            OnOwnEvent?.Invoke(id);
            
            return true;
        }

        public bool Own(string id)
        {
            return _playerData.Ownerships.Contains(id);
        }

        public bool GetAdFree()
        {
            return _playerData.Ownerships.Contains(GlobalConstants.NO_AD_ITEM_NAME);
        }

        public bool GetTutorialPlayed()
        {
            return _playerData.TutorialPlayed;
        }

        public void SetTutorialPlayed()
        {
            _playerData.TutorialPlayed = true;
            RequestSaveData(true, false, false);
        }

        public bool GetTutorialTimeAttackPlayed()
        {
            return _playerData.TimeAttackTutorialPlayed;
        }
        
        public bool GetTutorialMultiplayerPlayed()
        {
            return _playerData.MultiplayerTutorialPlayed;
        }
        
        public void SetTutorialTimeAttackPlayed()
        {
            _playerData.TimeAttackTutorialPlayed = true;
            RequestSaveData(true, false, false);
        }
        
        public void SetTutorialMultiplayerPlayed()
        {
            _playerData.MultiplayerTutorialPlayed = true;
            RequestSaveData(true, false, false);
        }

        public int GetResource(string name)
        {
            if (!_playerData.Resources.ContainsKey(name))
            {
                Log.Warn($"Resource \"{name}\" doesn't exist in player's data dictionary.");
                return 0;
            }

            return _playerData.Resources[name];
        }

        public bool UseResource(string name, int count, bool doNotSave = false)
        {
            if (count <= 0)
            {
                Log.Warn($"Resource \"{name}\": Cannot use 0 or negative resource.");
                return true;
            }
            
            if (!_playerData.Resources.ContainsKey(name))
            {
                Log.Warn($"Resource \"{name}\" doesn't exist in player's data dictionary.");
                return false;
            }

            if (_playerData.Resources[name] < count)
            {
                Log.Info($"Resource \"{name}\" is not enough to use (use: {count}) is not used (still is {_playerData.Resources[name]}).");
                return false;
            }
            
            _playerData.Resources[name] -= count;
            Log.Info($"Resource \"{name}\" is decreased by {count}, {_playerData.Resources[name]} remaining.");

            if (!doNotSave)
            {
                RequestSaveData(true, false, false);
            }
            
            OnResourceChangeEvent?.Invoke(name, _playerData.Resources[name], -count);
            return true;
        }

        public bool AddResource(string name, int count, bool allowCreateEntry = false, bool doNotSave = false)
        {
            if (count <= 0)
            {
                Log.Warn($"Resource \"{name}\": Cannot add 0 or negative resource.");
                return true;
            }
            
            if (!_playerData.Resources.ContainsKey(name) && !allowCreateEntry)
            {
                Log.Warn($"Resource \"{name}\" doesn't exist in player's data dictionary and the addition call" +
                         $"does not allow creation of this resource.");
                return false;
            }
            else if (!_playerData.Resources.ContainsKey(name))
            {
                Log.Info($"Resource \"{name}\" is created in player's data dictionary.");
                _playerData.Resources.Add(name, 0);
            }

            _playerData.Resources[name] += count;
            Log.Info($"Resource \"{name}\" is increased by {count}, now having {_playerData.Resources[name]}.");
            
            if (!doNotSave) RequestSaveData(true, false, false);
                        
            OnResourceChangeEvent?.Invoke(name, _playerData.Resources[name], count);
            
            return true;
        }

        public void UpdateLeaderboard(string name, int modScore)
        {
            _playerData.Leaderboard.TryAdd(name, 0);
            _playerData.Leaderboard[name] += modScore;
        }

        public Dictionary<string, int> GetLeaderboard()
        {
            return _playerData.Leaderboard;
        }

        public bool GetLeaderboardShouldInitialize()
        {
            return _playerData.Leaderboard == null || _playerData.Leaderboard.Count <= 0;
        }

        public void SetPreference(PlayerPreference newPreference)
        {
            _preference = new PlayerPreference(newPreference);
            RequestSaveData(false, false, true);
        }

        public PlayerPreference GetPreference()
        {
            return new PlayerPreference(_preference);
        }
    }
}