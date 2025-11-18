using DomainObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DomainObjects.Helpers;
using Half = DomainObjects.Time.Half;

namespace DomainObjects
{
    public class Game
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public IPlay CurrentPlay { get; set; }
        public Possession WonCoinToss { get; set; }
        public bool DeferredPossession { get; set; }

        public List<IPlay> Plays { get; set; } = new List<IPlay>();

        // Logger for game events - will be assigned to Play.Result for play-by-play logging
        // Defaults to NullLogger so tests don't need to set it up
        public ILogger Logger { get; set; } = NullLogger.Instance;

        // Field position tracking (absolute position 0-100 on field, does not flip on possession changes)
        // Use FormatFieldPosition(possession) to display in NFL notation (e.g., "Team 25 yard line")
        public int FieldPosition { get; set; } = 0; // Line of scrimmage
        public int YardsToGo { get; set; } = 10; // Yards needed for first down
        public Downs CurrentDown { get; set; } = Downs.First;

        // Score tracking
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;

        public int TimeRemaining =>
            Halves[0].Quarters[0].TimeRemaining +
            Halves[0].Quarters[1].TimeRemaining +
            Halves[1].Quarters[0].TimeRemaining +
            Halves[1].Quarters[1].TimeRemaining;

        public List<Half> Halves { get; } = new List<Half>() {
            new FirstHalf(),
            new SecondHalf()
        };

        public Quarter CurrentQuarter { get; set; }
        public Half CurrentHalf { get; set; }

        //a game is created with 3600 seconds to go,
        //and the first type of play is a kickoff
        public Game()
        {
            CurrentQuarter = Halves[0].Quarters[0];
            CurrentHalf = Halves[0];
        }

        // ========================================
        // FIELD POSITION HELPERS
        // ========================================

        /// <summary>
        /// Gets the offensive team (team with current possession)
        /// </summary>
        public Team GetOffensiveTeam(Possession possession)
        {
            return possession == Possession.Home ? HomeTeam : AwayTeam;
        }

        /// <summary>
        /// Gets the defensive team (team without possession)
        /// </summary>
        public Team GetDefensiveTeam(Possession possession)
        {
            return possession == Possession.Home ? AwayTeam : HomeTeam;
        }

        /// <summary>
        /// Formats current field position in NFL notation
        /// </summary>
        public string FormatFieldPosition(Possession possession)
        {
            var offenseTeam = GetOffensiveTeam(possession);
            var defenseTeam = GetDefensiveTeam(possession);
            return FieldPositionHelper.FormatFieldPosition(FieldPosition, offenseTeam, defenseTeam);
        }

        /// <summary>
        /// Formats a specific field position in NFL notation
        /// </summary>
        public string FormatFieldPosition(int fieldPosition, Possession possession)
        {
            var offenseTeam = GetOffensiveTeam(possession);
            var defenseTeam = GetDefensiveTeam(possession);
            return FieldPositionHelper.FormatFieldPosition(fieldPosition, offenseTeam, defenseTeam);
        }

        /// <summary>
        /// Formats current field position with "yard line" suffix
        /// </summary>
        public string FormatFieldPositionWithYardLine(Possession possession)
        {
            var offenseTeam = GetOffensiveTeam(possession);
            var defenseTeam = GetDefensiveTeam(possession);
            return FieldPositionHelper.FormatFieldPositionWithYardLine(FieldPosition, offenseTeam, defenseTeam);
        }

        // ========================================
        // SCORING METHODS
        // ========================================

        /// <summary>
        /// Adds a touchdown (6 points) to the scoring team
        /// </summary>
        /// <param name="scoringTeam">The team that scored the touchdown</param>
        public void AddTouchdown(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 6;
                Logger.LogInformation($"TOUCHDOWN! Home team scores! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 6;
                Logger.LogInformation($"TOUCHDOWN! Away team scores! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a field goal (3 points) to the scoring team
        /// </summary>
        /// <param name="scoringTeam">The team that kicked the field goal</param>
        public void AddFieldGoal(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 3;
                Logger.LogInformation($"FIELD GOAL is good! Home team scores! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 3;
                Logger.LogInformation($"FIELD GOAL is good! Away team scores! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a safety (2 points) to the defending team.
        /// NOTE: The defending team (the team that got the safety) receives the points.
        /// </summary>
        /// <param name="defendingTeam">The team that forced the safety (receives the 2 points)</param>
        public void AddSafety(Possession defendingTeam)
        {
            if (defendingTeam == Possession.Home)
            {
                HomeScore += 2;
                Logger.LogInformation($"SAFETY! Home team gets 2 points! Home {HomeScore}, Away {AwayScore}");
            }
            else if (defendingTeam == Possession.Away)
            {
                AwayScore += 2;
                Logger.LogInformation($"SAFETY! Away team gets 2 points! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds an extra point (1 point) to the scoring team after a touchdown
        /// </summary>
        /// <param name="scoringTeam">The team that kicked the extra point</param>
        public void AddExtraPoint(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 1;
                Logger.LogInformation($"Extra point is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 1;
                Logger.LogInformation($"Extra point is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
        }

        /// <summary>
        /// Adds a two-point conversion (2 points) to the scoring team after a touchdown
        /// </summary>
        /// <param name="scoringTeam">The team that converted the two-point attempt</param>
        public void AddTwoPointConversion(Possession scoringTeam)
        {
            if (scoringTeam == Possession.Home)
            {
                HomeScore += 2;
                Logger.LogInformation($"Two-point conversion is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
            else if (scoringTeam == Possession.Away)
            {
                AwayScore += 2;
                Logger.LogInformation($"Two-point conversion is GOOD! Home {HomeScore}, Away {AwayScore}");
            }
        }
    }

    public enum Positions
    {
        QB,//Quarterback,
        C,//Center,
        G,//OffensiveRightGuard,
        //OffensiveLeftGuard,
        T,//OffensiveRightTackle,
        //OffensiveLeftTackle,
        TE,//TightEnd,
        WR,//WideReceiver,
        RB,//RunningBack,        
        DT,//DefensiveLeftTackle,
        //DefensiveRightTackle,
        DE,//DefensiveLeftEnd,
        //DefenseiveRightEnd,
        LB,//MiddleLinebacker,
        OLB,//LeftOutsideLinebacker,
        //RightOutsideLinebacker,
        CB,//Cornerback,
        S,//Safety
        K,//Kicker,
        P,//Punter,
        FB,//Fullback,
        FS,//FreeSafety,
        LS,//LongSnapper
        H,//Holder
    }

    public enum Downs
    {
        First,
        Second,
        Third,
        Fourth,
        None
    }

    public enum Possession
    {
        None,
        Home,
        Away
    }

    public enum PlayType
    {
        Kickoff,
        FieldGoal,
        Punt,
        Pass,
        Run
    }
}
