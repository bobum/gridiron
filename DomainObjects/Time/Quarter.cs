using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects.Time
{

    public class Quarter
    {
        public int TimeRemaining { get; set; }

        public QuarterType QuarterType { get; private set; }

        public Quarter(QuarterType type)
        {
            QuarterType = type;
            TimeRemaining = 900;
        }
    }

    public enum QuarterType
    {
        First,
        Second,
        Third,
        Fourth,
        Overtime
    }
}
