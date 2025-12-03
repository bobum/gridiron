using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public static class StatTypes
    {
        public enum PlayerStatType
        {
            PassingYards,
            PassingAttempts,
            PassingCompletions,
            PassingTouchdowns,
            InterceptionsThrown,
            RushingYards,
            RushingAttempts,
            RushingTouchdowns,
            Fumbles,
            Receptions,
            ReceivingTargets,
            ReceivingYards,
            ReceivingTouchdowns,
            Tackles,
            Sacks,
            InterceptionsCaught,
            ForcedFumbles,
            FumbleRecoveries,
            FieldGoalsMade,
            FieldGoalsAttempted,
            ExtraPointsMade,
            ExtraPointsAttempted,
            Punts,
            PuntYards,
            KickoffReturns,
            KickoffReturnYards,
            PuntReturns,
            PuntReturnYards,
            GamesPlayed,
            GamesStarted,
            InterceptionReturnYards,
            FumblesLost,
            PuntsInside20
        }

        public enum TeamStatType
        {
            PointsScored,
            PointsAllowed,
            TotalYards,
            PassingYards,
            RushingYards,
            TurnoversCommitted,
            TurnoversForced,
            Penalties,
            PenaltyYards,
            ThirdDownConversions,
            ThirdDownAttempts,
            RedZoneAttempts,
            RedZoneTouchdowns,
            TimeOfPossessionSeconds,
            Wins,
            Losses,
            Ties
        }

        public enum CoachStatType
        {
            Wins,
            Losses,
            Ties,
            PlayoffAppearances,
            Championships,
            GamesCoached
        }

        public enum TrainerStatType
        {
            InjuriesTreated,
            AverageRecoveryTimeDays,
            SuccessfulRehabs,
            SeasonEndingInjuries,
            PlayerReturnRate
        }

        public enum ScoutStatType
        {
            PlayersScouted,
            DraftHits,
            DraftMisses,
            ProPlayersRecommended,
            CollegePlayersRecommended,
            ScoutingAccuracy
        }
    }
}
