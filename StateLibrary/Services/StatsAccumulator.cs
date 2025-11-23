using DomainObjects;
using System.Reflection;
using static DomainObjects.StatTypes;

namespace StateLibrary.Services
{
    public static class StatsAccumulator
    {
        private static Random _random = new Random();

        public static void AccumulatePassStats(PassPlay play)
        {
            // Check for sack (negative yards on pass play)
            bool isSack = play.YardsGained < 0;

            if (isSack)
            {
                // Sack stats handled in AccumulateDefensiveStats
            }
            else
            {
                if (play.PrimaryPasser != null)
                {
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingYards, play.YardsGained);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingTouchdowns, play.IsTouchdown ? 1 : 0);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.InterceptionsThrown, play.Interception ? 1 : 0);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingAttempts, 1);
                    UpdatePlayerStat(play.PrimaryPasser, PlayerStatType.PassingCompletions, play.IsComplete ? 1 : 0);
                }

                foreach (var segment in play.PassSegments)
                {
                    UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingTargets, 1);
                    if (segment.IsComplete)
                    {
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.Receptions, 1);
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingYards, segment.YardsGained);
                        // PassSegment doesn't have Touchdown property directly, infer from play result if this is the final segment
                        // Or check if segment ended in TD? PassSegment doesn't have IsTouchdown.
                        // We can check if the play was a TD and this is the final receiver.
                        bool isTouchdown = play.IsTouchdown && segment == play.PassSegments.Last();
                        UpdatePlayerStat(segment.Receiver, PlayerStatType.ReceivingTouchdowns, isTouchdown ? 1 : 0);
                    }
                }
            }
        }

        public static void AccumulateRunStats(RunPlay play)
        {
            foreach (var segment in play.RunSegments)
            {
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingYards, segment.YardsGained);
                // RunSegment likely doesn't have Touchdown property either.
                bool isTouchdown = play.IsTouchdown && segment == play.RunSegments.Last();
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingTouchdowns, isTouchdown ? 1 : 0);
                UpdatePlayerStat(segment.BallCarrier, PlayerStatType.RushingAttempts, 1);
            }
        }

        public static void AccumulateFieldGoalStats(FieldGoalPlay play)
        {
            // Defensive check - should never happen, but catches bugs early
            if (play.Kicker == null)
            {
                // Log or throw - this indicates a bug in play execution
                throw new InvalidOperationException("Field goal play missing kicker");
            }

            if (play.IsGood)
            {
                UpdatePlayerStat(play.Kicker, PlayerStatType.FieldGoalsMade, 1);
            }
            UpdatePlayerStat(play.Kicker, PlayerStatType.FieldGoalsAttempted, 1);
        }

        public static void AccumulatePuntStats(PuntPlay play)
        {
            // Defensive check - should never happen, but catches bugs early
            if (play.Punter == null)
            {
                throw new InvalidOperationException("Punt play missing kicker");
            }

            UpdatePlayerStat(play.Punter, PlayerStatType.Punts, 1);
            UpdatePlayerStat(play.Punter, PlayerStatType.PuntYards, play.PuntDistance);

            // Return stats
            if (play.InitialReturner != null)
            {
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.PuntReturns, 1);
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.PuntReturnYards, play.TotalReturnYards);
            }

            // Punts inside 20
            // If punt ends inside the 20 yard line (field position <= 20)
            // Note: Field position is relative to the offense. 
            // If kicking from own 20, end position 80 is opponent's 20.
            // Standard convention: 0-100 scale where 100 is opponent end zone.
            // So inside 20 means EndFieldPosition >= 80.
            if (play.EndFieldPosition >= 80)
            {
                UpdatePlayerStat(play.Punter, PlayerStatType.PuntsInside20, 1);
            }
        }

        public static void AccumulateKickoffStats(KickoffPlay play)
        {
            if (play.InitialReturner != null)
            {
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.KickoffReturns, 1);
                UpdatePlayerStat(play.InitialReturner, PlayerStatType.KickoffReturnYards, play.TotalReturnYards);
            }
        }

        public static void AccumulateFumbleStats(IPlay play)
        {
            ProcessFumbles(play.Fumbles);
        }

        private static void ProcessFumbles(List<Fumble> fumbles)
        {
            if (fumbles == null) return;

            foreach (var fumble in fumbles)
            {
                UpdatePlayerStat(fumble.FumbledBy, PlayerStatType.Fumbles, 1);
                if (fumble.RecoveredBy != null)
                {
                    UpdatePlayerStat(fumble.RecoveredBy, PlayerStatType.FumbleRecoveries, 1);
                }
            }
        }

        public static void AccumulateDefensiveStats(IPlay play)
        {
            var defensePlayers = play.DefensePlayersOnField;
            if (defensePlayers == null || !defensePlayers.Any()) return;

            var defenders = defensePlayers.Where(p => IsDefender(p)).ToList();
            if (!defenders.Any()) return;

            bool isSack = false;

            // Sacks
            if (play is PassPlay passPlay && passPlay.YardsGained < 0 && !passPlay.IsComplete)
            {
                isSack = true;
                var sacker = defenders[_random.Next(defenders.Count)];
                UpdatePlayerStat(sacker, PlayerStatType.Sacks, 1);
                UpdatePlayerStat(sacker, PlayerStatType.Tackles, 1); // Credit tackle too
            }

            // Generic Tackles
            // Only award generic tackle if it wasn't a sack AND play ended in a tackle (not TD, not incomplete)
            bool genericTackleOccurred = !play.IsTouchdown;

            if (play is PassPlay pp)
            {
                // If incomplete and not a sack, no tackle occurred
                if (!pp.IsComplete && !isSack)
                {
                    genericTackleOccurred = false;
                }
            }

            if (genericTackleOccurred && !isSack)
            {
                var tackler = defenders[_random.Next(defenders.Count)];
                UpdatePlayerStat(tackler, PlayerStatType.Tackles, 1);
            }

            // Interceptions
            if (play is PassPlay interceptionPlay && interceptionPlay.Interception && interceptionPlay.InterceptionDetails != null)
            {
                UpdatePlayerStat(interceptionPlay.InterceptionDetails.InterceptedBy, PlayerStatType.InterceptionsCaught, 1);
            }
        }

        private static void UpdatePlayerStat(Player player, PlayerStatType statType, int value)
        {
            if (player == null) return;

            if (!player.Stats.ContainsKey(statType))
            {
                player.Stats[statType] = 0;
            }
            player.Stats[statType] += value;
        }

        private static bool IsDefender(Player p)
        {
            return p.Position == Positions.DT ||
                   p.Position == Positions.DE ||
                   p.Position == Positions.LB ||
                   p.Position == Positions.CB ||
                   p.Position == Positions.S ||
                   p.Position == Positions.FS;
        }
    }
}
