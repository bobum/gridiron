using DomainObjects;
using DomainObjects.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Penalties
{
    /// <summary>
    /// Defensive Pass Interference - Spot foul with automatic first down.
    /// Most impactful penalty in football - can result in 40+ yard penalties.
    /// Occurs when defensive player illegally interferes with receiver's opportunity to catch.
    ///
    /// Special Rules:
    /// - Spot foul (enforced from where interference occurred)
    /// - If in end zone, ball placed at 1-yard line
    /// - Automatic first down
    /// - Much more likely on deep passes where DB is beaten
    ///
    /// NFL Rule 8-5-1: Pass Interference
    /// </summary>
    public class DefensivePassInterferencePenalty : PenaltyBase
    {
        public override string Name => "Defensive Pass Interference";

        public override string Description =>
            "Defensive player illegally interfered with receiver's opportunity to catch a pass";

        // ==================== Occurrence ====================

        /// <summary>
        /// Base probability: 0.64% per pass play
        /// Source: NFL penalty data analysis
        /// Note: This is per pass attempt, not per play
        /// </summary>
        public override double BaseOccurrenceProbability => 0.0064;

        public override PenaltyTiming Timing => PenaltyTiming.DuringCoverage;

        /// <summary>
        /// DPI probability varies dramatically based on:
        /// - Pass depth (MUCH more likely on deep balls where DB is beaten)
        /// - Pass completion (more likely on incompletions)
        /// - Coverage skill (worse coverage = more DPI)
        /// - Receiver skill (better receivers draw more DPI)
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
                    // Worse coverage = more DPI (0.7x to 1.5x)
                    adjustmentFactor *= 1.5 - (avgCoverage / 100.0 * 0.8);
                }
            }

            // Adjust based on receiver skills
            if (context.OffensivePlayers != null && context.OffensivePlayers.Any())
            {
                var receivers = context.OffensivePlayers
                    .Where(p => p.Position == Positions.WR ||
                                p.Position == Positions.TE)
                    .ToList();

                if (receivers.Any())
                {
                    var avgCatching = receivers.Average(p => p.Catching);
                    // Better receivers = more DPI drawn (0.9x to 1.3x)
                    adjustmentFactor *= 0.9 + (avgCatching / 100.0 * 0.4);
                }
            }

            // CRITICAL: Deep passes have MUCH higher DPI rates
            // This is the key differentiator for DPI
            if (context.AirYards > 30)
            {
                adjustmentFactor *= 3.0;  // Deep balls are DPI magnets
            }
            else if (context.AirYards > 20)
            {
                adjustmentFactor *= 2.5;
            }
            else if (context.AirYards > 15)
            {
                adjustmentFactor *= 2.0;
            }
            else if (context.AirYards > 10)
            {
                adjustmentFactor *= 1.2;
            }

            // DPI is rare on completions (would be defensive holding instead)
            // But much more common on incomplete deep passes
            if (!context.PassCompleted && context.AirYards > 15)
            {
                adjustmentFactor *= 2.5;
            }
            else if (context.PassCompleted)
            {
                adjustmentFactor *= 0.1;  // Very rare on completions
            }

            return BaseOccurrenceProbability * adjustmentFactor;
        }

        // ==================== Eligibility ====================

        public override TeamSide CommittedBy => TeamSide.Defense;

        /// <summary>
        /// Can be committed by defensive backs covering receivers.
        /// Linebackers can also commit DPI when covering tight ends/RBs.
        /// </summary>
        public override List<Positions> EligiblePositions => new List<Positions>
        {
            Positions.CB,   // Cornerbacks (most common)
            Positions.S,    // Safeties
            Positions.FS,   // Free Safeties
            Positions.LB,   // Linebackers (when in coverage)
            Positions.OLB   // Outside Linebackers
        };

        // ==================== Enforcement ====================

        /// <summary>
        /// Spot foul - yardage varies based on where interference occurred.
        /// This value is not used; CalculateYardage determines actual penalty.
        /// </summary>
        public override int Yards => 15;  // Placeholder - actual yardage calculated dynamically

        /// <summary>
        /// Calculates yardage based on where the foul occurred (spot foul).
        /// Special rule: If in end zone, ball placed at 1-yard line.
        /// </summary>
        public override int CalculateYardage(PenaltyEnforcementContext context)
        {
            // If interference in the end zone, ball goes to the 1-yard line
            if (context.InEndZone)
            {
                // Calculate distance to 1-yard line
                // Assuming FieldPosition 0-100 where 100 is opponent's goal
                return Math.Max(1, 100 - context.FieldPosition - 1);
            }

            // Otherwise, it's enforced from the spot of the foul
            // The spot is passed in via SpotOfFoul
            return context.SpotOfFoul - context.FieldPosition;
        }

        /// <summary>
        /// Spot foul - one of only a few penalties enforced from spot of foul
        /// </summary>
        public override bool IsSpotFoul => true;

        /// <summary>
        /// Automatic first down - always
        /// This is a massive penalty, can turn 3rd and 20 into 1st and 10
        /// </summary>
        public override bool IsAutomaticFirstDown => true;

        /// <summary>
        /// Not a dead ball foul - occurs during the pass
        /// </summary>
        public override bool IsDeadBallFoul => false;

        /// <summary>
        /// DPI is almost always accepted.
        /// Only declined if play result was better (extremely rare - like a TD).
        /// </summary>
        public override bool ShouldAccept(PenaltyAcceptanceContext context)
        {
            // If play resulted in a touchdown, decline the penalty
            // Otherwise always accept (spot foul + automatic first down is huge)
            if (context.YardsGainedOnPlay >= context.YardsToGo + 50)
            {
                // Likely a touchdown - decline
                return false;
            }

            // Accept in virtually all other cases
            return true;
        }
    }
}
