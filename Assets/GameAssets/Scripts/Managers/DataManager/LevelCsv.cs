using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class LevelCsv
    {
        public string Level { get; set; }
        public Dictionary<string, int> StickerList { get; set; }
        public Dictionary<string, Vector2> StickerPositions { get; set; }
        public Dictionary<string, Vector2> NumberPositions { get; set; }
    }
}