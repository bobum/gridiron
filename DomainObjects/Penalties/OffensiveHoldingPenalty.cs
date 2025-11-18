using DomainObjects;
using DomainObjects.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Offensive Holding - 10 yard penalty from previous spot, replay down.
    /// Most common blocking penalty, occurs when offensive player illegally uses hands
    /// to restrain a defender. More frequent on pass plays due to longer engagement.
    ///
    /// NFL Rule 12-1-3: Use of Hands, Arms, and Body
    /// </summary>
    public class OffensiveHoldingPenalty : PenaltyBase
    {
        public override string Name => "Offensive Holding";

        public override string Description =>
            "Offensive player illegally used hands to restrain or pull a defender";

        // ==================== Occurrence ====================

        /// <summary>
        /// Base probability: 1.9% per play (most common offensive penalty)
        /// Source: NFL penalty data analysis
        /// </summary>
        public override double BaseOccurrenceProbability => 0.019;

        public override PenaltyTiming Timing => PenaltyTiming.DuringBlocking;

        /// <summary>
        /// Adjusts probability based on:
        /// - O-line skill (better blocking = fewer holds)
        /// - D-line pressure (better pass rush = more holds)
        /// - Play type (more common on pass plays)
        /// </summary>
        public override double CalculateOccurrenceProbability(PenaltyContext context)
        {
            var adjustmentFactor = 1.0;

            // Adjust based on offensive line skill
            if (context.OffensivePlayers != null && context.OffensivePlayers.Any())
            {
                var oLinemen = context.OffensivePlayers
                    .Where(p => p.Position == Positions.T ||
                                p.Position == Positions.G ||
                                p.Position == Positions.C ||
                                p.Position == Positions.TE ||
                                p.Position == Positions.RB)
                    .ToList();

                if (oLinemen.Any())
                {
                    var avgOLineSkill = oLinemen.Average(p => p.Blocking);
                    // Better O-line = fewer holding penalties (0.7x to 1.3x)
                    adjustmentFactor *= 1.3 - (avgOLineSkill / 100.0 * 0.6);
                }
            }

            // Adjust based on defensive pressure
            if (context.DefensivePlayers != null && context.DefensivePlayers.Any())
            {
                var dLinemen = context.DefensivePlayers
                    .Where(p => p.Position == Positions.DE || p.Position == Positions.DT)
                    .ToList();

                if (dLinemen.Any())
                {
                    var avgDLineSkill = dLinemen.Average(p => p.Tackling);
                    // Better pass rush = more holding (0.8x to 1.4x)
                    adjustmentFactor *= 0.8 + (avgDLineSkill / 100.0 * 0.6);
                }
            }

            // Play type adjustment - more holding on pass plays
            if (context.PlayType == PlayType.Pass)
            {
                adjustmentFactor *= 1.2;
            }
            else if (context.PlayType == PlayType.Run)
            {
                adjustmentFactor *= 1.0;  // Baseline for run plays
            }

            return BaseOccurrenceProbability * adjustmentFactor;
        }

        // ==================== Eligibility ====================

        public override TeamSide CommittedBy => TeamSide.Offense;

        /// <summary>
        /// Can be committed by offensive linemen, tight ends, and running backs.
        /// Wide receivers can also hold on running plays (downfield blocking).
        /// </summary>
        public override List<Positions> EligiblePositions => new List<Positions>
        {
            Positions.T,   // Tackles
            Positions.G,   // Guards
            Positions.C,   // Center
            Positions.TE,  // Tight Ends
            Positions.RB,  // Running Backs
            Positions.FB,  // Fullbacks
            Positions.WR   // Wide Receivers (downfield blocking)
        };

        // ==================== Enforcement ====================

        /// <summary>
        /// 10 yards from the previous spot (where ball was snapped)
        /// </summary>
        public override int Yards => 10;

        /// <summary>
        /// Replay the down (no loss of down)
        /// </summary>
        public override bool IsLossOfDown => false;

        /// <summary>
        /// Not a dead ball foul - occurs during the play
        /// </summary>
        public override bool IsDeadBallFoul => false;

        /// <summary>
        /// Enforced from previous spot, not spot of foul
        /// </summary>
        public override bool IsSpotFoul => false;
    }
}
