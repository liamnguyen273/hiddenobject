using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class WaitScreenHelper : MonoBehaviour
    {
        public void StartWait()
        {
            gameObject.SetActive(true);
        }

        public void EndWait()
        {
            gameObject.SetActive(false);
        }
    }
}