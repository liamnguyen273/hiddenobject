using System;

namespace com.brg.Common
{
    public delegate float ProgressUpdaterDelegate();
    public delegate bool CompletionUpdaterDelegate(out bool success);
    public delegate string MessageUpdaterDelegate();
    
    public class SingleProgressItem : IProgressItem
    {
        private readonly ProgressUpdaterDelegate _progressUpdater;
        private readonly CompletionUpdaterDelegate _completionUpdater;
        private readonly MessageUpdaterDelegate _messageUpdater;
        
        public bool Completed => _completionUpdater(out _);

        public bool IsSuccess
        {
            get
            {
                _completionUpdater(out var res);
                return res;
            }
        }

        public float Progress => _progressUpdater();
        public string ProgressMessage => _messageUpdater();
        public int MessagePriority { get; set; }
        
        public SingleProgressItem(
            CompletionUpdaterDelegate completionUpdater,
            ProgressUpdaterDelegate progressUpdater, 
            MessageUpdaterDelegate messageUpdater,
            int priority)
        {
            _completionUpdater = completionUpdater ?? throw new Exception("ProgressItem's completionUpdater is required, cannot receive null.");
            _progressUpdater = progressUpdater ?? DefaultProgressUpdater;
            _messageUpdater = messageUpdater ?? DefaultMessageUpdate;
            MessagePriority = priority;
        }
        
        private float DefaultProgressUpdater()
        {
            return _completionUpdater(out _) ? 1f : 0f;
        }

        private string DefaultMessageUpdate()
        {
            var done = _completionUpdater(out var success);
            return done ? (success ? "Success" : "Failed") : "Progressing...";
        }
    }
}
