using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.SkillsChecks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Handles the complete interception scenario including return, fumbles, and pick-six
    /// </summary>
    public class InterceptionSkillsCheckResult : SkillsCheckResult<InterceptionResult>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _qb;
        private readonly Player _intendedReceiver;
        private readonly List<Player> _offensePlayers;
        private readonly List<Player> _defensePlayers;
        private readonly int _interceptionSpot;

        public InterceptionSkillsCheckResult(
            ISeedableRandom rng,
            Player qb,
            Player intendedReceiver,
            List<Player> offensePlayers,
            List<Player> defensePlayers,
            int interceptionSpot)
        {
            _rng = rng;
            _qb = qb;
            _intendedReceiver = intendedReceiver;
            _offensePlayers = offensePlayers;
            _defensePlayers = defensePlayers;
            _interceptionSpot = interceptionSpot;
        }

        public override void Execute(Game game)
        {
            // Select interceptor (defensive back with best coverage/awareness/speed)
            var interceptor = _defensePlayers
                .Where(p => p.Position == Positions.CB || p.Position == Positions.S ||
                           p.Position == Positions.FS || p.Position == Positions.LB)
                .OrderByDescending(p => p.Coverage + p.Awareness + p.Speed)
                .FirstOrDefault();

            if (interceptor == null)
            {
                // Fallback - shouldn't happen
                interceptor = _defensePlayers.First();
            }

            // Calculate interception return yardage
            var returnYardsResult = new InterceptionReturnSkillsCheckResult(
                _rng,
                interceptor,
                _offensePlayers,
                _interceptionSpot);
            returnYardsResult.Execute(game);

            var returnInfo = returnYardsResult.Result;

            // Calculate final position after return
            // Interception return moves toward offense's 0 (opposite direction)
            var finalPosition = _interceptionSpot - returnInfo.ReturnYards;

            // Initialize result
            var result = new InterceptionResult
            {
                Interceptor = interceptor,
                ThrownBy = _qb,
                IntendedReceiver = _intendedReceiver,
                InterceptionSpot = _interceptionSpot,
                ReturnYards = returnInfo.ReturnYards,
                IsPickSix = false,
                FumbledDuringReturn = false,
                FumbleRecovery = null,
                FinalPosition = finalPosition
            };

            // Check for pick-six (touchdown)
            if (finalPosition <= 0)
            {
                result.IsPickSix = true;
                result.FinalPosition = 0;
            }
            else
            {
                // Check for fumble during interception return
                var fumbleCheck = new FumbleOccurredSkillsCheck(
                    _rng,
                    interceptor,
                    _offensePlayers,
                    PlayType.Pass,
                    isQBSack: false);
                fumbleCheck.Execute(game);

                if (fumbleCheck.Occurred)
                {
                    result.FumbledDuringReturn = true;

                    // Handle fumble recovery
                    var fumbleRecovery = new FumbleRecoverySkillsCheckResult(
                        _rng,
                        interceptor,
                        _offensePlayers,  // Now offense (trying to recover)
                        _defensePlayers,  // Now defense (interceptor's team trying to keep it)
                        finalPosition);
                    fumbleRecovery.Execute(game);

                    result.FumbleRecovery = fumbleRecovery.Result;

                    // Update final position based on fumble recovery
                    if (result.FumbleRecovery.OutOfBounds)
                    {
                        result.FinalPosition = result.FumbleRecovery.RecoverySpot;
                    }
                    else
                    {
                        // Calculate final position after fumble return
                        var fumbleReturnDirection = result.FumbleRecovery.RecoveredBy != null &&
                            _offensePlayers.Contains(result.FumbleRecovery.RecoveredBy)
                            ? 1  // Offense recovered, moving forward
                            : -1; // Defense kept it, moving toward offense's goal

                        result.FinalPosition = result.FumbleRecovery.RecoverySpot +
                            (fumbleReturnDirection * result.FumbleRecovery.ReturnYards);

                        // Clamp to field boundaries
                        result.FinalPosition = Math.Max(0, Math.Min(100, result.FinalPosition));

                        // Check if fumble return resulted in TD
                        if (fumbleReturnDirection < 0 && result.FinalPosition <= 0)
                        {
                            result.IsPickSix = true; // Fumble recovered and returned for TD
                            result.FinalPosition = 0;
                        }
                    }
                }
            }

            Result = result;
        }
    }

    /// <summary>
    /// Complete result of an interception play
    /// </summary>
    public class InterceptionResult
    {
        public Player Interceptor { get; set; }
        public Player ThrownBy { get; set; }
        public Player IntendedReceiver { get; set; }
        public int InterceptionSpot { get; set; }
        public int ReturnYards { get; set; }
        public bool IsPickSix { get; set; }
        public bool FumbledDuringReturn { get; set; }
        public FumbleRecoveryResult? FumbleRecovery { get; set; }
        public int FinalPosition { get; set; }

        /// <summary>
        /// Whether possession changed (always true unless fumble was recovered by offense)
        /// </summary>
        public bool PossessionChange
        {
            get
            {
                if (!FumbledDuringReturn)
                    return true; // Normal interception = possession change

                if (FumbleRecovery == null)
                    return true;

                // Fumbled during return - check who recovered
                // If offense recovered, they get the ball back (no net possession change)
                // If defense kept it, possession change stands
                // This requires checking if recoverer is on offense or defense
                // For now, return true (will be handled by caller with more context)
                return true;
            }
        }
    }
}
