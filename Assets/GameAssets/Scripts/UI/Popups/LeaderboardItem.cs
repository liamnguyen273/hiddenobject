using com.brg.Common.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class LeaderboardItem : MonoBehaviour
    {
        [SerializeField] private GameObject[] _normalItems;
        [SerializeField] private GameObject[] _playerItems;
        
        [Header("Fields")]
        [SerializeField] private TextLocalizer _rankText;
        [SerializeField] private TextLocalizer _nameText;
        [SerializeField] private TextLocalizer _scoreText;
        [SerializeField] private Image _avatar;
        [SerializeField] private GameObject[] _stars;
        
        public void SetInfo(int rank, string name, int score, bool isPlayer)
        {
            _rankText.RawString = rank.ToString();
            _nameText.RawString = name;
            _scoreText.RawString = score.ToString();
            
            foreach (var item in _normalItems)
            {
                item.SetActive(!isPlayer);
            }     
            
            foreach (var item in _playerItems)
            {
                item.SetActive(isPlayer);
            }

            _avatar.sprite = GM.Instance.Data.GetAvatar(name);

            for (int i = 1; i <= 3; ++i)
            {
                _stars[i - 1].SetActive(i == rank);
            }
        }
    }
}