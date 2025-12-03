using System.Collections.Generic;
using static DomainObjects.StatTypes;

namespace DomainObjects
{
    public class Trainer : Person
    {
        public string? Role { get; set; } // e.g., Head Athletic Trainer, Assistant Trainer, Team Doctor, Physical Therapist

        public int Age { get; set; }

        public int Experience { get; set; } // Years in profession

        public int MedicalSkill { get; set; } // 0-100

        public int RehabSkill { get; set; } // 0-100

        public int PreventionSkill { get; set; } // 0-100

        public int NutritionSkill { get; set; } // 0-100

        public int Reputation { get; set; } // 0-100

        public int ContractYears { get; set; }

        public int Salary { get; set; }

        public Dictionary<TrainerStatType, int> Stats { get; set; } = new ();
    }
}
