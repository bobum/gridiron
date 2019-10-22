using DomainObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class Game
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Play CurrentPlay { get; set; }

        public List<Play> Plays { get; set; } = new List<Play>();

        public int TimeRemaining =>
            Halves[0].Quarters[0].TimeRemaining +
            Halves[0].Quarters[1].TimeRemaining +
            Halves[1].Quarters[0].TimeRemaining +
            Halves[1].Quarters[1].TimeRemaining;

        public Possession Possession { get; set; }

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
            Possession = Possession.None;
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
