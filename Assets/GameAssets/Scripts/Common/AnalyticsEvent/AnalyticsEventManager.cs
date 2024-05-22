using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.Common.Logging;

namespace com.brg.Common.AnalyticEvents
{
    public class AnalyticsEventManager: ManagerBase
    {
        private readonly List<IAnalyticsServiceAdapter> _adapters;

        private int _total = 0;
        private int _done = 0;

        public AnalyticsEventManager(params IAnalyticsServiceAdapter[] adapters) : base()
        {
            _adapters = new List<IAnalyticsServiceAdapter>();
            _adapters.AddRange(adapters);
        }

        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;

        protected override void StartInitializationBehaviour()
        {
            foreach (var adapter in _adapters)
            {
                adapter.OnInitializationSuccessfulEvent += OnAdapterOnInitializationSuccessful;
            }

            _total = _adapters.Count;
            _done = 0;

            foreach (var adapter in _adapters)
            {
                adapter.Initialize();
            }
        }

        protected override void EndInitializationBehaviour()
        {
            // Do nothing
        }

        protected override IProgressItem MakeProgressItem()
        {
            var progresses =
                _adapters.Select(x => x.GetInitializeProgressItem())
                    .Append(new SingleProgressItem((out bool success) =>
                    {
                        success = true;
                        return _done >= _total;
                    }, null, null, 100))
                    .ToArray();
            return new ProgressItemGroup(progresses);
        }

        public AnalyticsEventBuilder MakeEvent(string name)
        {
            return new AnalyticsEventBuilder(name, this);
        }

        public void SendEvent(AnalyticsEventBuilder eventBuilder)
        {
            foreach (var adapter in _adapters)
            {
                adapter.SendEvent(eventBuilder);
            }
        }

        private void OnAdapterOnInitializationSuccessful()
        {
            ++_done;

            if (_done >= _total)
            {
                EndInitialize(GetInitializeProgressItem().IsSuccess);
            }
        }
    }
}