using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class Play
    {
        public string Result { get; set; }
        public Downs Down { get; set; }

        public int StartTime { get; set; }
        public int StopTime { get; set; }
    }
}
