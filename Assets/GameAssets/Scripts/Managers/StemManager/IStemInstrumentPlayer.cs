using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public interface IStemInstrumentPlayer
    {
        // Khởi tạo (nếu cần, không cần để body hàm trống.
        public void Initialize();

        // Bật tiếng (các) nhạc cụ số index.
        public void Play(params int[] instrumentIndexes);

        // Chơi tất cả nhạc cụ
        public void PlayAll();

        // Tắt tiếng nhạc cụ số index.
        public void Mute(params int[] instrumentIndexes);

        // Tắt tất cả tiếng.
        public void MuteAll();

        // Hành vi khi pause
        public void Pause();

        // Hành vi khi resume (sau khi pause)
        public void Resume();

        // Dọn sau khi chơi xong (tắt toàn bộ tiếng, dọn resource, etc.)
        public void Deinitialize();
    }
}