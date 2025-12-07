using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using static DomainObjects.StatTypes;

namespace DomainObjects
{
    public class Player : Person
    {
        public int Id { get; set; } // Primary key for EF Core

        public int? TeamId { get; set; } // Foreign key to Team (nullable - player might not be on a team)

        public Positions Position { get; set; }

        public int Number { get; set; }

        public string? Height { get; set; }

        public int Weight { get; set; }

        public int Age { get; set; }

        public int Exp { get; set; }

        public string? College { get; set; }

        // JSON backing fields
        public string StatsJson { get; set; } = "{}";
        public string SeasonStatsJson { get; set; } = "{}";
        public string CareerStatsJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<PlayerStatType, int> Stats 
        { 
            get => DeserializeStats(StatsJson);
            set => StatsJson = SerializeStats(value);
        }

        [NotMapped]
        public Dictionary<PlayerStatType, int> SeasonStats 
        { 
            get => DeserializeStats(SeasonStatsJson);
            set => SeasonStatsJson = SerializeStats(value);
        }

        // Realistic attributes
        public int Speed { get; set; } // 0-100

        public int Strength { get; set; } // 0-100

        public int Agility { get; set; } // 0-100

        public int Awareness { get; set; } // 0-100

        public int Fragility { get; set; } = 50; // 0-100 (higher = more injury-prone), defaults to 50

        public int Morale { get; set; } // 0-100

        public int Discipline { get; set; } // 0-100 (higher = fewer penalties)

        // Position-specific skills
        public int Passing { get; set; } // QB

        public int Catching { get; set; } // WR, TE, RB

        public int Rushing { get; set; } // RB, QB

        public int Blocking { get; set; } // OL, TE, FB

        public int Tackling { get; set; } // DL, LB, S, CB

        public int Coverage { get; set; } // CB, S, LB

        public int Kicking { get; set; } // K, P

        // Career stats
        [NotMapped]
        public Dictionary<PlayerStatType, int> CareerStats 
        { 
            get => DeserializeStats(CareerStatsJson);
            set => CareerStatsJson = SerializeStats(value);
        }

        public bool IsRetired { get; set; }

        public int ContractYears { get; set; }

        public int Salary { get; set; }

        public int Potential { get; set; } // 0-100

        public int Progression { get; set; } // 0-100

        public int Health { get; set; } // 0-100

        // Injury tracking

        /// <summary>
        /// Gets or sets current active injury for this player (null if not injured).
        /// </summary>
        public Injury? CurrentInjury { get; set; }

        /// <summary>
        /// Gets a value indicating whether whether the player is currently injured and unavailable.
        /// </summary>
        public bool IsInjured => CurrentInjury != null;

        private Dictionary<PlayerStatType, int> DeserializeStats(string json)
        {
            if (string.IsNullOrEmpty(json)) return new Dictionary<PlayerStatType, int>();
            try 
            {
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                var result = new Dictionary<PlayerStatType, int>();
                if (stringDict != null)
                {
                    foreach (var kvp in stringDict)
                    {
                        if (Enum.TryParse<PlayerStatType>(kvp.Key, out var statType))
                        {
                            result[statType] = kvp.Value;
                        }
                    }
                }
                return result;
            }
            catch { return new Dictionary<PlayerStatType, int>(); }
        }

        private string SerializeStats(Dictionary<PlayerStatType, int> stats)
        {
            if (stats == null) return "{}";
            var stringDict = new Dictionary<string, int>();
            foreach (var kvp in stats)
            {
                stringDict[kvp.Key.ToString()] = kvp.Value;
            }
            return JsonSerializer.Serialize(stringDict);
        }
    }
}
