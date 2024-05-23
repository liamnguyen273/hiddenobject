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

        public int LayerCount => layers.Count;
        public int StickerToFind => layers.Count - 1; //Number of stickers that need to be found
    }
}