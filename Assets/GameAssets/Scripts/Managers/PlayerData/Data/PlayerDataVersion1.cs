using System;
using System.Collections.Generic;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public class PlayerDataVersion1
    {
        public Dictionary<string, bool> CompletedLevels { get; set; }
        public Dictionary<string, int[]> InProgressLevels { get; set; }
        public bool AdFree { get; set; }
        public bool TutorialPlayed { get; set; }
        public int SearchHintCount { get; set; }

        public DateTime LastSaveTime { get; set; }

        public PlayerDataVersion1()
        {
            CompletedLevels = new Dictionary<string, bool>();
            InProgressLevels = new Dictionary<string, int[]>();
            SearchHintCount = 0;
            AdFree = false;
            TutorialPlayed = false;

            LastSaveTime = DateTime.UtcNow;
        }
    }
}