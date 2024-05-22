using System;
using UnityEngine;

namespace com.brg.Common.Random
{
    internal class StandardRandomEngine : IRandomEngine
    {
        private int _seed;
        private System.Random _random;

        public StandardRandomEngine(int seed)
        {
            _seed = seed;
            Reset();
        }

        public float GetFloat()
        {
            return (float)_random.NextDouble();
        }

        public float GetFloat(float max)
        {
            return (float)GetFloat() * max;
        }

        public float GetFloat(float min, float max)
        {
            return (float)GetFloat() * (max - min) + min;
        }

        public int GetInteger()
        {
            return _random.Next();
        }

        public int GetInteger(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }

        public int GetInteger(int minInclusive, int maxExclusive)
        {
            var value = _random.Next(minInclusive, maxExclusive);
            return value;
        }

        public void Reset()
        {
            _random = new System.Random(_seed);
        }
    }
}
