namespace com.brg.Common
{
    public interface IProgressItem
    {
        /// <summary>
        /// Whether the progress is completed.
        /// </summary>
        public bool Completed { get; }
        /// <summary>
        /// Whether the progress is successful, only relevant when <see cref="Completed"/> is true.
        /// </summary>
        public bool IsSuccess { get; }
        /// <summary>
        /// A number between 0 and 1, indicating the progress.
        /// </summary>
        public float Progress { get; }
        /// <summary>
        /// Get a message for the progress.
        /// </summary>
        public string ProgressMessage { get; }
        /// <summary>
        /// The importance of the message.
        /// </summary>
        public int MessagePriority { get; }
    }
}