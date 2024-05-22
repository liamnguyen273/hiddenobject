using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.brg.Utilities
{
    public static class AssetUtilities
    {
        public static bool Exists(string path)
        {
#if UNITY_EDITOR
            var guid = AssetDatabase.AssetPathToGUID(path);
            return string.IsNullOrEmpty(guid);
#else
            return false;
#endif
        }
    }
}