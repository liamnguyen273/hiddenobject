using System;
using com.brg.Common.Logging;
using UnityEngine;

namespace com.brg.Common
{
    public abstract class MonoManagerBase : MonoBehaviour, IInitializable
    {
        private IProgressItem _initializationProgressItem;

        public InitializationState State { get; private set; } = InitializationState.NOT_INITIALIZED;
        public bool Usable => State == InitializationState.SUCCESSFUL;
        public abstract ReinitializationPolicy ReInitPolicy { get; }

        protected virtual string LogName => GetType().Name;
        protected virtual int Priority => 1;
        protected readonly LogObj Log = new LogObj();
        
        public void Initialize()
        {
            Log.SetName(LogName);
            
            if (State == InitializationState.SUCCESSFUL && ReInitPolicy != ReinitializationPolicy.ALLOW_ALL_TIME)
            {
                Log?.Warn("Component is already has successful initialization " +
                          "(and not allowing reinitialization at all.)");
                return;
            }
            else if (State == InitializationState.FAILED && ReInitPolicy == ReinitializationPolicy.NOT_ALLOWED)
            {
                Log?.Warn("Component is already has failed initialization " +
                          "(and not allowing reinitialization.)");
                return;
            }
            else if (State == InitializationState.INITIALIZING)
            {
                Log?.Warn("Component is already initializing.");
                return;
            }
            
            Log?.Info("Initialization commenced...");
            State = InitializationState.INITIALIZING;
            StartInitializationBehaviour();
        }
        
        protected void EndInitialize(bool status)
        {
            State = status ? InitializationState.SUCCESSFUL : InitializationState.FAILED;
            EndInitializationBehaviour();
            Log?.Info(LogName, $"Initialization completed with status {status}");
        }
        
        public IProgressItem GetInitializeProgressItem()
        {
            if (_initializationProgressItem == null)
            {
                _initializationProgressItem = MakeProgressItem();
            }
            
            return _initializationProgressItem;
        }
        
        protected abstract void StartInitializationBehaviour();
        protected abstract void EndInitializationBehaviour();

        protected virtual IProgressItem MakeProgressItem()
        {
            return new SingleProgressItem(
                (out bool success) =>
                {
                    success = State == InitializationState.SUCCESSFUL;
                    return State > InitializationState.INITIALIZING;
                },
                null,
                null,
                Priority);
        }
    }
}
