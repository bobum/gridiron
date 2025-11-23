namespace DomainObjects
{
    public class Fumble
    {
        /// <summary>
        /// Player who fumbled the ball (nullable because may be set after creation)
        /// </summary>
        public Player? FumbledBy { get; set; }

        /// <summary>
        /// Player who forced the fumble (if applicable)
        /// </summary>
        public Player? ForcedBy { get; set; }

        /// <summary>
        /// Player who recovered the fumble (nullable because may be set after creation)
        /// </summary>
        public Player? RecoveredBy { get; set; }

        /// <summary>
        /// Yard line where fumble occurred (0-100 scale)
        /// </summary>
        public int FumbleSpot { get; set; }

        /// <summary>
        /// Yard line where ball was recovered (0-100 scale)
        /// </summary>
        public int RecoverySpot { get; set; }

        /// <summary>
        /// Return yards after recovery
        /// </summary>
        public int ReturnYards { get; set; }

        /// <summary>
        /// Whether recovery resulted in touchdown
        /// </summary>
        public bool RecoveryTouchdown { get; set; }

        /// <summary>
        /// Whether fumble went out of bounds
        /// </summary>
        public bool OutOfBounds { get; set; }
    }
}