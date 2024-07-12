using com.brg.Common;
using System.Collections.Generic;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class LevelSelector : MonoManagerBase
    {
        [Header("Components")] 
        [SerializeField] private Transform _itemHost;
        [SerializeField] protected string _bundleName = "default";

        private List<LevelSelectorItem> _orderedItems;
        
        public void OnSettingButton()
        {
            GM.Instance.Popups.GetPopup<PopupBehaviourSettings>().Show();
        }

        public void SaveLastPlayedLevel(string id)
        {
            // TODO   
        }

        protected virtual void RefreshLevelList(List<LevelEntry> sortedEntries)
        {
            var count = sortedEntries.Count;

            if (count > _orderedItems.Count)
            {
                Log.Error($"Please add more selector items. Missing: {count - _orderedItems.Count} items.");
            }

            for (int i = 0; i < _orderedItems.Count; ++i)
            {
                var entry = i >= sortedEntries.Count ? null : sortedEntries[i];
                var item = _orderedItems[i];
                
                if (entry != null)
                {
                    // Get progress too
                    GM.Instance.Player.GetLevelState(entry.Id, out var completion, out var hasProgress, out var progress);
                    item.ActivateWith(entry, completion.Completed, hasProgress, progress);
                }
                else
                {
                    item.Deactivate();
                }
            }
        }
    }
}