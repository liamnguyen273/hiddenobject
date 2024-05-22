using UnityEngine;

namespace com.brg.Common
{
    public static class CLog
    {
        public static bool ShouldLog = true;

        public static void Log(object log)
        {
            if (!ShouldLog) return;
            Debug.Log(log);
        }

        public static void Log(object log, Object context)
        {
            if (!ShouldLog) return;
            Debug.Log(log, context);
        }

        public static void Warn(object log)
        {
            if (!ShouldLog) return;
            Debug.LogWarning(log);
        }

        public static void Warn(object log, Object context)
        {
            if (!ShouldLog) return;
            Debug.LogWarning(log, context);
        }

        public static void Error(object log)
        {
            if (!ShouldLog) return;
            Debug.LogError(log);
        }

        public static void Error(object log, Object context)
        {
            if (!ShouldLog) return;
            Debug.LogError(log, context);
        }
    }
}
