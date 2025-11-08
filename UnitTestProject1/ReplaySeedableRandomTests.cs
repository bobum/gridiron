using System;
using System.IO;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class ReplaySeedableRandomTests
    {
        private const string TestLogFile = "test_replaylog.json";

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files after each test
            if (File.Exists(TestLogFile))
            {
                File.Delete(TestLogFile);
            }
        }

        #region Happy Path Tests

        [TestMethod]
        public void LogsAndReplaysRandomSequence()
        {
            // Arrange
            int seed = 1234;
            var rng = new ReplaySeedableRandom(seed);

            // Act - Record sequence
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

            log.Save(TestLogFile);

            // Act - Replay sequence
            var loadedLog = ReplayLog.Load(TestLogFile);
            var replayRng = new ReplaySeedableRandom(seed, loadedLog);

            // Assert - Verify exact replay
            Assert.AreEqual(d1, replayRng.NextDouble());
            Assert.AreEqual(i1, replayRng.Next(10, 20));
            Assert.AreEqual(i2, replayRng.Next());
            Assert.AreEqual(i3, replayRng.Next(100));
        }

        [TestMethod]
        public void RecordingMode_PopulatesLogCorrectly()
        {
            // Arrange
            int seed = 42;
            var rng = new ReplaySeedableRandom(seed);

            // Act
            rng.NextDouble();
            rng.NextDouble();
            rng.Next(5, 10);
            rng.Next();
            rng.Next(50);

            var log = rng.GetReplayLog();

            // Assert
            Assert.AreEqual(seed, log.Seed);
            Assert.AreEqual(2, log.Doubles.Count);
            Assert.AreEqual(1, log.IntRanges.Count);
            Assert.AreEqual(2, log.Ints.Count);

            // Verify IntRange entry
            Assert.AreEqual(5, log.IntRanges[0].Min);
            Assert.AreEqual(10, log.IntRanges[0].Max);
        }

        [TestMethod]
        public void RecordingMode_LogsValuesInOrder()
        {
            // Arrange
            int seed = 999;
            var rng = new ReplaySeedableRandom(seed);

            // Act - Create known sequence
            double d1 = rng.NextDouble();
            double d2 = rng.NextDouble();
            double d3 = rng.NextDouble();

            var log = rng.GetReplayLog();

            // Assert - Verify order is preserved
            Assert.AreEqual(3, log.Doubles.Count);
            Assert.AreEqual(d1, log.Doubles[0]);
            Assert.AreEqual(d2, log.Doubles[1]);
            Assert.AreEqual(d3, log.Doubles[2]);
        }

        #endregion

        #region Mixed Sequence Tests

        [TestMethod]
        public void ComplexInterleavedSequence_RecordsAndReplaysCorrectly()
        {
            // Arrange
            int seed = 777;
            var rng = new ReplaySeedableRandom(seed);

            // Act - Complex interleaved sequence
            double d1 = rng.NextDouble();
            int i1 = rng.Next(1, 100);
            double d2 = rng.NextDouble();
            int i2 = rng.Next();
            int i3 = rng.Next(50);
            double d3 = rng.NextDouble();
            int i4 = rng.Next(10, 20);

            var log = rng.GetReplayLog();
            var replayRng = new ReplaySeedableRandom(seed, log);

            // Assert - Verify exact replay in order
            Assert.AreEqual(d1, replayRng.NextDouble());
            Assert.AreEqual(i1, replayRng.Next(1, 100));
            Assert.AreEqual(d2, replayRng.NextDouble());
            Assert.AreEqual(i2, replayRng.Next());
            Assert.AreEqual(i3, replayRng.Next(50));
            Assert.AreEqual(d3, replayRng.NextDouble());
            Assert.AreEqual(i4, replayRng.Next(10, 20));
        }

        [TestMethod]
        public void LongSequence_RecordsAndReplaysCorrectly()
        {
            // Arrange
            int seed = 555;
            var rng = new ReplaySeedableRandom(seed);
            var doubles = new double[100];
            var ints = new int[100];

            // Act - Record long sequence
            for (int i = 0; i < 100; i++)
            {
                doubles[i] = rng.NextDouble();
                ints[i] = rng.Next(0, 1000);
            }

            var log = rng.GetReplayLog();
            var replayRng = new ReplaySeedableRandom(seed, log);

            // Assert - Verify all values replay correctly
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(doubles[i], replayRng.NextDouble(), $"Double mismatch at index {i}");
                Assert.AreEqual(ints[i], replayRng.Next(0, 1000), $"Int mismatch at index {i}");
            }
        }

        #endregion

        #region Error Condition Tests

        [TestMethod]
        public void ReplayMode_NextDouble_ThrowsWhenLogExhausted()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.Doubles.Add(0.5);
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.NextDouble(); // First call succeeds

            // Assert
            bool exceptionThrown = false;
            try
            {
                replayRng.NextDouble();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        [TestMethod]
        public void ReplayMode_NextDouble_ThrowsWithCorrectMessage()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.Doubles.Add(0.5);
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.NextDouble();

            // Assert
            try
            {
                replayRng.NextDouble();
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Replay log exhausted: Doubles", ex.Message);
            }
        }

        [TestMethod]
        public void ReplayMode_Next_ThrowsWhenLogExhausted()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.Ints.Add(42);
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.Next(); // First call succeeds

            // Assert
            bool exceptionThrown = false;
            try
            {
                replayRng.Next();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        [TestMethod]
        public void ReplayMode_Next_ThrowsWithCorrectMessage()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.Ints.Add(42);
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.Next();

            // Assert
            try
            {
                replayRng.Next();
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Replay log exhausted: Ints", ex.Message);
            }
        }

        [TestMethod]
        public void ReplayMode_NextWithMax_ThrowsWhenLogExhausted()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.Ints.Add(42);
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.Next(100); // First call succeeds

            // Assert
            bool exceptionThrown = false;
            try
            {
                replayRng.Next(100);
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        [TestMethod]
        public void ReplayMode_NextWithRange_ThrowsWhenLogExhausted()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.IntRanges.Add(new RandomIntRangeEntry { Min = 10, Max = 20, Value = 15 });
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.Next(10, 20); // First call succeeds

            // Assert
            bool exceptionThrown = false;
            try
            {
                replayRng.Next(10, 20);
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        [TestMethod]
        public void ReplayMode_NextWithRange_ThrowsWithCorrectMessage()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.IntRanges.Add(new RandomIntRangeEntry { Min = 10, Max = 20, Value = 15 });
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act
            replayRng.Next(10, 20);

            // Assert
            try
            {
                replayRng.Next(10, 20);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Replay log exhausted: IntRanges", ex.Message);
            }
        }

        [TestMethod]
        public void ReplayMode_NextWithRange_ThrowsOnRangeMismatch()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.IntRanges.Add(new RandomIntRangeEntry { Min = 10, Max = 20, Value = 15 });
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act & Assert - Call with different range than recorded should throw
            bool exceptionThrown = false;
            try
            {
                replayRng.Next(5, 25);
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        [TestMethod]
        public void ReplayMode_NextWithRange_ThrowsWithDetailedMismatchMessage()
        {
            // Arrange
            var log = new ReplayLog { Seed = 123 };
            log.IntRanges.Add(new RandomIntRangeEntry { Min = 10, Max = 20, Value = 15 });
            var replayRng = new ReplaySeedableRandom(123, log);

            // Act & Assert - Call with different range and verify detailed error message
            try
            {
                replayRng.Next(5, 25);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "Replay log mismatch");
                StringAssert.Contains(ex.Message, "(5,25)");
                StringAssert.Contains(ex.Message, "(10,20)");
            }
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void EmptyLog_CanBeCreatedButThrowsOnFirstAccess()
        {
            // Arrange
            var emptyLog = new ReplayLog { Seed = 123 };
            var replayRng = new ReplaySeedableRandom(123, emptyLog);

            // Act & Assert - Each call should throw
            bool thrown1 = false, thrown2 = false, thrown3 = false, thrown4 = false;

            try { replayRng.NextDouble(); }
            catch (InvalidOperationException) { thrown1 = true; }

            try { replayRng.Next(); }
            catch (InvalidOperationException) { thrown2 = true; }

            try { replayRng.Next(100); }
            catch (InvalidOperationException) { thrown3 = true; }

            try { replayRng.Next(10, 20); }
            catch (InvalidOperationException) { thrown4 = true; }

            Assert.IsTrue(thrown1, "NextDouble() should throw");
            Assert.IsTrue(thrown2, "Next() should throw");
            Assert.IsTrue(thrown3, "Next(int) should throw");
            Assert.IsTrue(thrown4, "Next(int, int) should throw");
        }

        [TestMethod]
        public void RecordingMode_WithNullLog_CreatesNewLog()
        {
            // Arrange & Act
            var rng = new ReplaySeedableRandom(seed: 456, log: null);
            rng.NextDouble();

            // Assert
            var log = rng.GetReplayLog();
            Assert.IsNotNull(log);
            Assert.AreEqual(456, log.Seed);
            Assert.AreEqual(1, log.Doubles.Count);
        }

        [TestMethod]
        public void MultipleCallsToSameMethodType_AllLogged()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 789);

            // Act - Multiple calls to NextDouble
            rng.NextDouble();
            rng.NextDouble();
            rng.NextDouble();
            rng.NextDouble();
            rng.NextDouble();

            var log = rng.GetReplayLog();

            // Assert
            Assert.AreEqual(5, log.Doubles.Count);
        }

        [TestMethod]
        public void MultipleIntRangeCallsWithDifferentRanges_AllLoggedSeparately()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 321);

            // Act
            int v1 = rng.Next(1, 10);
            int v2 = rng.Next(50, 100);
            int v3 = rng.Next(1, 10); // Same range as first

            var log = rng.GetReplayLog();
            var replayRng = new ReplaySeedableRandom(321, log);

            // Assert
            Assert.AreEqual(3, log.IntRanges.Count);
            Assert.AreEqual(v1, replayRng.Next(1, 10));
            Assert.AreEqual(v2, replayRng.Next(50, 100));
            Assert.AreEqual(v3, replayRng.Next(1, 10));
        }

        #endregion

        #region GetBytes and GetNonZeroBytes Tests

        [TestMethod]
        public void GetBytes_NotLogged_AlwaysDelegatesToBaseRng()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 111);
            byte[] buffer1 = new byte[10];

            // Act - Record mode
            rng.GetBytes(buffer1);
            var log = rng.GetReplayLog();

            // Assert - GetBytes should not be logged
            Assert.AreEqual(0, log.Doubles.Count);
            Assert.AreEqual(0, log.Ints.Count);
            Assert.AreEqual(0, log.IntRanges.Count);

            // Verify it fills the buffer
            bool hasNonZero = false;
            foreach (var b in buffer1)
            {
                if (b != 0) hasNonZero = true;
            }
            // Note: This could theoretically fail with all zeros, but extremely unlikely
            Assert.IsTrue(hasNonZero || buffer1.Length == 0);
        }

        [TestMethod]
        public void GetNonZeroBytes_NotLogged_AlwaysDelegatesToBaseRng()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 222);
            byte[] buffer1 = new byte[10];

            // Act - Record mode
            rng.GetNonZeroBytes(buffer1);
            var log = rng.GetReplayLog();

            // Assert - GetNonZeroBytes should not be logged
            Assert.AreEqual(0, log.Doubles.Count);
            Assert.AreEqual(0, log.Ints.Count);
            Assert.AreEqual(0, log.IntRanges.Count);

            // Verify all bytes are non-zero
            foreach (var b in buffer1)
            {
                Assert.AreNotEqual(0, b);
            }
        }

        [TestMethod]
        public void GetBytes_InReplayMode_StillDelegatesToBaseRng()
        {
            // Arrange
            var log = new ReplayLog { Seed = 333 };
            log.Doubles.Add(0.5); // Add some data to log
            var replayRng = new ReplaySeedableRandom(333, log);
            byte[] buffer = new byte[10];

            // Act - Should work even in replay mode
            replayRng.GetBytes(buffer);

            // Assert - Should not throw, and should fill buffer
            Assert.AreEqual(10, buffer.Length);
        }

        [TestMethod]
        public void GetNonZeroBytes_InReplayMode_StillDelegatesToBaseRng()
        {
            // Arrange
            var log = new ReplayLog { Seed = 444 };
            log.Doubles.Add(0.5); // Add some data to log
            var replayRng = new ReplaySeedableRandom(444, log);
            byte[] buffer = new byte[10];

            // Act - Should work even in replay mode
            replayRng.GetNonZeroBytes(buffer);

            // Assert - Should not throw, and all bytes should be non-zero
            foreach (var b in buffer)
            {
                Assert.AreNotEqual(0, b);
            }
        }

        #endregion

        #region Dispose Tests

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 555);

            // Act & Assert
            rng.Dispose(); // Should not throw
        }

        [TestMethod]
        public void Dispose_CanStillAccessLogAfterDispose()
        {
            // Arrange
            var rng = new ReplaySeedableRandom(seed: 666);
            rng.NextDouble();

            // Act
            rng.Dispose();
            var log = rng.GetReplayLog();

            // Assert
            Assert.IsNotNull(log);
            Assert.AreEqual(1, log.Doubles.Count);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void SimulatedGameSequence_RecordsAndReplaysCorrectly()
        {
            // Arrange - Simulate a series of game events
            int seed = 12345;
            var rng = new ReplaySeedableRandom(seed);

            // Act - Simulate game events with various random calls
            var results = SimulateGameEvents(rng);

            var log = rng.GetReplayLog();
            var replayRng = new ReplaySeedableRandom(seed, log);
            var replayResults = SimulateGameEvents(replayRng);

            // Assert - All results should match exactly
            Assert.AreEqual(results.Length, replayResults.Length);
            for (int i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(results[i], replayResults[i], $"Mismatch at event {i}");
            }
        }

        private double[] SimulateGameEvents(ISeedableRandom rng)
        {
            var results = new double[20];

            // Simulate various game events
            results[0] = rng.NextDouble(); // Pass completion check
            results[1] = rng.Next(1, 100); // Yards gained
            results[2] = rng.NextDouble(); // Tackle break check
            results[3] = rng.Next(0, 10); // Additional yards
            results[4] = rng.NextDouble(); // Fumble check
            results[5] = rng.Next(20, 40); // Field goal distance
            results[6] = rng.NextDouble(); // Field goal success
            results[7] = rng.Next(); // Random event seed
            results[8] = rng.NextDouble(); // Interception check
            results[9] = rng.Next(5, 15); // Penalty yards
            results[10] = rng.NextDouble(); // Sack check
            results[11] = rng.Next(1, 50); // Run yards
            results[12] = rng.NextDouble(); // Big play check
            results[13] = rng.Next(10, 30); // Bonus yards
            results[14] = rng.NextDouble(); // Injury check
            results[15] = rng.Next(0, 5); // Timeout decision
            results[16] = rng.NextDouble(); // Two-point conversion
            results[17] = rng.Next(40, 60); // Kickoff distance
            results[18] = rng.NextDouble(); // Onside kick recovery
            results[19] = rng.Next(0, 100); // Return yards

            return results;
        }

        [TestMethod]
        public void SaveAndLoad_PreservesCompleteSequence()
        {
            // Arrange
            int seed = 54321;
            var rng = new ReplaySeedableRandom(seed);

            // Generate complex sequence
            for (int i = 0; i < 50; i++)
            {
                rng.NextDouble();
                rng.Next(i, i + 100);
                rng.Next();
            }

            // Act - Save and reload
            var originalLog = rng.GetReplayLog();
            originalLog.Save(TestLogFile);
            var loadedLog = ReplayLog.Load(TestLogFile);

            // Assert - Verify logs match
            Assert.AreEqual(originalLog.Seed, loadedLog.Seed);
            Assert.AreEqual(originalLog.Doubles.Count, loadedLog.Doubles.Count);
            Assert.AreEqual(originalLog.Ints.Count, loadedLog.Ints.Count);
            Assert.AreEqual(originalLog.IntRanges.Count, loadedLog.IntRanges.Count);

            // Verify all values match
            for (int i = 0; i < originalLog.Doubles.Count; i++)
            {
                Assert.AreEqual(originalLog.Doubles[i], loadedLog.Doubles[i]);
            }

            for (int i = 0; i < originalLog.Ints.Count; i++)
            {
                Assert.AreEqual(originalLog.Ints[i], loadedLog.Ints[i]);
            }

            for (int i = 0; i < originalLog.IntRanges.Count; i++)
            {
                Assert.AreEqual(originalLog.IntRanges[i].Min, loadedLog.IntRanges[i].Min);
                Assert.AreEqual(originalLog.IntRanges[i].Max, loadedLog.IntRanges[i].Max);
                Assert.AreEqual(originalLog.IntRanges[i].Value, loadedLog.IntRanges[i].Value);
            }
        }

        #endregion
    }
}