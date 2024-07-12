using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public interface IPlayingStemData
    {
        public int PackNumber { get; }
        public int ClipCount { get; }
        
        public float DownloadProgress { get; }

        public AudioClip GetClip(int i);

        public IEnumerable<AudioClip> GetAllClips();
    }
}