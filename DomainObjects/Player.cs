using System;
using System.Collections.Generic;
using static DomainObjects.StatTypes;

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
        public Dictionary<PlayerStatType, int> Stats { get; set; } = new();
        public Dictionary<PlayerStatType, int> SeasonStats { get; set; } = new();

        // Realistic attributes
        public int Speed { get; set; }           // 0-100
        public int Strength { get; set; }        // 0-100
        public int Agility { get; set; }         // 0-100
        public int Awareness { get; set; }       // 0-100
        public int InjuryRisk { get; set; }      // 0-100
        public int Morale { get; set; }          // 0-100
        public int Discipline { get; set; }      // 0-100 (higher = fewer penalties)

        // Position-specific skills
        public int Passing { get; set; }         // QB
        public int Catching { get; set; }        // WR, TE, RB
        public int Rushing { get; set; }         // RB, QB
        public int Blocking { get; set; }        // OL, TE, FB
        public int Tackling { get; set; }        // DL, LB, S, CB
        public int Coverage { get; set; }        // CB, S, LB
        public int Kicking { get; set; }         // K, P

        // Career stats
        public Dictionary<PlayerStatType, int> CareerStats { get; set; } = new();
        public bool IsRetired { get; set; }
        public int ContractYears { get; set; }
        public int Salary { get; set; }
        public int Potential { get; set; }       // 0-100
        public int Progression { get; set; }     // 0-100
        public int Health { get; set; }          // 0-100
    }
}