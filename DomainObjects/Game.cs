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

        public List<Play> Plays { get; set; }

        public int TimeRemaining { get; set; }
        public Posession Posession { get; set; }

        public Game()
        {
            TimeRemaining = 3600;
            Posession = Posession.Home;
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
        Away
    }
}
