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
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PassProtectionCheck(double value)
        {
            ValidateProbability(value, nameof(PassProtectionCheck),
                "Used to determine if offensive line protects QB. " +
                "Lower values (< ~0.75) mean protection holds, higher values mean sack occurs.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the QB pressure check value. Lower values mean no pressure, higher values mean QB is under pressure.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom QBPressureCheck(double value)
        {
            ValidateProbability(value, nameof(QBPressureCheck),
                "Determines if QB is under pressure. " +
                "Lower values mean no pressure, higher values mean QB is under pressure (affects completion rate).");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the receiver selection value. Used for weighted random selection based on catching ability.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom ReceiverSelection(double value)
        {
            ValidateProbability(value, nameof(ReceiverSelection),
                "Used for weighted selection of target receiver. " +
                "Higher values favor receivers with higher catching ratings.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the pass type determination value. &lt; 0.15 = Screen, 0.15-0.50 = Short, 0.50-0.85 = Forward, &gt; 0.85 = Deep.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PassTypeDetermination(double value)
        {
            ValidateProbability(value, nameof(PassTypeDetermination),
                "Determines pass type: < 0.15 = Screen (15%), 0.15-0.50 = Short (35%), " +
                "0.50-0.85 = Forward (35%), > 0.85 = Deep (15%).");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the pass completion check value. Lower values (&lt; completion probability) mean completion, higher values mean incompletion.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PassCompletionCheck(double value)
        {
            ValidateProbability(value, nameof(PassCompletionCheck),
                "Compared against completion probability (based on QB/receiver skills and pressure). " +
                "Lower values (< completion %) result in completion.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the yards after catch opportunity check value. Lower values (&lt; ~0.35-0.55) mean receiver breaks tackles for extra YAC.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom YACOpportunityCheck(double value)
        {
            ValidateProbability(value, nameof(YACOpportunityCheck),
                "Determines if receiver breaks tackles for extra YAC. " +
                "Lower values (< ~0.35-0.55 depending on receiver skills) mean success.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the YAC random factor value (used to add variance to yards after catch calculation).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom YACRandomFactor(double value)
        {
            ValidateRandomFactor(value, nameof(YACRandomFactor),
                "Adds variance to YAC calculation. Formula: randomFactor * 8 - 2 (yields -2 to +6 yards).");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the big play check value. Lower values (&lt; 0.05) trigger a big play with bonus yards.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BigPlayCheck(double value)
        {
            ValidateProbability(value, nameof(BigPlayCheck),
                "5% chance for big play after catch (if receiver speed > 85). " +
                "Values < 0.05 trigger big play with 10-30 bonus yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the elapsed time random factor (0.0-1.0, used in calculation: base + factor * range).
        /// For normal passes: 4.0 + (factor * 3.0) = 4.0 to 7.0 seconds
        /// For sacks: 2.0 + (factor * 2.0) = 2.0 to 4.0 seconds
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom ElapsedTimeRandomFactor(double value)
        {
            ValidateRandomFactor(value, nameof(ElapsedTimeRandomFactor),
                "Random factor for elapsed time calculation. " +
                "Pass plays: 4.0 + (factor * 3.0) = 4.0 to 7.0 seconds. " +
                "Sacks: 2.0 + (factor * 2.0) = 2.0 to 4.0 seconds.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Pass Play - NextInt methods

        /// <summary>
        /// Sets the air yards value for the pass (distance ball travels in the air).
        /// Typical ranges by pass type: Screen (-3 to 3), Short (3-12), Forward (8-20), Deep (18-45)
        /// </summary>
        public TestFluentSeedableRandom AirYards(int value)
        {
            ValidateYardage(value, nameof(AirYards), -10, 100,
                "Distance ball travels in air. Typical ranges: Screen (-3 to 3), Short (3-12), Forward (8-20), Deep (18-45). " +
                "Limited by yards to goal line.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the yards after catch value.
        /// Valid range: 0 to 100
        /// </summary>
        public TestFluentSeedableRandom YACYards(int value)
        {
            ValidateYardage(value, nameof(YACYards), 0, 100,
                "Yards gained after catch (when tackled immediately, 0-2 yards). " +
                "Used in Next(0, 3) call when YACOpportunityCheck fails.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets yards when receiver is tackled immediately (no YAC opportunity).
        /// Valid range: 0 to 2
        /// Alias for YACYards but with more descriptive name for immediate tackle scenario.
        /// </summary>
        public TestFluentSeedableRandom ImmediateTackleYards(int value)
        {
            ValidateYardage(value, nameof(ImmediateTackleYards), 0, 2,
                "Yards when receiver tackled immediately (YAC opportunity check failed). " +
                "Used in Next(0, 3) call. Typical range: 0-2 yards.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the big play bonus yards value (extra yards when big play occurs).
        /// Valid range: 10 to 50
        /// </summary>
        public TestFluentSeedableRandom BigPlayBonusYards(int value)
        {
            ValidateYardage(value, nameof(BigPlayBonusYards), 10, 50,
                "Extra yards awarded when big play occurs (5% chance if receiver speed > 85). " +
                "Typical range: 10-30 yards.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the sack yardage loss value (2-10 yards typically).
        /// Valid range: 2 to 15
        /// </summary>
        public TestFluentSeedableRandom SackYards(int value)
        {
            ValidateYardage(value, nameof(SackYards), 2, 15,
                "Yards lost on sack (returned as negative). " +
                "Typical range: 2-10 yards, limited by field position (can't go past own goal line).");
            _intQueue.Enqueue(value);
            return this;
        }

        // Run Play methods (add as needed)

        /// <summary>
        /// Sets the run blocking check value. Lower values mean successful blocking.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom RunBlockingCheck(double value)
        {
            ValidateProbability(value, nameof(RunBlockingCheck),
                "Determines if offensive line successfully blocks for run play. " +
                "Lower values mean successful blocking.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the run defense check value. Lower values mean defense fails to stop the run.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom RunDefenseCheck(double value)
        {
            ValidateProbability(value, nameof(RunDefenseCheck),
                "Determines if defense successfully stops the run. " +
                "Lower values mean defense fails to stop the run.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the breakaway check value. Lower values trigger a breakaway run.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BreakawayCheck(double value)
        {
            ValidateProbability(value, nameof(BreakawayCheck),
                "Determines if running back breaks free for a long run. " +
                "Lower values (typically < ~0.05-0.10) trigger breakaway.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the run yards value.
        /// Valid range: -10 to 99
        /// </summary>
        public TestFluentSeedableRandom RunYards(int value)
        {
            ValidateYardage(value, nameof(RunYards), -10, 99,
                "Yards gained on run play (can be negative for loss). " +
                "Typical range: -3 to 15 yards, limited by field position.");
            _intQueue.Enqueue(value);
            return this;
        }

        // Kickoff methods (add as needed)

        /// <summary>
        /// Sets the kick distance value (in yards).
        /// Valid range: 20 to 75
        /// </summary>
        public TestFluentSeedableRandom KickDistance(int value)
        {
            ValidateYardage(value, nameof(KickDistance), 20, 75,
                "Distance of kickoff in yards. " +
                "Typical range: 45-70 yards for normal kickoff, 20-40 for onside kick.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the kick hang time value (in seconds).
        /// Valid range: 0.0 to 6.0
        /// </summary>
        public TestFluentSeedableRandom KickHangTime(double value)
        {
            ValidateTimeRange(value, nameof(KickHangTime), 0.0, 6.0,
                "Hang time of kick in seconds. " +
                "Typical range: 3.5-5.0 seconds for normal kickoff, 1.5-2.5 for onside kick.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the return yards value for kickoff returns.
        /// Valid range: 0 to 100
        /// </summary>
        public TestFluentSeedableRandom ReturnYards(int value)
        {
            ValidateYardage(value, nameof(ReturnYards), 0, 100,
                "Yards gained on kickoff return. " +
                "Typical range: 15-30 yards, limited by field position.");
            _intQueue.Enqueue(value);
            return this;
        }

        // Punt methods (add as needed)

        /// <summary>
        /// Sets the punt distance value (in yards).
        /// Valid range: 10 to 70
        /// </summary>
        public TestFluentSeedableRandom PuntDistance(int value)
        {
            ValidateYardage(value, nameof(PuntDistance), 10, 70,
                "Distance of punt in yards. " +
                "Typical range: 35-55 yards, limited by field position.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the punt hang time value (in seconds).
        /// Valid range: 0.0 to 6.0
        /// </summary>
        public TestFluentSeedableRandom PuntHangTime(double value)
        {
            ValidateTimeRange(value, nameof(PuntHangTime), 0.0, 6.0,
                "Hang time of punt in seconds. " +
                "Typical range: 4.0-5.5 seconds.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Field Goal methods (add as needed)

        /// <summary>
        /// Sets the field goal accuracy check value. Lower values mean kick is accurate.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom FieldGoalAccuracyCheck(double value)
        {
            ValidateProbability(value, nameof(FieldGoalAccuracyCheck),
                "Determines if field goal is accurate (direction). " +
                "Lower values (< accuracy threshold based on kicker skill and distance) mean kick is on target.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the field goal distance check value. Lower values mean kick has sufficient distance.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom FieldGoalDistanceCheck(double value)
        {
            ValidateProbability(value, nameof(FieldGoalDistanceCheck),
                "Determines if field goal has sufficient distance to clear crossbar. " +
                "Lower values (< distance threshold based on kicker skill) mean kick has enough power.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Generic methods for edge cases

        /// <summary>
        /// Enqueues a generic double value for NextDouble() calls.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom NextDouble(double value)
        {
            ValidateRandomFactor(value, nameof(NextDouble),
                "Generic random value. Should typically be in range 0.0-1.0 to match standard Random.NextDouble() behavior.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Enqueues a generic int value for Next() calls.
        /// </summary>
        public TestFluentSeedableRandom NextInt(int value)
        {
            // No validation - int can be any value depending on context
            _intQueue.Enqueue(value);
            return this;
        }

        // Validation helper methods

        private void ValidateProbability(double value, string parameterName, string usage)
        {
            if (value < 0.0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"Probability value must be between 0.0 and 1.0. Got: {value}\n" +
                    $"Usage: {usage}");
            }
        }

        private void ValidateRandomFactor(double value, string parameterName, string usage)
        {
            if (value < 0.0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"Random factor must be between 0.0 and 1.0 (used in formula: base + factor * range). Got: {value}\n" +
                    $"Usage: {usage}");
            }
        }

        private void ValidateTimeRange(double value, string parameterName, double min, double max, string usage)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"Time value must be between {min} and {max} seconds. Got: {value}\n" +
                    $"Usage: {usage}");
            }
        }

        private void ValidateYardage(int value, string parameterName, int min, int max, string usage)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"Yardage must be between {min} and {max}. Got: {value}\n" +
                    $"Usage: {usage}");
            }
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