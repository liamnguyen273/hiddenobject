
using System;
using System.Collections.Generic;

namespace com.brg.Common.Logging
{
    public class LogObj
    {
        public static readonly LogObj Default = new LogObj();
        
        private string _name;
        
        public LogObj()
        {
            _name = null;
        }        
        
        public void SetName(string name)
        {
            _name = name;
        }
        
        public LogObj(string name)
        {
            _name = name;
        }
        
        public void Info(object message)
        {
            CLog.Log(GetLog("INFO", string.Empty, message));
        }

        public void Info(string extraName, object message)
        {
            CLog.Log(GetLog("INFO", extraName, message));
        }

        public void Warn(object message)
        {
            CLog.Warn(GetLog("WARN", string.Empty, message));
        }
        public void Warn(string extraName, object message)
        {
            CLog.Warn(GetLog("WARN", extraName, message));
        }

        public void Error(object message)
        {
            CLog.Error(GetLog("ERROR", string.Empty, message));
        }
        
        public void Error(string extraName, object message)
        {
            CLog.Error(GetLog("ERROR", extraName, message));
        }

        private string GetLog(string pad, string extraName, object message)
        {
            return $"(GL)[{pad}]{FormatTime(DateTime.Now)} " +
                   $"{(_name != null ? $"[{_name}] " : "")}" +
                   $"{(extraName != string.Empty ? $"[{extraName}] " : "")}" +
                   $"{message}";
        }

        private string FormatTime(DateTime time)
        {
#if UNITY_EDITOR
            return "";
#else
            return "[" + time.ToString("yyyy/MM/dd HH:mm:ss") + "]";
#endif
        }
    }
}
