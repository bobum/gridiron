using System;

namespace DomainObjects.Helpers
{
    // Full interface matching the legacy ICryptoRandom
    public interface ISeedableRandom : IDisposable
    {
        void GetBytes(byte[] buffer);
        double NextDouble();
        int Next(int minValue, int maxValue);
        int Next();
        int Next(int maxValue);
        void GetNonZeroBytes(byte[] data);
    }

    // Seedable random implementation for deterministic and testable simulation
    public class SeedableRandom : ISeedableRandom
    {
        private readonly Random _internal;

        public SeedableRandom(int seed) => _internal = new Random(seed);
        public SeedableRandom(Guid guidSeed) => _internal = new Random(guidSeed.GetHashCode());
        public SeedableRandom() => _internal = new Random();

        public double NextDouble() => _internal.NextDouble();

        public int Next(int minValue, int maxValue) => _internal.Next(minValue, maxValue);

        public int Next() => _internal.Next();

        public int Next(int maxValue) => _internal.Next(maxValue);

        public void GetBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)_internal.Next(0, 256);
        }

        public void GetNonZeroBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // Generate random non-zero byte
                byte b = 0;
                while (b == 0)
                    b = (byte)_internal.Next(1, 256);
                data[i] = b;
            }
        }

        public void Dispose() { /* nothing to dispose */ }
    }
}