using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class Player : Person
    {
        public Positions Position { get; set; }
        public int Number { get; set; }
        public string Height { get; set; }
        public int Weight { get; set; }
        public int Age { get; set; }
        public int Exp { get; set; }
        public string College { get; set; }
    }
}
