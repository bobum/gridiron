using DomainObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Half = DomainObjects.Time.Half;

namespace DomainObjects
{
    public class Game
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Play CurrentPlay { get; set; }
        public Possession WonCoinToss { get; set; }
        public bool DeferredPossession { get; set; }

        public List<Play> Plays { get; set; } = new List<Play>();

        // Field position tracking (0 = offense's own goal line, 100 = opponent's goal line)
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
