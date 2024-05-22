namespace com.brg.Common.Random
{
    public enum Engine
    {
        STANDARD,
    }

    public interface IRandomEngine
    {
        public void Reset();
        public int GetInteger();
        public int GetInteger(int maxEclusive);
        public int GetInteger(int minInclusive, int maxExclusive);
        public float GetFloat();
        public float GetFloat(float max);
        public float GetFloat(float min, float max);
    }

    public static class RandomEngineFactory
    {
        public static IRandomEngine CreateEngine(Engine engine, int seed)
        {
            return engine switch
            { 
                Engine.STANDARD => new StandardRandomEngine(seed),
                _ => null
            };
        }
    }
}
