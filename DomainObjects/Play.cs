using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DomainObjects
{
    public class Play
    {
        public ILogger Result { get; set; } = NullLogger.Instance; // Logger for play-by-play output, defaults to NullLogger
        public Downs Down { get; set; }
        public bool GoodSnap { get; set; }
        public bool PossessionChange { get; set; } = false;
        public Possession Possession { get; set; } = Possession.None;
        public int StartTime { get; set; }
        public int StopTime { get; set; }
        public PlayType PlayType { get; set; }
        public List<Penalty> Penalties { get; set; } = new List<Penalty>();
        public List<Fumble> Fumbles { get; set; } = new List<Fumble>();
        public bool Interception { get; set; } = false;
        public Double ElapsedTime { get; set; } = 0.0;
        public bool QuarterExpired { get; set; } = false;
        public bool HalfExpired { get; set; } = false;
        public bool GameExpired { get; set; } = false;
        public List<Player> OffensePlayersOnField { get; set; } = new List<Player>();
        public List<Player> DefensePlayersOnField { get; set; } = new List<Player>();

        // Field position for this play
        public int StartFieldPosition { get; set; } = 0; // Where play started
        public int EndFieldPosition { get; set; } = 0; // Where play ended
        public int YardsGained { get; set; } = 0; // Net yards on the play
        public int YardsToGo { get; set; } = 10; // Yards needed for first down
        public bool IsFirstDown { get; set; } = false; // Did this play result in a first down?
        public bool IsTouchdown { get; set; } = false; // Did this play result in a touchdown?


    }
}
}
