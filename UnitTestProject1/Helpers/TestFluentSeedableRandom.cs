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
        /// Sets the tackle break check value. Lower values (&lt; tackle break probability) mean ball carrier breaks the tackle.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom TackleBreakCheck(double value)
        {
            ValidateProbability(value, nameof(TackleBreakCheck),
                "Determines if ball carrier breaks through tackles. " +
                "Lower values (< tackle break probability based on carrier's rushing/strength/agility vs defender's tackling/strength/speed) mean tackle is broken.");
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
        /// Sets the interception occurred check value. Lower values (&lt; interception probability) mean pass is intercepted.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom InterceptionOccurredCheck(double value)
        {
            ValidateProbability(value, nameof(InterceptionOccurredCheck),
                "Determines if incomplete pass is intercepted. " +
                "Base 3.5% probability, adjusted by QB skill vs coverage skill and pressure. " +
                "Lower values (< interception probability) result in interception.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the base return random factor for interception returns (0-7 yards base).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom InterceptionReturnBase(double value)
        {
            ValidateRandomFactor(value, nameof(InterceptionReturnBase),
                "Base return calculation for interceptions. Formula: 8.0 + (factor * 7.0) yields 8-15 yards base return.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets the variance random factor for interception returns (-5 to +25 yards variance).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom InterceptionReturnVariance(double value)
        {
            ValidateRandomFactor(value, nameof(InterceptionReturnVariance),
                "Variance calculation for interceptions. Formula: (factor * 30.0) - 5.0 yields -5 to +25 yards variance.");
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

        // Bad Snap methods

        /// <summary>
        /// Sets bad snap occurrence check. Lower values (&lt; ~2.2% with 70 blocking) mean bad snap occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BadSnapCheck(double value)
        {
            ValidateProbability(value, nameof(BadSnapCheck),
                "Determines if snap is bad. Lower values (< ~2.2% with average long snapper) trigger bad snap.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets bad snap yards - base loss component (maps to -5 to -20 yards for punts, -5 to -15 for FGs).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BadSnapYardsBase(double value)
        {
            ValidateRandomFactor(value, nameof(BadSnapYardsBase),
                "Base yardage loss on bad snap. Higher values = bigger loss. " +
                "Punt: 0.0-1.0 maps to -5 to -20 yards. FG: 0.0-1.0 maps to -5 to -15 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets bad snap yards - random variance component (±2.5 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BadSnapYardsRandom(double value)
        {
            ValidateRandomFactor(value, nameof(BadSnapYardsRandom),
                "Random variance for bad snap yardage (±2.5 yards). Formula: (value - 0.5) * 5.0");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Block methods

        /// <summary>
        /// Sets punt block occurrence check. Lower values (&lt; ~1-30% based on skills) mean block occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntBlockCheck(double value)
        {
            ValidateProbability(value, nameof(PuntBlockCheck),
                "Determines if punt is blocked. Lower values (< block probability based on snap quality and line play) trigger block. " +
                "Good snap: ~1% baseline. Bad snap: ~30% chance.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets field goal block occurrence check. Lower values (&lt; ~1.5-6.5% based on distance) mean block occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom FieldGoalBlockCheck(double value)
        {
            ValidateProbability(value, nameof(FieldGoalBlockCheck),
                "Determines if field goal is blocked. Lower values (< block probability based on distance and line play) trigger block. " +
                "PAT: ~1.5%. Short FG: ~2.5%. Long FG: ~6.5%.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked kick recovery check. For punts: &lt; 0.5 = offense recovers. For FGs: &lt; 0.5 = defense recovers.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedKickRecoveryCheck(double value)
        {
            ValidateProbability(value, nameof(BlockedKickRecoveryCheck),
                "Determines who recovers blocked kick. " +
                "Punts: < 0.5 = offense recovers, >= 0.5 = defense recovers. " +
                "Field goals: < 0.5 = defense recovers, >= 0.5 = offense recovers.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked punt recovery yards for offense (maps to -5 to -10 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedPuntRecoveryYards(double value)
        {
            ValidateRandomFactor(value, nameof(BlockedPuntRecoveryYards),
                "Recovery yards when offense recovers blocked punt (always negative). " +
                "Formula: -5.0 - (value * 5.0) yields -5 to -10 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked field goal recovery yards for offense (maps to -5 to -15 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedFieldGoalRecoveryYards(double value)
        {
            ValidateRandomFactor(value, nameof(BlockedFieldGoalRecoveryYards),
                "Recovery yards when offense recovers blocked field goal (always negative). " +
                "Formula: -5.0 - (value * 10.0) yields -5 to -15 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked punt ball bounce base component (-10 to +15 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedPuntBounceBase(double value)
        {
            ValidateRandomFactor(value, nameof(BlockedPuntBounceBase),
                "Base bounce yards when defense recovers blocked punt. " +
                "Formula: -10.0 + (value * 25.0) yields -10 to +15 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked punt ball bounce random variance (±5 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedPuntBounceRandom(double value)
        {
            ValidateRandomFactor(value, nameof(BlockedPuntBounceRandom),
                "Random variance for blocked punt bounce. Formula: (value - 0.5) * 10.0 yields ±5 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets blocked field goal return yards for defense (-5 to 100 yards).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockedFieldGoalReturnYards(double value)
        {
            ValidateRandomFactor(value, nameof(BlockedFieldGoalReturnYards),
                "Return yards when defense recovers blocked field goal. " +
                "Formula: baseReturn + (value * 100.0) - 50.0 yields -5 to 100 yards (clamped).");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Penalty methods

        /// <summary>
        /// Sets blocking penalty occurrence check. Lower values (&lt; ~2-5%) mean penalty occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom BlockingPenaltyCheck(double value)
        {
            ValidateProbability(value, nameof(BlockingPenaltyCheck),
                "Determines if blocking penalty (holding, illegal block, etc.) occurs. " +
                "Lower values (< ~2-5%) trigger penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets kicker penalty occurrence check (roughing/running into kicker). Lower values (&lt; ~1-3%) mean penalty occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom KickerPenaltyCheck(double value)
        {
            ValidateProbability(value, nameof(KickerPenaltyCheck),
                "Determines if roughing/running into kicker penalty occurs. " +
                "Lower values (< ~1-3%) trigger penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets tackle penalty occurrence check. Lower values (&lt; ~2-5%) mean penalty occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom TacklePenaltyCheck(double value)
        {
            ValidateProbability(value, nameof(TacklePenaltyCheck),
                "Determines if tackle penalty (facemask, horse collar, unnecessary roughness) occurs. " +
                "Lower values (< ~2-5%) trigger penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets roughing the passer penalty check. Lower values (&lt; ~1-3%) mean penalty occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom RoughingPasserCheck(double value)
        {
            ValidateProbability(value, nameof(RoughingPasserCheck),
                "Determines if roughing the passer penalty occurs. " +
                "Lower values (< ~1-3%) trigger penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets coverage penalty occurrence check (pass interference, illegal contact). Lower values (&lt; ~3-5%) mean penalty occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom CoveragePenaltyCheck(double value)
        {
            ValidateProbability(value, nameof(CoveragePenaltyCheck),
                "Determines if coverage penalty (pass interference, illegal contact) occurs. " +
                "Lower values (< ~3-5%) trigger penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Punt special outcome methods

        /// <summary>
        /// Sets punt out of bounds check. Lower values (&lt; ~12-20%) mean punt goes out of bounds.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntOutOfBoundsCheck(double value)
        {
            ValidateProbability(value, nameof(PuntOutOfBoundsCheck),
                "Determines if punt goes out of bounds. " +
                "Lower values (< ~12% baseline, up to 20% near goal line) trigger OOB.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets punt downed check. Lower values (&lt; ~15-55%) mean punt is downed by coverage team.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntDownedCheck(double value)
        {
            ValidateProbability(value, nameof(PuntDownedCheck),
                "Determines if punt is downed by coverage team. " +
                "Lower values (< ~15% baseline, up to 55% inside 5-yard line) trigger downed.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets punt fair catch check. Lower values (&lt; ~25-55%) mean returner calls fair catch.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntFairCatchCheck(double value)
        {
            ValidateProbability(value, nameof(PuntFairCatchCheck),
                "Determines if returner calls fair catch. " +
                "Lower values (< ~25% baseline with low hang time, up to 55% with high hang time or deep field position) trigger fair catch.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets punt muff check. Lower values (&lt; ~1-7%) mean returner muffs the catch.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntMuffCheck(double value)
        {
            ValidateProbability(value, nameof(PuntMuffCheck),
                "Determines if returner muffs the punt. " +
                "Lower values (< ~1% with great returner/low hang time, up to 7% with poor returner/high hang time) trigger muff.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets muff recovery check. For punts: &lt; 0.6 = receiving team. For kickoffs: &gt;= 0.5 = receiving team.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom MuffRecoveryCheck(double value)
        {
            ValidateProbability(value, nameof(MuffRecoveryCheck),
                "Determines who recovers muffed kick. " +
                "Punts: < 0.6 = receiving team, >= 0.6 = punting team (60% receiving team recovers). " +
                "Kickoffs: >= 0.5 = receiving team, < 0.5 = kicking team (50% split).");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets muff recovery yards (-5 to +5 yards from muff spot).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom MuffRecoveryYards(double value)
        {
            ValidateRandomFactor(value, nameof(MuffRecoveryYards),
                "Yards from muff spot where ball is recovered. " +
                "Formula: -5.0 + (value * 10.0) yields -5 to +5 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets punt return yards. Mapped to actual return yardage based on returner skill and coverage.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom PuntReturnYards(double value)
        {
            ValidateRandomFactor(value, nameof(PuntReturnYards),
                "Return yardage for punt return. Lower values = shorter/negative return. " +
                "Formula varies by returner skill and coverage. 0.95+ often yields TD return.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Kickoff special outcome methods

        /// <summary>
        /// Sets kickoff out of bounds check. Lower values (&lt; ~10%) mean kickoff goes out of bounds.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom KickoffOutOfBoundsCheck(double value)
        {
            ValidateProbability(value, nameof(KickoffOutOfBoundsCheck),
                "Determines if kickoff goes out of bounds. " +
                "Lower values (< ~10%) trigger OOB penalty.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets kickoff fair catch check. Lower values (&lt; ~70%) mean returner calls fair catch.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom KickoffFairCatchCheck(double value)
        {
            ValidateProbability(value, nameof(KickoffFairCatchCheck),
                "Determines if returner calls fair catch on kickoff. " +
                "Lower values (< ~70% deep in own territory) trigger fair catch.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets kickoff muff check. Lower values (&lt; ~2-5%) mean returner muffs the kickoff.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom KickoffMuffCheck(double value)
        {
            ValidateProbability(value, nameof(KickoffMuffCheck),
                "Determines if returner muffs the kickoff. " +
                "Lower values (< ~2-5%) trigger muff.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets kickoff return yards. Mapped to actual return yardage based on returner skill and coverage.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom KickoffReturnYards(double value)
        {
            ValidateRandomFactor(value, nameof(KickoffReturnYards),
                "Return yardage for kickoff return. Lower values = shorter return. " +
                "Formula varies by returner skill and coverage. 0.95+ often yields TD return.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets onside kick decision check (conditional - only consumed if team is trailing). Lower values (&lt; ~5%) trigger onside attempt.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom OnsideKickDecisionCheck(double value)
        {
            ValidateProbability(value, nameof(OnsideKickDecisionCheck),
                "Determines if team attempts onside kick (only consumed if trailing by 7+ points). " +
                "Lower values (< ~5%) trigger onside attempt.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets onside kick distance (10-15 yards typically).
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom OnsideKickDistance(double value)
        {
            ValidateRandomFactor(value, nameof(OnsideKickDistance),
                "Distance traveled by onside kick. " +
                "Formula: 10.0 + (value * 5.0) yields 10-15 yards.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets onside kick recovery check. Lower values (&lt; ~20-30%) mean kicking team recovers.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom OnsideKickRecoveryCheck(double value)
        {
            ValidateProbability(value, nameof(OnsideKickRecoveryCheck),
                "Determines who recovers onside kick. " +
                "Lower values (< ~20-30%) = kicking team recovers, >= ~20-30% = receiving team recovers.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Field Goal outcome methods

        /// <summary>
        /// Sets field goal make/miss check. Lower values (&lt; make probability) mean kick is good.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom FieldGoalMakeCheck(double value)
        {
            ValidateProbability(value, nameof(FieldGoalMakeCheck),
                "Determines if field goal is made. " +
                "Lower values (< make probability based on kicker skill and distance) result in made kick. " +
                "PAT: ~98%. Short FG: ~85-95%. Medium FG: ~70-85%. Long FG: ~40-60%.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets miss direction (only consumed if kick misses). &lt; 0.4 = wide right, 0.4-0.8 = wide left, &gt; 0.8 = short.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom MissDirection(double value)
        {
            ValidateRandomFactor(value, nameof(MissDirection),
                "Direction of missed field goal (only consumed if kick misses). " +
                "< 0.4 = wide right, 0.4-0.8 = wide left, > 0.8 = short.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Run play specific methods

        /// <summary>
        /// Sets QB check for run plays. Lower values (&lt; 0.10) mean QB scrambles instead of handoff.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom QBCheck(double value)
        {
            ValidateProbability(value, nameof(QBCheck),
                "Determines if QB scrambles or hands off to RB. " +
                "Lower values (< 0.10) = QB scrambles, >= 0.10 = handoff to RB.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets run direction (0-4: 0=far left, 1=left, 2=middle, 3=right, 4=far right).
        /// Valid range: 0 to 4
        /// </summary>
        public TestFluentSeedableRandom RunDirection(int value)
        {
            if (value < 0 || value > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(RunDirection),
                    value,
                    "Run direction must be 0-4 (0=far left, 1=left, 2=middle, 3=right, 4=far right). Got: " + value);
            }
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets run base yards random factor. Used to calculate base yardage before blocks/breaks.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom RunBaseYardsRandom(double value)
        {
            ValidateRandomFactor(value, nameof(RunBaseYardsRandom),
                "Random factor for run yardage calculation. " +
                "Formula: baseYards + (value * 25.0) - 15.0 yields variable yards based on blocking/skills. " +
                "Lower values (< 0.2) often result in losses. Higher values (> 0.8) result in good gains.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets tackle break yards (bonus yards when tackle is broken).
        /// Valid range: 3 to 15
        /// </summary>
        public TestFluentSeedableRandom TackleBreakYards(int value)
        {
            ValidateYardage(value, nameof(TackleBreakYards), 3, 15,
                "Bonus yards awarded when ball carrier breaks tackle. Typical range: 3-8 yards.");
            _intQueue.Enqueue(value);
            return this;
        }

        /// <summary>
        /// Sets breakaway yards (bonus yards for breakaway run).
        /// Valid range: 10 to 99
        /// </summary>
        public TestFluentSeedableRandom BreakawayYards(int value)
        {
            ValidateYardage(value, nameof(BreakawayYards), 10, 99,
                "Bonus yards for breakaway run (when RB breaks into open field). Typical range: 15-44 yards.");
            _intQueue.Enqueue(value);
            return this;
        }

        // Fumble methods

        /// <summary>
        /// Sets fumble occurrence check. Lower values (&lt; ~1-3%) mean fumble occurs.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom FumbleCheck(double value)
        {
            ValidateProbability(value, nameof(FumbleCheck),
                "Determines if ball carrier fumbles. " +
                "Lower values (< ~1-3% based on carrier's awareness/strength and hits taken) trigger fumble.");
            _doubleQueue.Enqueue(value);
            return this;
        }

        // Injury check methods

        /// <summary>
        /// Sets injury occurrence check. Lower values (&lt; injury probability) mean injury occurs.
        /// Valid range: 0.0 to 1.0
        /// Base rates: Run/Pass 3%, Kickoff 5%, Punt 4%
        /// Modified by fragility, defenders involved, big plays, out of bounds, QB sacks
      /// </summary>
        public TestFluentSeedableRandom InjuryOccurredCheck(double value)
        {
    ValidateProbability(value, nameof(InjuryOccurredCheck),
    "Determines if player sustains injury. " +
             "Base rates: Run/Pass 3%, Kickoff 5%, Punt 4%. " +
            "Modified by player fragility, gang tackles, big plays, etc.");
            _doubleQueue.Enqueue(value);
        return this;
        }

        /// <summary>
        /// Sets injury severity check. &lt; 0.6 = Minor, 0.6-0.9 = Moderate, &gt;= 0.9 = GameEnding.
        /// Valid range: 0.0 to 1.0
        /// </summary>
        public TestFluentSeedableRandom InjurySeverityCheck(double value)
        {
            ValidateProbability(value, nameof(InjurySeverityCheck),
    "Determines injury severity. " +
      "< 0.6 = Minor (60%), 0.6-0.9 = Moderate (30%), >= 0.9 = GameEnding (10%)");
     _doubleQueue.Enqueue(value);
      return this;
   }

        /// <summary>
        /// Sets injury type check. Determines type based on position-specific distributions.
        /// Valid range: 0.0 to 1.0
     /// RB/WR: 0-0.40 Ankle, 0.40-0.65 Knee, 0.65-0.75 Shoulder, 0.75-0.80 Concussion, 0.80-1.0 Hamstring
        /// </summary>
   public TestFluentSeedableRandom InjuryTypeCheck(double value)
        {
   ValidateRandomFactor(value, nameof(InjuryTypeCheck),
   "Determines injury type based on position-specific distributions. " +
      "Example for RB: 0-0.40 Ankle, 0.40-0.65 Knee, 0.65-0.75 Shoulder, etc.");
            _doubleQueue.Enqueue(value);
 return this;
        }

        /// <summary>
/// Sets recovery time for minor injuries (1-2 plays).
        /// Valid range: 1 to 2
        /// </summary>
        public TestFluentSeedableRandom InjuryRecoveryTime(int value)
        {
            if (value < 1 || value > 2)
            {
   throw new ArgumentOutOfRangeException(
                    nameof(InjuryRecoveryTime),
   value,
   "Minor injury recovery time must be 1-2 plays. Got: " + value);
 }
            _intQueue.Enqueue(value);
  return this;
        }

        /// <summary>
    /// Sets tackler injury check (50% chance to check each tackler for injury).
     /// Valid range: 0.0 to 1.0
        /// Values &lt; 0.5 trigger injury check for that tackler
 /// </summary>
        public TestFluentSeedableRandom TacklerInjuryGateCheck(double value)
        {
ValidateProbability(value, nameof(TacklerInjuryGateCheck),
    "50% gate check for whether to check tackler for injury. " +
          "< 0.5 = check tackler, >= 0.5 = skip tackler");
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