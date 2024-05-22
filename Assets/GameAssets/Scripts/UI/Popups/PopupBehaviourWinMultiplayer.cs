using com.brg.Common.Localization;
using com.brg.Common.Logging;
using com.brg.Common.UI;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourWinMultiplayer : UIPopupBehaviour
    {
        [SerializeField] private TextLocalizer _titleText;
        [SerializeField] private Button _doubleRewardButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Animator _fireworkLeft;
        [SerializeField] private Animator _fireworkRight;

        [Header("Win")] 
        [SerializeField] private Image _winAvatar; 
        [SerializeField] private TextLocalizer _winName; 
        [SerializeField] private TextLocalizer _winScore;      
        
        [Header("Lose")] 
        [SerializeField] private Image _loseAvatar; 
        [SerializeField] private TextLocalizer _loseName; 
        [SerializeField] private TextLocalizer _loseScore; 

        [Header("Others")] 
        [SerializeField] private TextLocalizer _rewardText;

        private LevelEntry _entry;
        private string _opponentName;
        private int _youScore;
        private int _opponentScore;
        private bool _isWin;
        private int _reward;

        public void SetInfo(LevelEntry entry, string opponentName, int youScore, int opponentScore, bool isWin)
        {
            _entry = entry;
            _opponentName = opponentName;
            _youScore = youScore;
            _opponentScore = opponentScore;
            _isWin = isWin;
            _reward = _isWin ? _entry.GetWinMultiplayerReward() : _entry.GetLoseMultiplayerReward();
        }
        
        protected override void InnateOnShowStart()
        {
            Refresh();
            base.InnateOnShowStart();
        }

        protected override void InnateOnShowEnd()
        {
            if (_isWin)
            {
                _fireworkLeft.Play("fire");
                _fireworkRight.Play("fire");
            }

            AudioManager.StopSound(LibrarySounds.Congrats, stopInstantly: true);
            
            GiveRewardsWithAnimation();
            
            base.InnateOnShowEnd();
        }

        protected override void InnateOnHideEnd()
        {
            _fireworkLeft.Play("idle");
            _fireworkRight.Play("idle");
            
            base.InnateOnHideEnd();
        }

        public void OnReturnButton()
        {
            GM.Instance.RequestGoToMenu();
            Popup.Hide();
        }

        public void OnDoubleRewardButton()
        {
            var request = new AdRequest(
                AdManager.TYPE_REWARD,
                () =>
                {
                    GiveRewardsWithAnimation();
                    _doubleRewardButton.interactable = false;
                },
                null);
            GM.Instance.Ad.RequestAd(request);
        }

        private void GiveRewardsWithAnimation()
        {
            if (_entry == null)
            {
                LogObj.Default.Warn("Level entry is null, cannot give rewards");
            }
            
            GM.ResourceSplit(GlobalConstants.STAMP_RESOURCE, _reward, out var items, out var counts);
            GM.Instance.ResolveAnimateAddItems(items, counts, false);
        }
        
        private void Refresh()
        {
            _doubleRewardButton.interactable = true;

            var winAvatar = GM.Instance.Data.GetAvatar(_isWin ? "You" : _opponentName);
            var loseAvatar = GM.Instance.Data.GetAvatar(_isWin ? _opponentName : "You");

            _rewardText.RawString = _reward.ToString();

            _winAvatar.sprite = winAvatar;
            _loseAvatar.sprite = loseAvatar;
            _winName.RawString = _isWin ? "You" : _opponentName;
            _loseName.RawString = _isWin ? _opponentName : "You";
            _winScore.RawString = (_isWin ? _youScore : _opponentScore).ToString();
            _loseScore.RawString = (_isWin ? _opponentScore : _youScore).ToString();

            _titleText.RawString = _isWin ? "Congratulations!" : "Better Luck Next Time...";
        }
    }
}