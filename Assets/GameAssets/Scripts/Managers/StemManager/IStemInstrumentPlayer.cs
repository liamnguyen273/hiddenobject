using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public interface IStemInstrumentPlayer
    {
        public void Initialize();
        
        public void Play(bool immediately, params int[] instrumentIndexes);
        
        public void PlayAll();
        
        public void Mute(params int[] instrumentIndexes);
        
        public void MuteAll();
        
        public void Pause();

        public void Resume();

        public void Deinitialize();
    }
}