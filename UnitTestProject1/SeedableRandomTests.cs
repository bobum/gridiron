using System;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class SeedableRandomTests
    {
        [TestMethod]
        public void SameSeedProducesSameSequence()
        {
            int seed = 42;
            var rng1 = new SeedableRandom(seed);
            var rng2 = new SeedableRandom(seed);

            // Compare a sequence of NextDouble
            for (int i = 0; i < 10; i++)
            {
                double v1 = rng1.NextDouble();
                double v2 = rng2.NextDouble();
                Assert.AreEqual(v1, v2, $"Mismatch at position {i}: {v1} vs {v2}");
            }

            // Compare a sequence of Next(int, int)
            for (int i = 0; i < 10; i++)
            {
                int vi1 = rng1.Next(0, 1000);
                int vi2 = rng2.Next(0, 1000);
                Assert.AreEqual(vi1, vi2, $"Int mismatch at position {i}: {vi1} vs {vi2}");
            }
        }

        [TestMethod]
        public void DifferentSeedsProduceDifferentSequence()
        {
            var rng1 = new SeedableRandom(1);
            var rng2 = new SeedableRandom(2);

            int different = 0;
            for (int i = 0; i < 10; i++)
            {
                if (rng1.NextDouble() != rng2.NextDouble())
                    different++;
            }
            Assert.IsGreaterThan(0, different, $"Expected at least one difference, found {different} different values.");
        }

        [TestMethod]
        public void GuidSeedProducesSameSequenceForSameGuid()
        {
            var guid = Guid.NewGuid();
            var rng1 = new SeedableRandom(guid);
            var rng2 = new SeedableRandom(guid);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(rng1.Next(), rng2.Next(), $"Mismatch at position {i}");
            }
        }

        [TestMethod]
        public void GetBytesAndGetNonZeroBytesWork()
        {
            var rng = new SeedableRandom(123);
            byte[] buffer = new byte[20];
            rng.GetBytes(buffer);

            // Should fill buffer with bytes between 0 and 255
            foreach (var b in buffer)
            {
                Assert.IsTrue(b >= 0 && b <= 255);
            }

            byte[] nonZero = new byte[20];
            rng.GetNonZeroBytes(nonZero);

            // Should fill buffer with bytes between 1 and 255 (no zeros)
            foreach (var b in nonZero)
            {
                Assert.IsTrue(b > 0 && b <= 255);
            }
        }
    }
}