using DomainObjects.Helpers;
using System;
using System.Collections.Generic;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Seedable random for testing with strongly-typed, fluent configuration
    /// </summary>
    public class TestFluentSeedableRandom : ISeedableRandom
    {
        private Queue<double> _doubleQueue = new Queue<double>();
        private Queue<int> _intQueue = new Queue<int>();

        // Pass Play - NextDouble methods

        /// <summary>
        /// Sets the pass protection check value. Lower values (&lt; ~0.75) mean protection holds, higher values mean sack occurs.
        /// </summary>
        public TestFluentSeedableRandom PassProtectionCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the QB pressure check value. Lower values mean no pressure, higher values mean QB is under pressure.
        /// </summary>
        public TestFluentSeedableRandom QBPressureCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the receiver selection value. Used for weighted random selection based on catching ability.
        /// </summary>
        public TestFluentSeedableRandom ReceiverSelection(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the pass type determination value. &lt; 0.15 = Screen, 0.15-0.50 = Short, 0.50-0.85 = Forward, &gt; 0.85 = Deep.
        /// </summary>
        public TestFluentSeedableRandom PassTypeDetermination(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the pass completion check value. Lower values (&lt; completion probability) mean completion, higher values mean incompletion.
        /// </summary>
        public TestFluentSeedableRandom PassCompletionCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the yards after catch opportunity check value. Lower values (&lt; ~0.35-0.55) mean receiver breaks tackles for extra YAC.
        /// </summary>
        public TestFluentSeedableRandom YACOpportunityCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the YAC random factor value (used to add variance to yards after catch calculation).
        /// </summary>
        public TestFluentSeedableRandom YACRandomFactor(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the big play check value. Lower values (&lt; 0.05) trigger a big play with bonus yards.
        /// </summary>
        public TestFluentSeedableRandom BigPlayCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the elapsed time value (used to calculate play duration).
        /// </summary>
        public TestFluentSeedableRandom ElapsedTimeRandomFactor(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Pass Play - NextInt methods

        /// <summary>
        /// Sets the air yards value for the pass (distance ball travels in the air).
        /// </summary>
        public TestFluentSeedableRandom AirYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the yards after catch value.
        /// </summary>
        public TestFluentSeedableRandom YACYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the big play bonus yards value (extra yards when big play occurs).
        /// </summary>
        public TestFluentSeedableRandom BigPlayBonusYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the sack yardage loss value (2-10 yards typically).
        /// </summary>
        public TestFluentSeedableRandom SackYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        // Run Play methods (add as needed)

        /// <summary>
        /// Sets the run blocking check value. Lower values mean successful blocking.
        /// </summary>
        public TestFluentSeedableRandom RunBlockingCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the run defense check value. Lower values mean defense fails to stop the run.
        /// </summary>
        public TestFluentSeedableRandom RunDefenseCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the breakaway check value. Lower values trigger a breakaway run.
        /// </summary>
        public TestFluentSeedableRandom BreakawayCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the run yards value.
        /// </summary>
        public TestFluentSeedableRandom RunYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        // Kickoff methods (add as needed)

        /// <summary>
        /// Sets the kick distance value (in yards).
        /// </summary>
        public TestFluentSeedableRandom KickDistance(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the kick hang time value (in seconds).
        /// </summary>
        public TestFluentSeedableRandom KickHangTime(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the return yards value for kickoff returns.
        /// </summary>
        public TestFluentSeedableRandom ReturnYards(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        // Punt methods (add as needed)

        /// <summary>
        /// Sets the punt distance value (in yards).
        /// </summary>
        public TestFluentSeedableRandom PuntDistance(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the punt hang time value (in seconds).
        /// </summary>
        public TestFluentSeedableRandom PuntHangTime(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Field Goal methods (add as needed)

        /// <summary>
        /// Sets the field goal accuracy check value. Lower values mean kick is accurate.
        /// </summary>
        public TestFluentSeedableRandom FieldGoalAccuracyCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the field goal distance check value. Lower values mean kick has sufficient distance.
        /// </summary>
        public TestFluentSeedableRandom FieldGoalDistanceCheck(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Generic methods for edge cases

        /// <summary>
        /// Enqueues a generic double value for NextDouble() calls.
        /// </summary>
        public TestFluentSeedableRandom NextDouble(double value)
        {
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Enqueues a generic int value for Next() calls.
        /// </summary>
        public TestFluentSeedableRandom NextInt(int value)
        {
            _intQueue.Enqueue(value);
            return this;
        }

        // ISeedableRandom implementation
        double ISeedableRandom.NextDouble()
        {
            if (_doubleQueue.Count == 0)
                throw new InvalidOperationException(
                    "No more double values in queue. Did you forget to add a value using the fluent methods?");

            return _doubleQueue.Dequeue();
        }

        int ISeedableRandom.Next(int minValue, int maxValue)
        {
            if (_intQueue.Count == 0)
                throw new InvalidOperationException(
                    "No more int values in queue. Did you forget to add a value using the fluent methods?");

            return _intQueue.Dequeue();
        }

        int ISeedableRandom.Next(int maxValue)
        {
            return ((ISeedableRandom)this).Next(0, maxValue);
        }

        int ISeedableRandom.Next()
        {
            return ((ISeedableRandom)this).Next(0, int.MaxValue);
        }

        void ISeedableRandom.GetBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Fill buffer with sequential byte values for testing
            for (int i = 0; i < buffer.Length; i++)
            {
                if (_intQueue.Count > 0)
                {
                    buffer[i] = (byte)(_intQueue.Dequeue() % 256);
                }
                else
                {
                    buffer[i] = (byte)(i % 256);
                }
            }
        }

        void ISeedableRandom.GetNonZeroBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Fill buffer with sequential non-zero byte values for testing
            for (int i = 0; i < data.Length; i++)
            {
                if (_intQueue.Count > 0)
                {
                    int value = _intQueue.Dequeue() % 255;
                    data[i] = (byte)(value == 0 ? 1 : value);
                }
                else
                {
                    data[i] = (byte)((i % 255) + 1);
                }
            }
        }

        public void Dispose()
        {
            // Nothing to dispose in test implementation
            _doubleQueue.Clear();
            _intQueue.Clear();
        }

        // Backward compatibility properties (deprecated, but kept for migration)
        [Obsolete("Use fluent methods instead")]
        public double[] __NextDouble { get; set; } = new double[99];

        [Obsolete("Use fluent methods instead")]
        public int[] __NextInt { get; set; } = new int[99];
    }
}