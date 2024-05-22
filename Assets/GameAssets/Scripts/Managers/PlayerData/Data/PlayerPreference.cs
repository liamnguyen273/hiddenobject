using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Internal;

namespace com.tinycastle.StickerBooker
{
    [Serializable]
    public class PlayerPreference
    {
        public int MusicVolume { get; set; }
        public int SfxVolume { get; set; }
        public bool Vibration { get; set; }

        [JsonConstructor]
        public PlayerPreference()
        {
            MusicVolume = 50;
            SfxVolume = 50;
            Vibration = true;
        }

        public PlayerPreference(PlayerPreference other) : this()
        {
            if (other != null)
            {
                MusicVolume = other.MusicVolume;
                SfxVolume = other.SfxVolume;
                Vibration = other.Vibration;
            }
        }
    }
}