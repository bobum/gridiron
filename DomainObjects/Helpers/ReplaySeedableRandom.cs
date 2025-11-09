using System;

namespace DomainObjects.Helpers
{
    /// <summary>
    /// A wrapper around <see cref="SeedableRandom"/> that provides record-and-replay functionality
    /// for random number generation sequences. This enables deterministic replay of game simulations
    /// for debugging, testing, and verification purposes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Two Modes of Operation:</strong>
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Recording Mode</term>
    /// <description>
    /// When instantiated without a <see cref="ReplayLog"/> (log parameter is null), 
    /// the class operates in recording mode. All random values generated are logged 
    /// to an internal <see cref="ReplayLog"/> that can be retrieved via <see cref="GetReplayLog"/>
    /// and saved to disk for later replay.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Replay Mode</term>
    /// <description>
    /// When instantiated with a pre-existing <see cref="ReplayLog"/>, the class operates 
    /// in replay mode. Instead of generating new random values, it returns the exact 
    /// values stored in the log, ensuring identical execution of the simulation.
    /// </description>
    /// </item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Debugging rare game events by capturing and replaying the exact RNG sequence</description></item>
    /// <item><description>Regression testing to ensure game simulation logic produces consistent results</description></item>
    /// <item><description>Investigating user-reported issues by replaying their game with the same random sequence</description></item>
    /// <item><description>Validating game balance changes by comparing outcomes with identical RNG inputs</description></item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Example Usage:</strong>
    /// <code>
    /// // Recording mode: capture a game simulation
    /// var rng = new ReplaySeedableRandom(seed: 42);
    /// RunGameSimulation(rng);
    /// var log = rng.GetReplayLog();
    /// log.Save("game_replay.json");
    /// 
    /// // Replay mode: reproduce the exact same simulation
    /// var loadedLog = ReplayLog.Load("game_replay.json");
    /// var replayRng = new ReplaySeedableRandom(seed: 42, log: loadedLog);
    /// RunGameSimulation(replayRng); // Produces identical results
    /// </code>
    /// </para>
    /// 
    /// <para>
    /// <strong>Important Notes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>The sequence of random calls during replay must exactly match the recording</description></item>
    /// <item><description>Mismatched calls will throw <see cref="InvalidOperationException"/> with details</description></item>
    /// <item><description>The same seed must be used for both recording and replay (though it's not actively used during replay)</description></item>
    /// </list>
    /// </remarks>
    public class ReplaySeedableRandom : ISeedableRandom
    {
        private readonly SeedableRandom _baseRng;
        private readonly ReplayLog _log;
        private readonly bool _isReplay;
        private int _doubleIndex = 0;
        private int _intIndex = 0;
        private int _intRangeIndex = 0;

        /// <summary>
        /// Initializes a new instance of <see cref="ReplaySeedableRandom"/>.
        /// </summary>
        /// <param name="seed">The seed value for the underlying random number generator.</param>
        /// <param name="log">
        /// Optional replay log. If null, operates in recording mode (captures random values).
        /// If provided, operates in replay mode (returns logged values).
        /// </param>
        public ReplaySeedableRandom(int seed, ReplayLog log = null!)
        {
            _baseRng = new SeedableRandom(seed);
            _log = log ?? new ReplayLog { Seed = seed };
            _isReplay = log != null;
        }

        /// <summary>
        /// Returns a random double between 0.0 and 1.0.
        /// In recording mode, generates and logs a new value.
        /// In replay mode, returns the next logged value.
        /// </summary>
        /// <returns>A random double value.</returns>
        /// <exception cref="InvalidOperationException">Thrown in replay mode if the log is exhausted.</exception>
        public double NextDouble()
        {
            if (_isReplay)
            {
                if (_doubleIndex >= _log.Doubles.Count)
                    throw new InvalidOperationException("Replay log exhausted: Doubles");
                return _log.Doubles[_doubleIndex++];
            }
            var v = _baseRng.NextDouble();
            _log.Doubles.Add(v);
            return v;
        }

        /// <summary>
        /// Returns a non-negative random integer.
        /// In recording mode, generates and logs a new value.
        /// In replay mode, returns the next logged value.
        /// </summary>
        /// <returns>A random integer.</returns>
        /// <exception cref="InvalidOperationException">Thrown in replay mode if the log is exhausted.</exception>
        public int Next()
        {
            if (_isReplay)
            {
                if (_intIndex >= _log.Ints.Count)
                    throw new InvalidOperationException("Replay log exhausted: Ints");
                return _log.Ints[_intIndex++];
            }
            var v = _baseRng.Next();
            _log.Ints.Add(v);
            return v;
        }

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum.
        /// In recording mode, generates and logs a new value.
        /// In replay mode, returns the next logged value.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A random integer between 0 and maxValue - 1.</returns>
        /// <exception cref="InvalidOperationException">Thrown in replay mode if the log is exhausted.</exception>
        public int Next(int maxValue)
        {
            if (_isReplay)
            {
                if (_intIndex >= _log.Ints.Count)
                    throw new InvalidOperationException("Replay log exhausted: Ints");
                return _log.Ints[_intIndex++];
            }
            var v = _baseRng.Next(maxValue);
            _log.Ints.Add(v);
            return v;
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// In recording mode, generates and logs a new value along with the range parameters.
        /// In replay mode, returns the next logged value after validating the range matches.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number.</param>
        /// <returns>A random integer between minValue and maxValue - 1.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown in replay mode if the log is exhausted or if the range parameters don't match the logged entry.
        /// </exception>
        public int Next(int minValue, int maxValue)
        {
            if (_isReplay)
            {
                if (_intRangeIndex >= _log.IntRanges.Count)
                    throw new InvalidOperationException("Replay log exhausted: IntRanges");
                var entry = _log.IntRanges[_intRangeIndex++];
                if (entry.Min != minValue || entry.Max != maxValue)
                    throw new InvalidOperationException($"Replay log mismatch: expected ({minValue},{maxValue}), got ({entry.Min},{entry.Max})");
                return entry.Value;
            }
            var v = _baseRng.Next(minValue, maxValue);
            _log.IntRanges.Add(new RandomIntRangeEntry { Min = minValue, Max = maxValue, Value = v });
            return v;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// Note: This method is not logged and always delegates to the underlying <see cref="SeedableRandom"/>.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        public void GetBytes(byte[] buffer) => _baseRng.GetBytes(buffer);

        /// <summary>
        /// Fills the elements of a specified array of bytes with non-zero random numbers.
        /// Note: This method is not logged and always delegates to the underlying <see cref="SeedableRandom"/>.
        /// </summary>
        /// <param name="data">An array of bytes to contain random numbers.</param>
        public void GetNonZeroBytes(byte[] data) => _baseRng.GetNonZeroBytes(data);

        /// <summary>
        /// Disposes the random number generator.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Retrieves the replay log containing all recorded random values.
        /// This log can be saved to disk and later used to replay the exact same sequence.
        /// </summary>
        /// <returns>The <see cref="ReplayLog"/> containing all recorded random values.</returns>
        public ReplayLog GetReplayLog() => _log;
    }
}