using com.brg.Common;
using com.brg.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class LevelSelectorMultiplayer : LevelSelector
    {
        [SerializeField] private LeaderboardItem _playerLeaderboardItem;
        
        public override void PrepareActivate()
        {
            base.PrepareActivate();

            GM.Instance.Popups.GetPopup<PopupBehaviourLeaderboard>(out var behaviour);
            behaviour.RefreshLeaderboard();
            var rank = behaviour.GetYouRank();
            var score = 0;
            GM.Instance.Player.GetLeaderboard().TryGetValue("You", out score);
            _playerLeaderboardItem.SetInfo(rank, "You", score, true);
        }

        public void OnLeaderboardButton()
        {
            var popup = GM.Instance.Popups.GetPopup<PopupBehaviourLeaderboard>(out var behaviour);
            popup.Show();
        }
    }
}