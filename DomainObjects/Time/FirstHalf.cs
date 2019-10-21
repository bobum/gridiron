using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects.Time
{
    public class FirstHalf : Half
    {
        public FirstHalf() : base(HalfType.First)
        {
        }
    }
}
