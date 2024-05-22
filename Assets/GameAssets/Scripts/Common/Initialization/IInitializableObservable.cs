using System;

namespace com.brg.Common
{
    public delegate InitializationState InitilizationDelegate();
    
    /// <summary>
    /// Interface for an initializable that exposes events for initializations
    /// </summary>
    public interface IInitializableOberservable
    {
        public event Action OnInitializationSuccessfulEvent;
        public event Action OnInitializationFailedEvent;
    }
}
