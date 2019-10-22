using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects.Time
{

    public class Quarter
    {
        private int timeRemaining;

        [RangeAttribute(0, 900, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int TimeRemaining
        {
            get => timeRemaining;
            set
            {
                if (value >= 900)
                {
                    timeRemaining = 900;
                    return;
                }

                if (value <= 0)
                {
                    timeRemaining = 0;
                    return;
                }

                timeRemaining = value;
            }
        }

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
