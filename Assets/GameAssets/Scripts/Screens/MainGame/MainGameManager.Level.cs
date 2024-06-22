using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.Utilities;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager
    {
        private int _saveProgressMeter = 0;
        
        private void PerformLevelLoad()
        {
            _entry = GM.Instance.Data.GetLevelEntry(_currentId);

            if (_entry.IsMultiplayer)
            {
                // TODO
                var randomLevelId = GM.Instance.GetRandomAvailableLevel();
                Log.Info($"Chosen level for multiplayer is: {randomLevelId}");
                GM.Instance.Data.RequestLoadAssetPack(randomLevelId, out _assetHandle);
                _hasAssetLoad = true;
            }
            else
            {
                GM.Instance.Data.RequestLoadAssetPack(_currentId, out _assetHandle);
                _hasAssetLoad = true;
            }
        }

        private bool _hasAssetLoad = false;
        private void UpdateLoadAssetPack()
        {
            if (GameState != GameState.LOADING || !_hasAssetLoad)
            {
                return;
            }

            if (_assetHandle.FullyLoaded)
            {
                OnAssetFullyLoaded();
                _hasAssetLoad = false;
            }
        }

        private void OnAssetFullyLoaded()
        {
            _hud.SetEntry(_entry);
            _map.SetMap(_assetHandle.ThumbnailSprite, _assetHandle.FullSprite, _assetHandle.Stickers);
            
            PopulateStickerBookkeeping(false);

            GameState = GameState.LOAD_DONE;
        }

        private void PopulateStickerBookkeeping(bool forceAsNew)
        {
            Log.Info($"Populating map with sticker for level {_currentId}...");
            
            LoadProgress(out var completed, out var hasProgress, out var time, out var attachedStickers);
            
            var shouldPlayNormally = !completed || hasProgress || forceAsNew;
            
            _gameTimer = time;
            var stickerList = _map.GetStickerList();
            var attachedSet = attachedStickers.ToHashSet();
            
            _allStickers = stickerList;
            _allFindStickers = new List<StaticSticker>();
            _findStickers = new List<StaticSticker>();
            _foundStickers = new List<StaticSticker>();
            
            var seed = 19823046 + _entry.SortOrder * 100 + _entry.SortOrder;
            var rng = new System.Random(seed);
            var total = stickerList.Count > 50 ? 50 : 35;
            
            foreach (var i in Enumerable.Range(0, total).OrderBy(_ => rng.Next()))
            {
                var sticker = _allStickers[i];
                _allFindStickers.Add(sticker);
                _findStickers.Add(sticker);
            }
            
            // if (shouldPlayNormally)
            // {
            //     _leftOverStickers = stickerList
            //         .Where(x => !attachedSet.Contains(x.GetDefinition().Number))
            //         .OrderBy(x => x.GetDefinition().Number)
            //         .ToList();
            //
            //     _attachedStickers = stickerList
            //         .Where(x => attachedSet.Contains(x.GetDefinition().Number))
            //         .ToList();
            // }
            // else
            // {
            //     _leftOverStickers = new List<StaticSticker>();    // Empty
            //     _attachedStickers = stickerList.ToList();   // Clone
            // }

            // Toggle states of stickers
            foreach (var sticker in _allStickers)
            {
                sticker.SetState(StaticStickerState.COLORED);
            }
            
            // TODO
            // foreach (var sticker in _leftOverStickers)
            // {
            //     sticker.SetState(StaticStickerState.OUTLINE);
            // }

            // _hud.UpdateProgress(_attachedStickers.Count, _allStickers.Count);
            _hud.UpdateProgress(_foundStickers.Count, _allFindStickers.Count);
        }
        
        private void LoadProgress(out bool completed, out bool hasProgress, out int time, out int[] attachedStickers)
        {
            GM.Instance.Player.GetLevelState(_currentId, out var completion, out hasProgress, out var progress);
            
            completed = completion.Completed;
            // time = completion.Completed ? completion.BestTime : progress.CurrentTime;
            // attachedStickers = (_entry.IsTimeAttack || _entry.IsMultiplayer) ? new int[0] : progress.AttachedStickers ?? new int[0];
            //
            // Log.Info($"Level progress loaded for level {_currentId}: Completion: {completed}, Time: {time}, Attached sticker count: {attachedStickers.Length}");
            hasProgress = false;
            time = 0;
            attachedStickers = Array.Empty<int>();
        }
        
        private void SaveProgress(bool forceSave = false)
        {
            if (_entry.IsTimeAttack || _entry.IsMultiplayer) return;
            
            // TODO
            
            // if (CanonicalState != GameState.IN_GAME)
            // {
            //     Log.Warn($"Game state is {CanonicalState}, should not save progress at this state.");
            //     return;
            // }
            //
            // if (forceSave) _saveProgressMeter = 999;
            // else ++_saveProgressMeter;
            //
            // if (_saveProgressMeter < 4) return;
            //
            // Log.Info($"Level progress save requested for level ${_currentId}");
            //
            // var content = _attachedStickers.Select(x => x.GetDefinition().Number).ToArray();
            // var time = _gameTimer;
            //
            // GM.Instance.Player.SetLevelProgress(_currentId, new LevelProgress()
            // {
            //     CurrentTime = (int)time,
            //     AttachedStickers = content
            // });
            // _saveProgressMeter = 0;
            //
            // GM.Instance.Player.RequestSaveData(true, false, false);
        }

        private void SaveLevelComplete()
        {
            if (CanonicalState != GameState.COMPLETING && CanonicalState != GameState.COMPLETED)
            {
                Log.Warn($"Game state is {CanonicalState}, should not save completion at this state.");
                return;
            }
            
            Log.Info($"Level ${_currentId} should be marked as completed, and remove the progress");
            
            // Save completion
            GM.Instance.Player.SetLevelAsComplete(_currentId, (int)_gameTimer);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) return;
            
            if (GameState == GameState.IN_GAME)
            {
                SaveProgress(true);
            }
        }
    }
}