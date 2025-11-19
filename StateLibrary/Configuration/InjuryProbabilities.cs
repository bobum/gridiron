namespace StateLibrary.Configuration
{
    /// <summary>
    /// Configuration constants for injury occurrence probabilities.
    /// Based on NFL injury statistics, adjusted for gameplay balance.
    /// </summary>
    public static class InjuryProbabilities
    {
      // Base injury rates by play type (per play occurrence)
    /// <summary>
    /// Base rate for run plays: 3.0% (high contact, frequent collisions)
        /// </summary>
 public const double RUN_PLAY_BASE_RATE = 0.03;

        /// <summary>
   /// Base rate for pass plays: 3.0% (contact on tackles)
    /// </summary>
    public const double PASS_PLAY_BASE_RATE = 0.03;

     /// <summary>
   /// Base rate for sacked QBs: 6.0% (vulnerable position, doubled from pass play)
        /// </summary>
    public const double SACK_BASE_RATE = 0.06;

        /// <summary>
 /// Base rate for kickoff plays: 5.0% (highest impact collisions)
/// </summary>
        public const double KICKOFF_BASE_RATE = 0.05;

      /// <summary>
     /// Base rate for punt returns: 4.0% (high-speed collisions)
     /// </summary>
  public const double PUNT_RETURN_BASE_RATE = 0.04;

 /// <summary>
        /// Base rate for field goal attempts: 0.1% (minimal contact)
        /// </summary>
        public const double FIELD_GOAL_BASE_RATE = 0.001;

        // Contact intensity multipliers
        /// <summary>
        /// Gang tackle multiplier (3+ defenders): 1.4x
        /// </summary>
        public const double GANG_TACKLE_MULTIPLIER = 1.4;

        /// <summary>
        /// Multiple pass rusher multiplier: 1.3x
   /// </summary>
      public const double MULTIPLE_RUSHER_MULTIPLIER = 1.3;

        /// <summary>
        /// Kickoff/punt collision multiplier: 1.5x baseline
    /// </summary>
        public const double SPECIAL_TEAMS_COLLISION_MULTIPLIER = 1.5;

        /// <summary>
        /// Out of bounds contact multiplier: 0.5x (less severe)
        /// </summary>
        public const double OUT_OF_BOUNDS_MULTIPLIER = 0.5;

        /// <summary>
     /// Big play multiplier (20+ yards): 1.2x (more defenders involved)
 /// </summary>
     public const double BIG_PLAY_MULTIPLIER = 1.2;

        // Position-specific risk multipliers
   /// <summary>
        /// RB/LB position multiplier: 1.2x (high contact positions)
        /// </summary>
        public const double HIGH_CONTACT_POSITION_MULTIPLIER = 1.2;

        /// <summary>
        /// QB baseline multiplier: 0.7x normal, but 2.0x on sacks
        /// </summary>
     public const double QB_BASE_MULTIPLIER = 0.7;
   public const double QB_SACK_MULTIPLIER = 2.0;

    /// <summary>
        /// K/P position multiplier: 0.3x (minimal contact)
        /// </summary>
        public const double KICKER_MULTIPLIER = 0.3;

        /// <summary>
        /// WR/CB position multiplier: 1.0x (baseline)
        /// </summary>
   public const double SKILL_POSITION_MULTIPLIER = 1.0;

     // Severity distribution (must sum to 1.0)
        /// <summary>
/// Probability of minor injury: 60%
        /// </summary>
        public const double MINOR_INJURY_PROBABILITY = 0.60;

     /// <summary>
        /// Probability of moderate injury: 30%
      /// </summary>
        public const double MODERATE_INJURY_PROBABILITY = 0.30;

        /// <summary>
        /// Probability of game-ending injury: 10%
/// </summary>
      public const double GAME_ENDING_INJURY_PROBABILITY = 0.10;

        // Recovery time (in number of plays)
     /// <summary>
        /// Minor injury recovery: 1-2 plays
  /// </summary>
        public const int MINOR_INJURY_MIN_PLAYS = 1;
      public const int MINOR_INJURY_MAX_PLAYS = 2;

        /// <summary>
        /// Moderate injury: out for rest of current drive (tracked via possession change)
        /// </summary>
        public const int MODERATE_INJURY_PLAYS = int.MaxValue; // Until possession changes

        /// <summary>
        /// Game-ending injury: out for remainder of game
  /// </summary>
        public const int GAME_ENDING_INJURY_PLAYS = int.MaxValue;
    }
}
