using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainObjects
{
    public class Person : SoftDeletableEntity
    {
        private static int _personCounter = 0;

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Person()
        {
            // Auto-generate default names to ensure Person is always in valid state
            // Production code should override with actual names
            var id = System.Threading.Interlocked.Increment(ref _personCounter);
            FirstName = "Person";
            LastName = $"{id}";
        }
    }
}
