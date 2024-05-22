using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public struct LevelProgress
    {
        public int CurrentTime { get; set; }
        public int[] AttachedStickers { get; set; }
    }
    
    [Serializable]
    public class PlayerProgressData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, LevelProgress> InProgressLevels { get; set; }
        
        public DateTime LastSaveTime { get; set; }

        [JsonConstructor]
        public PlayerProgressData()
        {
            InProgressLevels = new Dictionary<string, LevelProgress>();
            LastSaveTime = DateTime.UtcNow;
        }
    }
}