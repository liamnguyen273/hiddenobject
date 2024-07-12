using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class GM
    {
        public event Action HasBlockingElementsEvent;
        public event Action NoBlockingElementsEvent;

        private bool _hasBlockingElements = false;

        public string GetRandomAvailableLevel()
        {
            var levels = Data.GetSortedLevelEntries(GlobalConstants.NORMAL_LEVEL_BUNDLE)
                .Where(IsCheat ? ResolveUnlockCondition : x => true)
                .ToList();

            Log.Info($"Getting random level from {levels.Count} levels.");
            
            if (levels.Count == 0) return "level_1";

            var rand = -1;
            rand = Rng.GetInteger(0, levels.Count);
            
            return levels[rand].Id;
        }
        
        public void Vibrate()
        {
            // if (Player.GetPreference().Vibration)
            // {
            //     Handheld.Vibrate();
            // }
        }
        
        public void OnMaxDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }

        public static void ResourceSplit(string resource, int count, out string[] itemArray, out int[] countArray)
        {
            var leftCount = count;

            var items = new List<string>();
            var counts = new List<int>();
                        
            if  (leftCount < 10)
            {
                items.AddRange(Enumerable.Repeat(resource, leftCount));
                counts.AddRange(Enumerable.Repeat(1, leftCount));
            }
            else
            {
                while (leftCount > 0)
                {
                    var amount = Math.Min(10, leftCount);
                    items.Add(resource);
                    counts.Add(amount);
                    leftCount -= amount;
                }
            }

            itemArray = items.ToArray();
            countArray = counts.ToArray();
        }
        
        public void UpdateLeaderboardAfterGame()
        {
            const float baseAddChance = 0.06f;
            const float addChanceIntensifier = 0.02f;
            const int scoreRangeLow = 5;
            const int scoreRangeHigh = 15;
            
            UpdateLeaderboardHelper(0, null, 
                baseAddChance,
                addChanceIntensifier, 
                scoreRangeLow, 
                scoreRangeHigh);
            
        }
        
        public void UpdateLeaderboardAfterLaunch()
        {
            const float baseAddChance = 0.2f;
            const float addChanceIntensifier = 0.05f;
            const int scoreRangeLow = 10;
            const int scoreRangeHigh = 25;
            
            UpdateLeaderboardHelper(0, null, 
                baseAddChance,
                addChanceIntensifier, 
                scoreRangeLow, 
                scoreRangeHigh);
        }
        
        public void UpdateLeaderboardAfterMatch(int youScoreMod, string opponentName, int opponentScore)
        {
            const float baseAddChance = 0.1f;
            const float addChanceIntensifier = 0.05f;
            const int scoreRangeLow = 5;
            const int scoreRangeHigh = 15;

            UpdateLeaderboardHelper(youScoreMod, new HashSet<string> { opponentName }, 
                baseAddChance,
                addChanceIntensifier, 
                scoreRangeLow, 
                scoreRangeHigh);
        }

        private void UpdateLeaderboardHelper(int youScoreMod, in HashSet<string> noUpdates, 
            float baseAddChance, 
            float addChanceIntensifier, 
            int scoreRangeLow, 
            int scoreRangeHigh)
        {
            var addChance = baseAddChance;
            var names = Data.GetLeaderboardNames();
            foreach (var name in names)
            {
                if (noUpdates?.Contains(name) ?? false) continue;
                var chance = Rng.GetFloat(0f, 1f);
                if (chance > addChance)
                {
                    addChance = Mathf.Min(addChance + addChanceIntensifier, 1f);
                    continue;
                }

                addChance = baseAddChance;

                var score = Rng.GetInteger(scoreRangeLow, scoreRangeHigh + 1);
                
                Player.UpdateLeaderboard(name, score);
            }
            
            Player.UpdateLeaderboard("You", youScoreMod);
        }

        private void AttachEvents()
        {
            ReEvaluateBlockingElements();
            Ad.OnAdFadedInEvent += OnAdFadedIn;
            Ad.OnAdFadedOutEvent += OnAdFadedOut;
            Popups.HasPopupEvent += OnHasPopups;
            Popups.AllPopupsHiddenEvent += OnHasNoPopups;
        }

        private void OnAdFadedIn()
        {
            ReEvaluateBlockingElements();
        }

        private void OnAdFadedOut()
        {
            ReEvaluateBlockingElements();
        }

        private void OnHasPopups()
        {
            ReEvaluateBlockingElements();
        }

        private void OnHasNoPopups()
        {
            ReEvaluateBlockingElements();
        }

        private void ReEvaluateBlockingElements()
        {
            var oldBlocking = _hasBlockingElements;
            _hasBlockingElements = Ad.Active || Popups.HasPopups;

            if (oldBlocking == _hasBlockingElements) return;
            
            if (_hasBlockingElements)
            {
                HasBlockingElementsEvent?.Invoke();
            }
            else
            {
                NoBlockingElementsEvent?.Invoke();
            }
        }
    }
}