using System;
using System.Collections.Generic;
using System.Linq;

namespace com.brg.Common
{
    public class ProgressItemGroup : IProgressItem
    {
        private int _highestPriority;
        private IProgressItem[] _items;
        
        public bool Completed => CompletionUpdater(out _);
        public bool IsSuccess  
        {
            get
            {
                CompletionUpdater(out var success);
                return success;
            }
        }
        public float Progress => ProgressUpdater();
        public string ProgressMessage => MessageUpdater();
        public int MessagePriority { get; set; }

        public ProgressItemGroup(params IProgressItem[] progressItems)
        {
            _items = progressItems;
            _highestPriority = progressItems.Min(p => p.MessagePriority);
        }

        public ProgressItemGroup(IEnumerable<IProgressItem> items)
        {
            _items = items.ToArray();
            _highestPriority = _items.Min(p => p.MessagePriority);
        }

        private bool CompletionUpdater(out bool success)
        {
            success = true;
            var completed = true;
            if (_items == null) return true;
            
            foreach (var item in _items)
            {
                success &= item.IsSuccess;
                completed &= item.Completed;
            }
            
            return completed;
        }

        private float ProgressUpdater()
        {
            if (_items == null) return 1f;

            var sum = _items.Sum(item => item.Progress);

            return sum / _items.Length;
        }

        private string MessageUpdater()
        {
            var done = CompletionUpdater(out var success);
            return done ? (success ? "Success" : "Failed") : "Progressing...";
        }
    }
}
