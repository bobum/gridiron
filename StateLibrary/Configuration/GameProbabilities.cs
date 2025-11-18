namespace StateLibrary.Configuration
{
    /// <summary>
    /// Central configuration for all game probability values.
    /// Modify these values to tune game balance and simulation realism.
    /// All probability values are organized by game domain for easy discovery and maintenance.
    /// </summary>
    public static class GameProbabilities
    {
        /// <summary>
        /// Passing game probabilities including completion rates, interceptions, pressure, and yards after catch.
        /// </summary>
        public static class Passing
        {
            // Pass Type Distribution
            public const double SCREEN_PASS_THRESHOLD = 0.15;        // 15% of passes are screens
            public const double SHORT_PASS_THRESHOLD = 0.50;         // Next 35% are short passes (cumulative 50%)
            public const double FORWARD_PASS_THRESHOLD = 0.85;       // Next 35% are forward passes (cumulative 85%)
                                                                      // Remaining 15% are deep passes

            // Pass Completion
            public const double COMPLETION_BASE_PROBABILITY = 0.60;  // Base 60% completion rate
            public const double COMPLETION_PRESSURE_PENALTY = 0.20;  // -20% when QB is under pressure
            public const double COMPLETION_MIN_CLAMP = 0.25;         // Minimum 25% completion rate
            public const double COMPLETION_MAX_CLAMP = 0.85;         // Maximum 85% completion rate
            public const double COMPLETION_SKILL_DENOMINATOR = 250.0; // Skill differential divisor for completion adjustment

            // Interceptions
            public const double INTERCEPTION_BASE_PROBABILITY = 0.035; // 3.5% interception rate on incomplete passes
            public const double INTERCEPTION_PRESSURE_BONUS = 0.02;    // +2% interception chance when QB pressured
            public const double INTERCEPTION_MIN_CLAMP = 0.01;         // Minimum 1% interception rate
            public const double INTERCEPTION_MAX_CLAMP = 0.15;         // Maximum 15% interception rate

            // QB Pressure
            public const double QB_PRESSURE_BASE_PROBABILITY = 0.30;   // 30% base pressure rate
            public const double QB_PRESSURE_MIN_CLAMP = 0.10;          // Minimum 10% pressure rate
            public const double QB_PRESSURE_MAX_CLAMP = 0.60;          // Maximum 60% pressure rate
            public const double QB_PRESSURE_SKILL_DENOMINATOR = 250.0; // Skill differential divisor

            // Pass Protection (Sacks)
            public const double PASS_PROTECTION_BASE_PROBABILITY = 0.75; // 75% protection success rate
            public const double PASS_PROTECTION_MIN_CLAMP = 0.40;        // Minimum 40% protection success
            public const double PASS_PROTECTION_MAX_CLAMP = 0.95;        // Maximum 95% protection success (sacks are relatively rare)
            public const double PASS_PROTECTION_SKILL_DENOMINATOR = 200.0; // Skill differential divisor

            // Yards After Catch (YAC)
            public const double YAC_OPPORTUNITY_BASE_PROBABILITY = 0.35; // 35% chance for YAC opportunity
            public const double YAC_SKILL_THRESHOLD = 70.0;              // Receiver skill threshold for YAC bonus
            public const double YAC_SKILL_DENOMINATOR = 400.0;           // Skill bonus divisor
            public const double YAC_MIN_CLAMP = 0.15;                    // Minimum 15% YAC opportunity
            public const double YAC_MAX_CLAMP = 0.55;                    // Maximum 55% YAC opportunity

            // Big Play After Catch
            public const double BIG_PLAY_YAC_PROBABILITY = 0.05;         // 5% chance for big play after catch
            public const double BIG_PLAY_YAC_SPEED_THRESHOLD = 85.0;     // Requires 85+ speed rating
            public const int BIG_PLAY_YAC_MIN_BONUS = 10;                // Minimum bonus yards on big play
            public const int BIG_PLAY_YAC_MAX_BONUS = 30;                // Maximum bonus yards on big play
        }

        /// <summary>
        /// Rushing game probabilities including scrambles, tackle breaks, big runs, and blocking.
        /// </summary>
        public static class Rushing
        {
            // QB Scramble
            public const double QB_SCRAMBLE_PROBABILITY = 0.10;          // 10% chance QB keeps ball for scramble/option

            // Tackle Break
            public const double TACKLE_BREAK_BASE_PROBABILITY = 0.25;    // 25% base tackle break rate (for elite backs)
            public const double TACKLE_BREAK_MIN_CLAMP = 0.05;           // Minimum 5% tackle break rate
            public const double TACKLE_BREAK_MAX_CLAMP = 0.50;           // Maximum 50% tackle break rate
            public const double TACKLE_BREAK_SKILL_DENOMINATOR = 250.0;  // Skill differential divisor

            // Big Run Breakaway
            public const double BIG_RUN_BASE_PROBABILITY = 0.08;         // 8% base big run probability
            public const double BIG_RUN_SPEED_THRESHOLD = 70.0;          // Speed threshold for bonus
            public const double BIG_RUN_SPEED_DENOMINATOR = 500.0;       // Speed bonus divisor
            public const double BIG_RUN_MIN_CLAMP = 0.03;                // Minimum 3% big run rate
            public const double BIG_RUN_MAX_CLAMP = 0.15;                // Maximum 15% big run rate

            // Blocking Success
            public const double BLOCKING_SUCCESS_BASE_PROBABILITY = 0.50; // 50% base blocking success
            public const double BLOCKING_SUCCESS_MIN_CLAMP = 0.20;        // Minimum 20% blocking success
            public const double BLOCKING_SUCCESS_MAX_CLAMP = 0.80;        // Maximum 80% blocking success
            public const double BLOCKING_SUCCESS_SKILL_DENOMINATOR = 200.0; // Skill differential divisor
        }

        /// <summary>
        /// Turnover probabilities including fumbles and fumble recoveries.
        /// </summary>
        public static class Turnovers
        {
            // Fumble Probability by Play Type
            public const double FUMBLE_QB_SACK_PROBABILITY = 0.12;       // 12% strip sack fumble rate
            public const double FUMBLE_RETURN_PROBABILITY = 0.025;       // 2.5% fumble rate on kickoff/punt returns
            public const double FUMBLE_NORMAL_PROBABILITY = 0.015;       // 1.5% fumble rate on normal plays
            public const double FUMBLE_MIN_CLAMP = 0.003;                // Minimum 0.3% fumble rate
            public const double FUMBLE_MAX_CLAMP = 0.25;                 // Maximum 25% fumble rate

            // Fumble Multipliers
            public const double FUMBLE_GANG_TACKLE_MULTIPLIER = 1.3;     // 1.3x fumble chance with 3+ defenders
            public const double FUMBLE_TWO_DEFENDERS_MULTIPLIER = 1.15;  // 1.15x fumble chance with 2 defenders

            // Fumble Recovery
            public const double FUMBLE_OUT_OF_BOUNDS_PROBABILITY = 0.12; // 12% chance fumble goes out of bounds
            public const double FUMBLE_RECOVERY_BACKWARD_BASE = 0.50;    // 50% offense recovery on backward bounce
            public const double FUMBLE_RECOVERY_FORWARD_BASE = 0.70;     // 70% offense recovery on forward bounce
            public const double FUMBLE_RECOVERY_SIDEWAYS_BASE = 0.60;    // 60% offense recovery on sideways bounce
            public const double FUMBLE_RECOVERY_BACKWARD_THRESHOLD = 0.4; // Random threshold for backward bounce
            public const double FUMBLE_RECOVERY_FORWARD_THRESHOLD = 0.7;  // Random threshold for forward bounce
            public const double FUMBLE_RECOVERY_AWARENESS_FACTOR = 0.15; // ±15% adjustment for awareness differential
            public const double FUMBLE_RECOVERY_MIN_CLAMP = 0.3;         // Minimum 30% recovery chance
            public const double FUMBLE_RECOVERY_MAX_CLAMP = 0.8;         // Maximum 80% recovery chance
        }

        /// <summary>
        /// Field goal probabilities including make rates by distance, blocks, and recoveries.
        /// </summary>
        public static class FieldGoals
        {
            // Make Probability by Distance
            public const double FG_MAKE_VERY_SHORT = 0.98;               // 98% make rate ≤30 yards (extra points)
            public const double FG_MAKE_SHORT_BASE = 0.90;               // 90% make rate at 30 yards
            public const double FG_MAKE_MEDIUM_BASE = 0.80;              // 80% make rate at 40 yards
            public const double FG_MAKE_LONG_BASE = 0.65;                // 65% make rate at 50 yards
            public const double FG_MAKE_VERY_LONG_BASE = 0.40;           // 40% make rate at 60 yards

            // Make Probability Decay Rates (per yard)
            public const double FG_MAKE_SHORT_DECAY = 0.01;              // 1% decay per yard (30-40 yards)
            public const double FG_MAKE_MEDIUM_DECAY = 0.015;            // 1.5% decay per yard (40-50 yards)
            public const double FG_MAKE_LONG_DECAY = 0.025;              // 2.5% decay per yard (50-60 yards)
            public const double FG_MAKE_VERY_LONG_DECAY = 0.03;          // 3% decay per yard (60+ yards)

            // Distance Thresholds
            public const int FG_DISTANCE_SHORT = 30;                     // Short field goal threshold
            public const int FG_DISTANCE_MEDIUM = 40;                    // Medium field goal threshold
            public const int FG_DISTANCE_LONG = 50;                      // Long field goal threshold
            public const int FG_DISTANCE_VERY_LONG = 60;                 // Very long field goal threshold

            // Make Probability Adjustments
            public const double FG_MAKE_SKILL_DENOMINATOR = 200.0;       // Kicker skill adjustment divisor
            public const double FG_MAKE_MIN_CLAMP = 0.05;                // Minimum 5% make rate
            public const double FG_MAKE_MAX_CLAMP = 0.99;                // Maximum 99% make rate

            // Block Probability
            public const double FG_BLOCK_VERY_SHORT = 0.015;             // 1.5% block rate ≤30 yards
            public const double FG_BLOCK_SHORT = 0.025;                  // 2.5% block rate 30-45 yards
            public const double FG_BLOCK_MEDIUM = 0.040;                 // 4% block rate 45-55 yards
            public const double FG_BLOCK_LONG = 0.065;                   // 6.5% block rate 55+ yards
            public const double FG_BLOCK_BAD_SNAP_MULTIPLIER = 10.0;     // 10x block chance on bad snap
            public const double FG_BLOCK_KICKER_SKILL_DENOMINATOR = 300.0; // Kicker skill factor divisor
            public const double FG_BLOCK_DEFENDER_SKILL_FACTOR = 0.003;  // Adjustment per 10 skill points differential
            public const double FG_BLOCK_MIN_CLAMP = 0.005;              // Minimum 0.5% block rate
            public const double FG_BLOCK_MAX_CLAMP = 0.25;               // Maximum 25% block rate

            // Block Distance Thresholds
            public const int FG_BLOCK_DISTANCE_SHORT = 30;               // Short kick threshold for blocking
            public const int FG_BLOCK_DISTANCE_MEDIUM = 45;              // Medium kick threshold for blocking
            public const int FG_BLOCK_DISTANCE_LONG = 55;                // Long kick threshold for blocking

            // Blocked FG Recovery
            public const double BLOCKED_FG_DEFENSE_RECOVERY = 0.5;       // 50% defense recovery on blocked kick

            // Field Goal Miss Direction
            public const double FG_MISS_WIDE_RIGHT_THRESHOLD = 0.4;      // 40% of misses go wide right
            public const double FG_MISS_WIDE_LEFT_THRESHOLD = 0.8;       // 40% of misses go wide left (cumulative 80%)
                                                                          // Remaining 20% of misses are short
        }

        /// <summary>
        /// Kickoff probabilities including onside kicks, muffed catches, and out of bounds.
        /// </summary>
        public static class Kickoffs
        {
            // Onside Kick
            public const double ONSIDE_ATTEMPT_PROBABILITY = 0.05;       // 5% chance to attempt onside when trailing by 7+
            public const double ONSIDE_RECOVERY_BASE_PROBABILITY = 0.20; // 20% base recovery rate
            public const double ONSIDE_RECOVERY_SKILL_BONUS = 0.10;      // Up to +10% from kicker skill
            public const double ONSIDE_RECOVERY_SKILL_DENOMINATOR = 100.0; // Kicker skill divisor

            // Out of Bounds
            public const double KICKOFF_OOB_NORMAL = 0.03;               // 3% out of bounds on normal kicks
            public const double KICKOFF_OOB_DANGER_ZONE = 0.10;          // 10% out of bounds in danger zone
            public const int KICKOFF_OOB_DANGER_MIN = 65;                // Danger zone minimum yardage
            public const int KICKOFF_OOB_DANGER_MAX = 95;                // Danger zone maximum yardage

            // Muffed Catch
            public const double KICKOFF_MUFF_BASE = 0.015;               // 1.5% base muff rate
            public const double KICKOFF_MUFF_SHORT_KICK = 0.04;          // 4% muff rate on short kicks
            public const int KICKOFF_MUFF_SHORT_THRESHOLD = 50;          // Short kick distance threshold
            public const double KICKOFF_MUFF_SKILL_DENOMINATOR = 150.0;  // Returner skill adjustment divisor

            // Muff Recovery
            public const double KICKOFF_MUFF_RECEIVING_TEAM_RECOVERY = 0.6; // 60% receiving team recovers muff
        }

        /// <summary>
        /// Punt probabilities including bad snaps, blocks, muffs, fair catches, downed punts, and out of bounds.
        /// </summary>
        public static class Punts
        {
            // Bad Snap
            public const double PUNT_BAD_SNAP_BASE = 0.05;               // 5% worst case bad snap rate
            public const double PUNT_BAD_SNAP_SKILL_FACTOR = 0.04;       // Reduction factor based on long snapper skill
            public const double PUNT_BAD_SNAP_SKILL_DENOMINATOR = 100.0; // Long snapper skill divisor

            // Block Probability
            public const double PUNT_BLOCK_GOOD_SNAP = 0.01;             // 1% block rate on good snap
            public const double PUNT_BLOCK_BAD_SNAP = 0.20;              // 20% block rate on bad snap
            public const double PUNT_BLOCK_PUNTER_SKILL_DENOMINATOR = 200.0; // Punter skill factor divisor
            public const double PUNT_BLOCK_DEFENDER_SKILL_FACTOR = 0.005; // Adjustment per 10 skill points differential
            public const double PUNT_BLOCK_MIN_CLAMP = 0.002;            // Minimum 0.2% block rate
            public const double PUNT_BLOCK_MAX_CLAMP = 0.30;             // Maximum 30% block rate

            // Muffed Catch
            public const double PUNT_MUFF_BASE = 0.05;                   // 5% worst case muff rate
            public const double PUNT_MUFF_SKILL_FACTOR = 0.04;           // Reduction factor based on returner catching
            public const double PUNT_MUFF_SKILL_DENOMINATOR = 100.0;     // Returner skill divisor
            public const double PUNT_MUFF_HIGH_HANG_TIME_BONUS = 0.02;   // +2% muff chance over 4.5 seconds hang time
            public const double PUNT_MUFF_MEDIUM_HANG_TIME_BONUS = 0.01; // +1% muff chance over 4.0 seconds hang time
            public const double PUNT_MUFF_HIGH_HANG_THRESHOLD = 4.5;     // High hang time threshold (seconds)
            public const double PUNT_MUFF_MEDIUM_HANG_THRESHOLD = 4.0;   // Medium hang time threshold (seconds)

            // Fair Catch
            public const double PUNT_FAIR_CATCH_BASE = 0.25;             // 25% base fair catch rate
            public const double PUNT_FAIR_CATCH_HIGH_HANG_BONUS = 0.15;  // +15% for high hang time (>4.5s)
            public const double PUNT_FAIR_CATCH_MEDIUM_HANG_BONUS = 0.10; // +10% for medium hang time (>4.0s)
            public const double PUNT_FAIR_CATCH_OWN_10_BONUS = 0.20;     // +20% inside own 10 yard line
            public const double PUNT_FAIR_CATCH_OWN_20_BONUS = 0.10;     // +10% inside own 20 yard line

            // Out of Bounds
            public const double PUNT_OOB_BASE = 0.12;                    // 12% base out of bounds rate
            public const double PUNT_OOB_INSIDE_10_BONUS = 0.08;         // +8% inside opponent's 10 yard line
            public const double PUNT_OOB_INSIDE_15_BONUS = 0.05;         // +5% inside opponent's 15 yard line
            public const int PUNT_OOB_INSIDE_10_THRESHOLD = 90;          // Inside 10 field position threshold
            public const int PUNT_OOB_INSIDE_15_THRESHOLD = 85;          // Inside 15 field position threshold

            // Downed
            public const double PUNT_DOWNED_BASE = 0.15;                 // 15% base downed rate
            public const double PUNT_DOWNED_INSIDE_5_BONUS = 0.40;       // +40% inside opponent's 5 yard line
            public const double PUNT_DOWNED_INSIDE_10_BONUS = 0.25;      // +25% inside opponent's 10 yard line
            public const double PUNT_DOWNED_INSIDE_15_BONUS = 0.15;      // +15% inside opponent's 15 yard line
            public const double PUNT_DOWNED_HIGH_HANG_BONUS = 0.10;      // +10% for high hang time (>4.5s)
            public const double PUNT_DOWNED_MEDIUM_HANG_BONUS = 0.05;    // +5% for medium hang time (>4.0s)
            public const int PUNT_DOWNED_INSIDE_5_THRESHOLD = 95;        // Inside 5 field position threshold
            public const int PUNT_DOWNED_INSIDE_10_THRESHOLD = 90;       // Inside 10 field position threshold
            public const int PUNT_DOWNED_INSIDE_15_THRESHOLD = 85;       // Inside 15 field position threshold
        }

        /// <summary>
        /// Game decision probabilities including extra point vs 2-point conversion attempts and play selection.
        /// </summary>
        public static class GameDecisions
        {
            // Extra Point vs 2-Point Conversion
            public const double TWO_POINT_CONVERSION_ATTEMPT = 0.10;     // 10% go for 2-point conversion

            // 2-Point Play Selection
            public const double TWO_POINT_RUN_PROBABILITY = 0.5;         // 50% run on 2-point conversion
            public const double TWO_POINT_PASS_PROBABILITY = 0.5;        // 50% pass on 2-point conversion
        }
    }
}
