using System;
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
        public int StartTime { get; set; }
        public int StopTime { get; set; }
        public PlayType PlayType { get; set; }
        public List<Penalty> Penalties { get; set; } = new List<Penalty>();
        public Double ElapsedTime { get; set; } = 0.0;
    }
}
