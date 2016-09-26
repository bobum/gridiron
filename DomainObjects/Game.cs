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

        public Game()
        {
            TimeRemaining = 3600;
        }
    }

    public enum Positions
    {
        Quarterback,
        Center,
        OffensiveRightGuard,
        OffensiveLeftGuard,
        OffensiveRightTackle,
        OffensiveLeftTackle,
        TightEnd,
        WideReceiver,
        RunningBack,        
        DefensiveLeftTackle,
        DefensiveRightTackle,
        DefensiveLeftEnd,
        DefenseiveRightEnd,
        MiddleLinebacker,
        LeftOutsideLinebacker,
        RightOutsideLinebacker,
        Cornerback,
        Safety
    }

    public enum Downs
    {
        First,
        Second,
        Third,
        Fourth,
        None
    }
}
