using System.Collections.Generic;
using static DomainObjects.StatTypes;

namespace DomainObjects
{
    public class Scout : Person
    {
        public string? Role { get; set; } // e.g., Director of Scouting, College Scout, Pro Scout

        public int Age { get; set; }

        public int Experience { get; set; } // Years scouting

        public int EvaluationSkill { get; set; } // 0-100

        public int NegotiationSkill { get; set; } // 0-100

        public int NetworkingSkill { get; set; } // 0-100

        public int PotentialRecognition { get; set; } // 0-100

        public int Reputation { get; set; } // 0-100

        public int ContractYears { get; set; }

        public int Salary { get; set; }

        public Dictionary<ScoutStatType, int> Stats { get; set; } = new ();
    }
}
