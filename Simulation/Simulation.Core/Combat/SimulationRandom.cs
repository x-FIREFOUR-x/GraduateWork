using System;


namespace Simulation.Combat
{
    internal sealed class SimulationRandom
    {
        private ulong state;

        public SimulationRandom(int seed)
        {
            state = unchecked((ulong)seed);
        }

        public ulong NextUInt64()
        {
            unchecked
            {
                state += 0x9E3779B97F4A7C15UL;
                ulong value = state;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }

        public float NextFloat() => (NextUInt64() >> 40) * (1f / (1 << 24));

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Upper bound must be positive.");
            return (int)(NextUInt64() % (ulong)maxExclusive);
        }
    }
}
