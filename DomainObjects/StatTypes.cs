using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class StatTypes
    {
        public enum PlayerStatType
        {
            PassingYards,
            PassingTouchdowns,
            InterceptionsThrown,
            RushingYards,
            RushingTouchdowns,
            Fumbles,
            Receptions,
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
            GamesStarted
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
