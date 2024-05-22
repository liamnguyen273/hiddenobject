using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Internal;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public class CompletedLevelData
    {
        public CompletedLevelData()
        {
            Completed = false;
            BestTime = 0;
        }

        public CompletedLevelData(bool completed, int bestTime)
        {
            Completed = completed;
            BestTime = bestTime;
        }

        public bool Completed { get; set; }
        public int BestTime { get; set; }
    }
    
    [Serializable]
    public class PlayerData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CompletedLevelData> CompletedLevels { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HashSet<string> Ownerships { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Resources { get; set; }        
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Leaderboard { get; set; }
        
        public bool TutorialPlayed { get; set; }
        public bool TimeAttackTutorialPlayed { get; set; } 
        public bool MultiplayerTutorialPlayed { get; set; }
        public DateTime LastSaveTime { get; set; }

        [JsonConstructor]
        public PlayerData()
        {
            CompletedLevels = new Dictionary<string, CompletedLevelData>();
            Ownerships = new HashSet<string>();
            Leaderboard = new Dictionary<string, int>();
            Resources = new Dictionary<string, int>()
            {
                { GlobalConstants.STAMP_RESOURCE, 0 },
                { GlobalConstants.CHEST_PROGRESS_RESOURCE, 0 },
                { GlobalConstants.POWER_COMPASS, 0 },
                { GlobalConstants.POWER_LOOKUP, 0 },
            };
            
            TutorialPlayed = false;
            LastSaveTime = DateTime.UtcNow;
        }
    }
}