using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    [CreateAssetMenu]
    public class OfflinePlayingStemData : ScriptableObject, IPlayingStemData
    {
        [SerializeField] private int _packNumber;
        [SerializeField] private AudioClip[] _clips;

        public int PackNumber => _packNumber;
        public int ClipCount => _clips.Length;
        public float DownloadProgress => 1f;

        public AudioClip GetClip(int i)
        {
            return i > 0 && i < _clips.Length ? _clips[i] : null;
        }

        public IEnumerable<AudioClip> GetAllClips()
        {
            return _clips;
        }
    }
}