using System.Collections.Generic;
using com.brg.Common;

namespace com.brg.Common.AnalyticEvents
{
    public interface IAnalyticsServiceAdapter: IInitializable, IInitializableOberservable
    {
        public void SendEvent(AnalyticsEventBuilder eventBuilder);
        public bool TranslateGameEventName(string name, out string translatedName);
    }
}