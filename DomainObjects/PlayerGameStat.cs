using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using static DomainObjects.StatTypes;

namespace DomainObjects
{
    public class PlayerGameStat : SoftDeletableEntity
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        // Store stats as JSON string for persistence
        public string StatsJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<PlayerStatType, int> Stats
        {
            get
            {
                if (string.IsNullOrEmpty(StatsJson))
                    return new Dictionary<PlayerStatType, int>();
                
                try 
                {
                    var stringDict = JsonSerializer.Deserialize<Dictionary<string, int>>(StatsJson);
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
                catch
                {
                    return new Dictionary<PlayerStatType, int>();
                }
            }
            set
            {
                var stringDict = new Dictionary<string, int>();
                foreach (var kvp in value)
                {
                    stringDict[kvp.Key.ToString()] = kvp.Value;
                }
                StatsJson = JsonSerializer.Serialize(stringDict);
            }
        }
    }
}
