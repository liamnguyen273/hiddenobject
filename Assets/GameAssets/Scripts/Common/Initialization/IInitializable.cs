namespace com.brg.Common
{
    public enum ReinitializationPolicy
    {
        NOT_ALLOWED = 0,
        ALLOW_ON_FAILED,
        ALLOW_ALL_TIME,
    }

    public enum InitializationState
    {
        NOT_INITIALIZED = 0,
        INITIALIZING,
        SUCCESSFUL,
        FAILED,
    }
    
    /// <summary>
    /// Interface for an initializable
    /// </summary>
    public interface IInitializable
    {
        public InitializationState State { get; }
        public bool Usable { get; }
        public ReinitializationPolicy ReInitPolicy { get; }

        public IProgressItem GetInitializeProgressItem();

        public void Initialize();
    }
}
