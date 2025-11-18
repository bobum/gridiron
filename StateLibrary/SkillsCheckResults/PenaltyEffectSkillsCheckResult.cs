using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Determines the effect of a penalty that occurred.
    /// Takes a penalty context and determines who committed it, yards assessed, and acceptance.
    /// This separates "what happened" from "did it occur" - allows independent tuning of frequency vs effect.
    /// </summary>
    public class PenaltyEffectSkillsCheckResult : SkillsCheckResult<PenaltyResult>
    {
        private readonly ISeedableRandom _rng;
        private readonly PenaltyNames _penaltyName;
        private readonly PenaltyOccuredWhen _occurredWhen;
        private readonly List<Player> _homePlayersOnField;
        private readonly List<Player> _awayPlayersOnField;
        private readonly Possession _offense;
        private readonly int _currentFieldPosition;

        public PenaltyEffectSkillsCheckResult(
            ISeedableRandom rng,
            PenaltyNames penaltyName,
            PenaltyOccuredWhen occurredWhen,
            List<Player> homePlayersOnField,
            List<Player> awayPlayersOnField,
            Possession offense,
            int currentFieldPosition)
        {
            _rng = rng;
            _penaltyName = penaltyName;
            _occurredWhen = occurredWhen;
            _homePlayersOnField = homePlayersOnField;
            _awayPlayersOnField = awayPlayersOnField;
            _offense = offense;
            _currentFieldPosition = currentFieldPosition;
        }

        public override void Execute(Game game)
        {
            // Get the base penalty definition from the static list
            var penaltyDef = Penalties.List.SingleOrDefault(p => p.Name == _penaltyName);
            if (penaltyDef == null)
            {
                // Should never happen, but fallback
                Result = null;
                return;
            }

            // Determine which team committed the penalty based on historical odds
            var teamRoll = _rng.NextDouble();
            var calledOn = teamRoll <= penaltyDef.AwayOdds ? Possession.Away : Possession.Home;

            // Select player who committed the penalty from players on field
            var eligiblePlayers = calledOn == Possession.Home
                ? _homePlayersOnField
                : _awayPlayersOnField;

            // If no eligible players (empty list), penalty doesn't occur
            if (eligiblePlayers == null || eligiblePlayers.Count == 0)
            {
                // IMPORTANT: Consume Next() even when failing to maintain consistent random value consumption
                // This ensures tests with queue-based RNG work correctly
                _rng.Next(1);  // Dummy call to maintain consumption pattern
                Result = null;
                return;
            }

            // Select player weighted by discipline (lower discipline = more likely to commit penalty)
            var committedBy = SelectPlayerByDiscipline(eligiblePlayers);

            // Determine yards from penalty definition
            // Most penalties have standard yardage, some are context-dependent
            var yards = DeterminePenaltyYards(_penaltyName, _currentFieldPosition);

            // Determine if penalty is accepted
            // Generally accepted unless it helps the offense more to decline
            var accepted = DetermineAcceptance(calledOn, yards);

            Result = new PenaltyResult
            {
                PenaltyName = _penaltyName,
                CalledOn = calledOn,
                CommittedBy = committedBy,
                OccurredWhen = _occurredWhen,
                Yards = yards,
                Accepted = accepted
            };
        }

        /// <summary>
        /// Selects a player from the eligible list, weighted by discipline.
        /// Players with lower discipline are more likely to commit penalties.
        /// </summary>
        private Player SelectPlayerByDiscipline(List<Player> eligiblePlayers)
        {
            // If only one player, return them
            if (eligiblePlayers.Count == 1)
            {
                return eligiblePlayers[0];
            }

            // Calculate penalty weights for each player (lower discipline = higher weight)
            // Weight = (100 - Discipline) + 20 (to ensure minimum weight)
            // This means discipline 0 = weight 120, discipline 100 = weight 20
            var weights = new List<double>();
            double totalWeight = 0;

            foreach (var player in eligiblePlayers)
            {
                // If discipline is 0 (not initialized), use default weight
                var discipline = player.Discipline > 0 ? player.Discipline : 70;
                var weight = (100 - discipline) + 20; // Inverse relationship
                weights.Add(weight);
                totalWeight += weight;
            }

            // Select player using weighted random selection
            var roll = _rng.NextDouble() * totalWeight;
            var cumulativeWeight = 0.0;

            for (int i = 0; i < eligiblePlayers.Count; i++)
            {
                cumulativeWeight += weights[i];
                if (roll < cumulativeWeight)
                {
                    return eligiblePlayers[i];
                }
            }

            // Fallback (should never reach here)
            return eligiblePlayers[eligiblePlayers.Count - 1];
        }

        private int DeterminePenaltyYards(PenaltyNames penaltyName, int fieldPosition)
        {
            // Standard penalty yardages based on NFL rules
            switch (penaltyName)
            {
                // 5-yard penalties
                case PenaltyNames.FalseStart:
                case PenaltyNames.DelayofGame:
                case PenaltyNames.Encroachment:
                case PenaltyNames.DefensiveOffside:
                case PenaltyNames.NeutralZoneInfraction:
                case PenaltyNames.IllegalFormation:
                case PenaltyNames.IllegalShift:
                case PenaltyNames.IllegalMotion:
                case PenaltyNames.Offensive12OnField:
                case PenaltyNames.Defensive12OnField:
                case PenaltyNames.IllegalSubstitution:
                case PenaltyNames.RunningIntotheKicker:
                case PenaltyNames.DefensiveDelayofGame:
                case PenaltyNames.OffensiveOffside:
                    return 5;

                // 10-yard penalties
                case PenaltyNames.OffensiveHolding:
                case PenaltyNames.DefensiveHolding:
                case PenaltyNames.IllegalUseofHands:
                case PenaltyNames.IllegalBlockAbovetheWaist:
                case PenaltyNames.OffensivePassInterference:
                case PenaltyNames.IllegalContact:
                case PenaltyNames.Clipping:
                case PenaltyNames.Tripping:
                case PenaltyNames.IneligibleDownfieldPass:
                case PenaltyNames.IneligibleDownfieldKick:
                case PenaltyNames.IllegalForwardPass:
                case PenaltyNames.IntentionalGrounding:
                case PenaltyNames.IllegalBlindsideBlock:
                case PenaltyNames.ChopBlock:
                case PenaltyNames.LowBlock:
                case PenaltyNames.IllegalPeelback:
                case PenaltyNames.IllegalCrackback:
                case PenaltyNames.IllegalTouchPass:
                case PenaltyNames.IllegalTouchKick:
                case PenaltyNames.InvalidFairCatchSignal:
                    return 10;

                // 15-yard penalties
                case PenaltyNames.UnnecessaryRoughness:
                case PenaltyNames.FaceMask15Yards:
                case PenaltyNames.RoughingthePasser:
                case PenaltyNames.RoughingtheKicker:
                case PenaltyNames.UnsportsmanlikeConduct:
                case PenaltyNames.Taunting:
                case PenaltyNames.HorseCollarTackle:
                case PenaltyNames.PersonalFoul:
                case PenaltyNames.PlayerOutofBoundsonPunt:
                case PenaltyNames.FairCatchInterference:
                case PenaltyNames.Leaping:
                case PenaltyNames.Leverage:
                case PenaltyNames.InterferencewithOpportunitytoCatch:
                    return 15;

                // Spot foul (defensive pass interference)
                case PenaltyNames.DefensivePassInterference:
                    // Spot of the foul - for now return variable yardage
                    // TODO: Calculate actual spot based on air yards
                    return (int)(10 + _rng.NextDouble() * 25); // 10-35 yards

                // Ejection (also 15 yards)
                case PenaltyNames.Disqualification:
                    return 15;

                default:
                    return 10; // Default fallback
            }
        }

        private bool DetermineAcceptance(Possession calledOn, int yards)
        {
            // For now, always accept penalties
            // TODO: Implement smart acceptance logic based on:
            // - Down and distance
            // - Field position
            // - Which team benefited from the play
            // - Whether penalty helps or hurts more than the play result
            return true;
        }
    }

    /// <summary>
    /// Result of a penalty occurrence, containing all details about what happened
    /// </summary>
    public class PenaltyResult
    {
        /// <summary>
        /// The specific penalty that was called
        /// </summary>
        public PenaltyNames PenaltyName { get; set; }

        /// <summary>
        /// Which team committed the penalty
        /// </summary>
        public Possession CalledOn { get; set; }

        /// <summary>
        /// The player who committed the penalty
        /// </summary>
        public Player CommittedBy { get; set; }

        /// <summary>
        /// When during the play the penalty occurred
        /// </summary>
        public PenaltyOccuredWhen OccurredWhen { get; set; }

        /// <summary>
        /// Yardage assessment for the penalty
        /// </summary>
        public int Yards { get; set; }

        /// <summary>
        /// Whether the penalty was accepted or declined
        /// </summary>
        public bool Accepted { get; set; }
    }
}
