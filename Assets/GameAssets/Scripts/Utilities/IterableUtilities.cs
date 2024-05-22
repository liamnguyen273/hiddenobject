using com.brg.Common.Random;
using System;
using System.Collections.Generic;

namespace com.brg.Utilities
{
    public static class IterableUtilities
    {
        public static void Shuffle<T>(this IList<T> list, IRandomEngine engine)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = engine.GetInteger(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
