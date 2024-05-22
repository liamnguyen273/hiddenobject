namespace com.brg.Common
{
    public class ImmediateProgressItem : IProgressItem
    {
        public bool Completed => true;
        public bool IsSuccess => true;
        public float Progress => 1;
        public string ProgressMessage => "Completed";
        public int MessagePriority => 1000;
    }
}