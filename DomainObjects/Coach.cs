using System.Collections.Generic;
using static DomainObjects.StatTypes;

namespace DomainObjects
{
    public class Coach : Person
    {
        public string? Role { get; set; } // e.g., Head Coach, Offensive Coordinator, Defensive Coordinator, Special Teams Coordinator
        public int Age { get; set; }
        public int Experience { get; set; } // Years coaching
        public int Leadership { get; set; } // 0-100
        public int Strategy { get; set; } // 0-100
        public int Motivation { get; set; } // 0-100
        public int Adaptability { get; set; } // 0-100
        public Dictionary<CoachStatType, int> Stats { get; set; } = new();

        // Role-specific skills
        public int OffensiveSkill { get; set; } // Offensive roles
        public int DefensiveSkill { get; set; } // Defensive roles
        public int SpecialTeamsSkill { get; set; } // Special teams

        public int Reputation { get; set; } // 0-100
        public int ContractYears { get; set; }
        public int Salary { get; set; }
    }
}