using DomainObjects;
using DomainObjects.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Defensive Holding - 5 yard penalty with automatic first down.
    /// Occurs when defensive player illegally restrains an offensive player
    /// who is not the ball carrier. More common on short/intermediate routes.
    ///
    /// NFL Rule 12-1-5: Contact with Eligible Receiver
    /// </summary>
    public class DefensiveHoldingPenalty : PenaltyBase
    {
        public override string Name => "Defensive Holding";

        public override string Description =>
            "Defensive player illegally restrained an offensive player who is not the ball carrier";

        // ==================== Occurrence ====================

        /// <summary>
        /// Base probability: 0.6% per play
        /// Source: NFL penalty data analysis
        /// </summary>
        public override double BaseOccurrenceProbability => 0.006;

        public override PenaltyTiming Timing => PenaltyTiming.DuringCoverage;

        /// <summary>
        /// Adjusts probability based on:
        /// - Defensive back coverage skill (worse coverage = more holds)
        /// - Receiver skill (better receivers = more penalties drawn)
        /// - Route depth (more common on short/intermediate routes)
        /// </summary>
        public override double CalculateOccurrenceProbability(PenaltyContext context)
        {
            var adjustmentFactor = 1.0;

            // Adjust based on defensive back coverage skills
            if (context.DefensivePlayers != null && context.DefensivePlayers.Any())
            {
                var defensiveBacks = context.DefensivePlayers
                    .Where(p => p.Position == Positions.CB ||
                                p.Position == Positions.S ||
                                p.Position == Positions.FS)
                    .ToList();

                if (defensiveBacks.Any())
                {
                    var avgCoverage = defensiveBacks.Average(p => p.Coverage);
                    // Worse coverage = more penalties (0.7x to 1.5x)
                    adjustmentFactor *= 1.5 - (avgCoverage / 100.0 * 0.8);
                }
            }

            // Adjust based on receiver skills (better receivers draw more penalties)
            if (context.OffensivePlayers != null && context.OffensivePlayers.Any())
            {
                var receivers = context.OffensivePlayers
                    .Where(p => p.Position == Positions.WR ||
                                p.Position == Positions.TE)
                    .ToList();

                if (receivers.Any())
                {
                    var avgCatching = receivers.Average(p => p.Catching);
                    // Better receivers = more penalties drawn (0.9x to 1.3x)
                    adjustmentFactor *= 0.9 + (avgCatching / 100.0 * 0.4);
                }
            }

            // More common on short/intermediate routes (under 10 yards)
            if (context.AirYards > 0 && context.AirYards < 10)
            {
                adjustmentFactor *= 1.5;
            }
            else if (context.AirYards >= 10 && context.AirYards < 20)
            {
                adjustmentFactor *= 1.2;
            }

            return BaseOccurrenceProbability * adjustmentFactor;
        }

        // ==================== Eligibility ====================

        public override TeamSide CommittedBy => TeamSide.Defense;

        /// <summary>
        /// Can be committed by defensive backs and linebackers during coverage.
        /// Defensive linemen generally don't commit holding (they commit other penalties).
        /// </summary>
        public override List<Positions> EligiblePositions => new List<Positions>
        {
            Positions.CB,   // Cornerbacks (most common)
            Positions.S,    // Safeties
            Positions.FS,   // Free Safeties
            Positions.LB,   // Linebackers
            Positions.OLB   // Outside Linebackers
        };

        // ==================== Enforcement ====================

        /// <summary>
        /// 5 yards from the previous spot
        /// Note: Less than offensive holding (10 yards)
        /// </summary>
        public override int Yards => 5;

        /// <summary>
        /// Automatic first down - key difference from offensive holding
        /// </summary>
        public override bool IsAutomaticFirstDown => true;

        /// <summary>
        /// Not a dead ball foul - occurs during pass coverage
        /// </summary>
        public override bool IsDeadBallFoul => false;

        /// <summary>
        /// Enforced from previous spot, not spot of foul
        /// </summary>
        public override bool IsSpotFoul => false;
    }
}
