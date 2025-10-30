using System;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class ReplaySeedableRandomTests
    {
        [TestMethod]
        public void LogsAndReplaysRandomSequence()
        {
            int seed = 1234;
            var rng = new ReplaySeedableRandom(seed);

            double d1 = rng.NextDouble();
            int i1 = rng.Next(10, 20);
            int i2 = rng.Next();
            int i3 = rng.Next(100);

            var log = rng.GetReplayLog();

            // Print log for debugging
            Console.WriteLine($"Logged Doubles: {string.Join(", ", log.Doubles)}");
            foreach (var entry in log.IntRanges)
                Console.WriteLine($"Logged IntRange: min={entry.Min} max={entry.Max} value={entry.Value}");
            Console.WriteLine($"Logged Ints: {string.Join(", ", log.Ints)}");

            log.Save("test_replaylog.json");

            // Replay using the saved log
            var loadedLog = ReplayLog.Load("test_replaylog.json");
            var replayRng = new ReplaySeedableRandom(seed, loadedLog);

            Assert.AreEqual(d1, replayRng.NextDouble());
            Assert.AreEqual(i1, replayRng.Next(10, 20));
            Assert.AreEqual(i2, replayRng.Next());
            Assert.AreEqual(i3, replayRng.Next(100));
        }
    }
}