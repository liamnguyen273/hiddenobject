using System;
using com.brg.Common.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public struct StickerDefinition
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public Vector2 NormalizedPosition { get; set; }

        public Vector2 AbsoluteNumberingPosition { get; set; }
        public Sprite Sprite { get; set; }
    }
}