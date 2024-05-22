using com.brg.Common;
using com.brg.Common.Localization;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public partial class LevelSelector : IActivatable
    {
        public void Activate()
        {
            // Uhm
        }
        
        public void Deactivate()
        {
            // Uhm
        }

        public IProgressItem GetPrepareActivateProgressItem()
        {
            return new ImmediateProgressItem();
        }

        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            return new ImmediateProgressItem();
        }
        
        public virtual void PrepareActivate()
        {
            // TODO: Bundle switch
            var list = GM.Instance.Data.GetSortedLevelEntries(_bundleName ?? GlobalConstants.NORMAL_LEVEL_BUNDLE);
            
            // Filter
            list = list.Where(x => x.ShowIngame)
                .OrderBy(x => x.SortOrder)
                .ToList();
            
            RefreshLevelList(list);    // This is immediately in the frame
        }

        public void PrepareDeactivate()
        {
            // Do nothing
        }
    }
}