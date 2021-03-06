﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class Play
    {
        public List<string> Result { get; set; } = new List<string>();
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

    }
}
