using System;
using Newtonsoft.Json;

namespace DomainObjects
{
    /// <summary>
    /// Represents an injury that occurred to a player during a game.
    /// Tracks the type, severity, and context of the injury.
    /// </summary>
    public class Injury
    {
        /// <summary>
 /// The type of injury (e.g., Ankle, Knee, Concussion)
        /// </summary>
    public InjuryType Type { get; set; }

        /// <summary>
        /// How severe the injury is (Minor, Moderate, GameEnding)
 /// </summary>
public InjurySeverity Severity { get; set; }

     /// <summary>
/// The player who was injured
        /// </summary>
        [JsonIgnore] // Prevents circular reference: Player -> Injury -> Player
        public Player InjuredPlayer { get; set; }

     /// <summary>
        /// Which play number this injury occurred on (0-based index in Plays list)
        /// </summary>
        public int PlayNumber { get; set; }

        /// <summary>
        /// Whether the player was removed from the field during the play
        /// </summary>
     public bool RemovedFromPlay { get; set; }

 /// <summary>
        /// The player who replaced the injured player (null if none available)
        /// </summary>
   [JsonIgnore] // Prevents potential circular reference issues
        public Player? ReplacementPlayer { get; set; }

        /// <summary>
        /// How many plays until the player can potentially return (for Minor/Moderate injuries)
  /// </summary>
        public int PlaysUntilReturn { get; set; }
  }

    /// <summary>
    /// Types of injuries that can occur during gameplay.
    /// Weighted differently by position.
    /// </summary>
    public enum InjuryType
    {
     None,
     Ankle,
    Knee,
        Shoulder,
     Concussion,
        Hamstring
    }

    /// <summary>
  /// Severity levels for injuries.
    /// Determines how long a player is out.
    /// </summary>
    public enum InjurySeverity
    {
        /// <summary>
        /// Minor injury - out for 1-2 plays
      /// </summary>
        Minor,

      /// <summary>
        /// Moderate injury - out for rest of current drive
        /// </summary>
        Moderate,

        /// <summary>
  /// Game-ending injury - out for remainder of game
        /// </summary>
    GameEnding
    }
}
