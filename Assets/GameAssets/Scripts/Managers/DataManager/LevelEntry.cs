using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public class LevelEntry
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("bundle")] public string Bundle { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; set; }
        [JsonProperty("levelName")] public string DisplayName { get; set; }
        [JsonProperty("unlockCondition")] public string UnlockCondition { get; set; }
        [JsonProperty("showUnlockCondition")] public string ShowUnlockCondition { get; set; }
        [JsonProperty("showIngame")] public bool ShowIngame { get; set; }

        [JsonIgnore] public bool Playable { get; internal set; }
        [JsonIgnore] public bool EvaluatedPlayable => Playable || IsMultiplayer;
        [JsonIgnore] public int TotalStickerCount { get; internal set; }
        [JsonIgnore] public bool IsTimeAttack => Bundle == GlobalConstants.TIME_ATTACK_LEVEL_BUNDLE || (Bundle == GlobalConstants.NORMAL_LEVEL_BUNDLE && (SortOrder % 5 == 0 || SortOrder % 6 == 0));
        [JsonIgnore] public bool IsMultiplayer => Bundle == GlobalConstants.MULTIPLAYER_LEVEL_BUNDLE;

        public string GetShowUnlockCondition()
        {
            return ShowUnlockCondition == "none" ? DisplayName : ShowUnlockCondition;
        }
        
        public int GetPlayPrice()
        {
            return (Id) switch
            {
                "easy" => 40,
                "normal" => 60,
                "hard" => 80,
                "very_hard" => 100,
                _ => 0
            };
        }        
        
        public int GetWinMultiplayerReward()
        {
            return (Id) switch
            {
                "easy" => 80,
                "normal" => 120,
                "hard" => 160,
                "very_hard" => 200,
                _ => 0
            };
        }   
        
        public int GetLoseMultiplayerReward()
        {
            return (Id) switch
            {
                "easy" => 15,
                "normal" => 20,
                "hard" => 25,
                "very_hard" => 30,
                _ => 0
            };
        }

        public float GetSpeedMod()
        {            return (Id) switch
            {
                "easy" => 1f,
                "normal" => 1.1f,
                "hard" => 1.3f,
                "very_hard" => 1.5f,
                _ => 1f
            };
        }
    }
}