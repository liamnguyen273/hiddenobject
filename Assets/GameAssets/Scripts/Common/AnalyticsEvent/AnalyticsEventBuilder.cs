using System;
using System.Collections.Generic;
using System.Text;

namespace com.brg.Common.AnalyticEvents
{
    public class AnalyticsEventBuilder
    {
        private string _name;
        private AnalyticsEventManager _manager;
        private readonly List<(string name, Type type, object value)> _list;

        public string Name => _name;
        public List<(string name, Type type, object value)> Parameters => _list;
        
        internal AnalyticsEventBuilder(string eventName, AnalyticsEventManager manager)
        {
            _name = eventName;
            _manager = manager;
            _list = new List<(string, Type, object)>();
        }

        internal IEnumerable<(string, Type, object)> IterateParameters()
        {
            foreach (var parameter in _list)
            {
                yield return parameter;
            }
        }

        public AnalyticsEventBuilder Add((string, Type, object) packedParam)
        {
            _list.Add(packedParam);
            return this;
        }

        public AnalyticsEventBuilder Add((string, Type, object)[] packedParams)
        {
            if (packedParams == null) return this;
            
            foreach (var param in packedParams)
            {
                _list.Add(param);
            }

            return this;
        }

        public AnalyticsEventBuilder Add(string parameterName, string value)
        {
            _list.Add((parameterName, typeof(string), value));
            return this;
        }
        
        public AnalyticsEventBuilder Add(string parameterName, int value)
        {
            _list.Add((parameterName, typeof(int), value));
            return this;
        }
        
        public AnalyticsEventBuilder Add(string parameterName, float value)
        {
            _list.Add((parameterName, typeof(float), value));
            return this;
        }        
        
        public AnalyticsEventBuilder Add(string parameterName, double value)
        {
            _list.Add((parameterName, typeof(double), value));
            return this;
        }
        
        public AnalyticsEventBuilder Add(string parameterName, bool value)
        {
            _list.Add((parameterName, typeof(bool), value));
            return this;
        }

        public void SendEvent()
        {
            _manager.SendEvent(this);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Name: \"{Name}\". Params: ");

            var i = 1;
            foreach (var param in _list)
            {
                builder.AppendLine($"{i:00}: \"{param.name}\" = {param.value} (type: \"{param.type.Name}\")");
            }

            return builder.ToString();
        }
    }
}