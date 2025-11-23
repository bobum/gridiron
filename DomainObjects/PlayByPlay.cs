using System;

namespace DomainObjects
{
    /// <summary>
    /// Stores play-by-play execution data for a completed game
    /// Allows exact recreation of game with same seed
    /// </summary>
    public class PlayByPlay : SoftDeletableEntity
    {
        public int Id { get; set; }  // Primary key
        public int GameId { get; set; }  // Foreign key to Game
        public required Game Game { get; set; }  // Navigation property

        /// <summary>
        /// Serialized JSON of all plays (List&lt;IPlay&gt;)
        /// Can be deserialized to recreate exact game flow
        /// </summary>
        public required string PlaysJson { get; set; }

        /// <summary>
        /// Complete play-by-play log as text
        /// Human-readable game narrative
        /// </summary>
        public required string PlayByPlayLog { get; set; }

        /// <summary>
        /// When this game was simulated/saved
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
