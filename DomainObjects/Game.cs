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

        public int TimeRemaining { get; set; }
        public Posession Posession { get; set; }

        //a game is created with 3600 seconds to go, 
        //and the first type of play is a kickoff
        public Game()
        {
            TimeRemaining = 3600;
            Posession = Posession.None;

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

    public enum Posession
    {
        Home,
        Away,
        None
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
