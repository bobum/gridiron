using DomainObjects;
using DomainObjects.Helpers;
using System.Collections.Generic;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// False Start - 5 yard dead ball foul, replay down.
    /// Occurs when offensive player moves after being set but before the snap.
    /// Second most common penalty after offensive holding.
    ///
    /// NFL Rule 7-4-2: False Start
    /// </summary>
    public class FalseStartPenalty : PenaltyBase
    {
        public override string Name => "False Start";

        public override string Description =>
            "Offensive player moved illegally before the snap";

        // ==================== Occurrence ====================

        /// <summary>
        /// Base probability: 1.55% per play (second most common penalty)
        /// Source: NFL penalty data analysis
        /// </summary>
        public override double BaseOccurrenceProbability => 0.0155;

        public override PenaltyTiming Timing => PenaltyTiming.PreSnap;

        /// <summary>
        /// False start probability is relatively constant - doesn't vary much by context.
        /// Could be adjusted by:
        /// - Crowd noise (away teams have more false starts)
        /// - Offensive line discipline
        /// - Game pressure/score situation
        ///
        /// For now, using base probability with no adjustments.
        /// </summary>
        public override double CalculateOccurrenceProbability(PenaltyContext context)
        {
            // Base implementation - could add home/away adjustment later
            return BaseOccurrenceProbability;
        }

        // ==================== Eligibility ====================

        public override TeamSide CommittedBy => TeamSide.Offense;

        /// <summary>
        /// Can be committed by offensive linemen and backs.
        /// Wide receivers in motion are NOT false start (they commit illegal shift/motion).
        /// </summary>
        public override List<Positions> EligiblePositions => new List<Positions>
        {
            Positions.T,   // Tackles (most common)
            Positions.G,   // Guards
            Positions.C,   // Center
            Positions.TE,  // Tight Ends (if they're set)
            Positions.RB,  // Running Backs
            Positions.FB   // Fullbacks
        };

        // ==================== Enforcement ====================

        /// <summary>
        /// 5 yards from the previous spot
        /// </summary>
        public override int Yards => 5;

        /// <summary>
        /// Dead ball foul - play never happens, whistle blows immediately
        /// This is a key difference from live ball fouls
        /// </summary>
        public override bool IsDeadBallFoul => true;

        /// <summary>
        /// Not a spot foul - enforced from line of scrimmage
        /// </summary>
        public override bool IsSpotFoul => false;

        /// <summary>
        /// No loss of down - replay the same down
        /// </summary>
        public override bool IsLossOfDown => false;

        /// <summary>
        /// False start acceptance logic:
        /// Defense always accepts - it's a free 5 yards with no downside.
        /// There's never a reason to decline a false start.
        /// </summary>
        public override bool ShouldAccept(PenaltyAcceptanceContext context)
        {
            // Always accept - no scenario where declining helps defense
            return true;
        }
    }
}
