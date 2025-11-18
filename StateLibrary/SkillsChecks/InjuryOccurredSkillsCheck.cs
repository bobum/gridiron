using System;
using System.Collections.Generic;
using System.Linq;
using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Skill check to determine if a player sustains an injury during a play.
   /// Factors considered:
    /// - Play type (run, pass, kickoff, etc.)
/// - Player fragility (0-100, higher = more prone to injury)
    /// - Contact intensity (gang tackles, big hits, etc.)
    /// - Position-specific risk factors
    /// </summary>
    public class InjuryOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
   private readonly ISeedableRandom _rng;
     private readonly PlayType _playType;
        private readonly Player _player;
  private readonly int _defendersInvolved;
        private readonly bool _isOutOfBounds;
 private readonly bool _isBigPlay;
   private readonly bool _isSack;

  /// <summary>
        /// Constructor for injury check
    /// </summary>
   /// <param name="rng">Random number generator</param>
  /// <param name="playType">Type of play (Run, Pass, Kickoff, etc.)</param>
   /// <param name="player">Player who might get injured</param>
    /// <param name="defendersInvolved">Number of defenders making contact (for gang tackle detection)</param>
 /// <param name="isOutOfBounds">Whether contact occurred out of bounds (less severe)</param>
  /// <param name="isBigPlay">Whether this was a big play (20+ yards)</param>
    /// <param name="isSack">Whether this was a QB sack</param>
        public InjuryOccurredSkillsCheck(
    ISeedableRandom rng,
        PlayType playType,
    Player player,
   int defendersInvolved = 1,
      bool isOutOfBounds = false,
    bool isBigPlay = false,
        bool isSack = false)
{
   _rng = rng ?? throw new ArgumentNullException(nameof(rng));
   _playType = playType;
      _player = player ?? throw new ArgumentNullException(nameof(player));
 _defendersInvolved = defendersInvolved;
    _isOutOfBounds = isOutOfBounds;
   _isBigPlay = isBigPlay;
      _isSack = isSack;
}

        public override void Execute(Game game)
    {
      // Calculate injury probability based on all factors
  double injuryProbability = CalculateInjuryProbability();

         // Roll for injury
   double roll = _rng.NextDouble();
    Occurred = roll < injuryProbability;
    Margin = injuryProbability - roll; // Positive = occurred, negative = avoided
 }

 /// <summary>
        /// Calculates the probability of injury based on multiple factors
        /// </summary>
   private double CalculateInjuryProbability()
{
        // Start with base rate for play type
     double baseProbability = GetBaseRateForPlayType();

   // Apply fragility adjustment
   // Fragility ranges from 0-100, where higher = more injury prone
  // Formula: 0.5 + (Fragility / 100.0) yields 0.5x to 1.5x multiplier
        double fragilityFactor = 0.5 + (_player.Fragility / 100.0);
            double probability = baseProbability * fragilityFactor;

       // Apply contact intensity multipliers
  if (_isOutOfBounds)
    {
    probability *= InjuryProbabilities.OUT_OF_BOUNDS_MULTIPLIER;
         }

      if (_defendersInvolved >= 3)
            {
   probability *= InjuryProbabilities.GANG_TACKLE_MULTIPLIER;
}

          if (_isBigPlay)
    {
      probability *= InjuryProbabilities.BIG_PLAY_MULTIPLIER;
   }

  // Apply position-specific multipliers
          probability *= GetPositionMultiplier();

        // Special case: QB sacks have higher injury risk
 if (_isSack && _player.Position == Positions.QB)
      {
    probability *= InjuryProbabilities.QB_SACK_MULTIPLIER;
    }

       return probability;
        }

        /// <summary>
        /// Gets base injury rate for the play type
     /// </summary>
   private double GetBaseRateForPlayType()
  {
            return _playType switch
          {
       PlayType.Run => InjuryProbabilities.RUN_PLAY_BASE_RATE,
             PlayType.Pass => _isSack ? InjuryProbabilities.SACK_BASE_RATE : InjuryProbabilities.PASS_PLAY_BASE_RATE,
  PlayType.Kickoff => InjuryProbabilities.KICKOFF_BASE_RATE,
  PlayType.Punt => InjuryProbabilities.PUNT_RETURN_BASE_RATE,
       PlayType.FieldGoal => InjuryProbabilities.FIELD_GOAL_BASE_RATE,
      _ => 0.001 // Fallback minimal rate
            };
      }

        /// <summary>
   /// Gets position-specific injury risk multiplier
    /// </summary>
    private double GetPositionMultiplier()
{
return _player.Position switch
         {
// High contact positions
    Positions.RB or Positions.LB or Positions.OLB => InjuryProbabilities.HIGH_CONTACT_POSITION_MULTIPLIER,
    
      // QB has reduced baseline but increased sack risk (handled separately)
           Positions.QB => InjuryProbabilities.QB_BASE_MULTIPLIER,
          
       // Kickers/Punters rarely get injured
        Positions.K or Positions.P => InjuryProbabilities.KICKER_MULTIPLIER,
  
  // Skill positions (WR, CB, S, etc.) at baseline
   _ => InjuryProbabilities.SKILL_POSITION_MULTIPLIER
      };
     }
    }
}
