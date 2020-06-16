using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects.Time
{
    public abstract class Half
    {
        public List<Quarter> Quarters { get; private set; }
        public int TimeRemaining => Quarters[0].TimeRemaining + Quarters[1].TimeRemaining;

        public HalfType HalfType { get; set; }

        protected Half(HalfType type)
        {
            HalfType = type;
            Quarters = new List<Quarter>
            {
                new Quarter(type == HalfType.First ? QuarterType.First : QuarterType.Third),
                new Quarter(type == HalfType.First ? QuarterType.Second : QuarterType.Fourth)
            };
        }
    }

    public enum HalfType
    {
        First,
        Second,
        GameOver
    }
}
