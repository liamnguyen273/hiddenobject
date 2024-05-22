using com.brg.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Logging;
using com.brg.Utilities;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourLeaderboard : UIPopupBehaviour
    {
        [SerializeField] private Transform _itemHost;
        [SerializeField] private LeaderboardItem _playerLeaderboardItem;
        
        private List<string> _sortedNames = null;
        private int _youRank = -1;
        private List<LeaderboardItem> _items = null;

        internal override void Initialize()
        {
            base.Initialize();
            _items = _itemHost.GetDirectOrderedChildComponents<LeaderboardItem>().ToList();
        }

        protected override void InnateOnShowStart()
        {
            RefreshLeaderboard();
            RefreshAppearance();
            base.InnateOnShowStart();
        }

        public void RefreshLeaderboard()
        {
            var leaderboard = GM.Instance.Player.GetLeaderboard()
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key);

            _sortedNames = leaderboard.ToList();
            _youRank = _sortedNames.FindIndex(x => x == "You") + 1;
        }

        public int GetYouRank()
        {
            if (_youRank <= 0)
            {
                LogObj.Default.Warn("Leaderboard popup should be refreshed before calling GetYouRank(). Will now" +
                                    "return wrong value.");
            }
            return _youRank;
        }

        private void RefreshAppearance()
        {
            var leaderboard = GM.Instance.Player.GetLeaderboard();
            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];
                if (_sortedNames.Count <= i)
                {
                    item.SetGOActive(false);
                    continue;
                }
                
                item.SetGOActive(true);
                
                var name = _sortedNames[i];
                leaderboard.TryGetValue(name, out var score);
                item.SetInfo(i + 1, name, score, name == "You");
            }
            
            var rank = GetYouRank();
            GM.Instance.Player.GetLeaderboard().TryGetValue("You", out var youScore);
            _playerLeaderboardItem.SetInfo(rank, "You", youScore, true);
        }
    }
}