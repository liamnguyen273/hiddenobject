using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.tinycastle.StickerBooker
{
    [CreateAssetMenu]
    public class StemData : ScriptableObject
    {
        public string id;
        public List<AudioClip> layers;
        public string levelResolution;

        public int LayerCount => layers.Count;
        public int StickerToFind => layers.Count - 1; //Number of stickers that need to be found

        public bool ResolvePlayForLevel(LevelEntry entry)
        {
            switch (levelResolution)
            {
                case "early":
                    return entry.SortOrder < 15;
                case "late":
                    return entry.SortOrder >= 15;
                default:
                    return false;
            }
        }
    }
}