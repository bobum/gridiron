using DomainObjects;
using GameManagement.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GameManagement.Services;

/// <summary>
/// Service for generating random players with realistic attributes
/// </summary>
public class PlayerGeneratorService : IPlayerGeneratorService
{
    private readonly ILogger<PlayerGeneratorService> _logger;
    private static List<string>? _firstNames;
    private static List<string>? _lastNames;
    private static List<string>? _colleges;
    private static readonly object _lock = new object();

    public PlayerGeneratorService(ILogger<PlayerGeneratorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoadNameData();
    }

    private void LoadNameData()
    {
        lock (_lock)
        {
            if (_firstNames == null)
            {
                try
                {
                    var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
                    
                    var firstNamesPath = Path.Combine(basePath, "FirstNames.json");
                    var lastNamesPath = Path.Combine(basePath, "LastNames.json");
                    var collegesPath = Path.Combine(basePath, "Colleges.json");

                    _firstNames = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(firstNamesPath)) ?? new List<string>();
                    _lastNames = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(lastNamesPath)) ?? new List<string>();
                    _colleges = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(collegesPath)) ?? new List<string>();

                    _logger.LogInformation("Loaded name data: {FirstNameCount} first names, {LastNameCount} last names, {CollegeCount} colleges",
                        _firstNames.Count, _lastNames.Count, _colleges.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load name data from JSON files");
                    // Fallback to basic names
                    _firstNames = new List<string> { "John", "Mike", "Tom", "James", "Robert" };
                    _lastNames = new List<string> { "Smith", "Johnson", "Williams", "Brown", "Jones" };
                    _colleges = new List<string> { "Alabama", "Ohio State", "Clemson", "Georgia", "Michigan" };
                }
            }
        }
    }

    public Player GenerateRandomPlayer(Positions position, int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        var player = new Player
        {
            FirstName = _firstNames![random.Next(_firstNames.Count)],
            LastName = _lastNames![random.Next(_lastNames.Count)],
            Position = position,
            College = _colleges![random.Next(_colleges.Count)],
            Number = GenerateJerseyNumber(position, random),
            Height = GenerateHeight(position, random),
            Weight = GenerateWeight(position, random),
            Age = random.Next(22, 31),  // Veterans: 22-30 years old
            Exp = random.Next(0, 12),   // 0-11 years experience
            ContractYears = random.Next(3, 6),  // 3-5 year contracts
            
            // General attributes (60-95 for veterans)
            Speed = GenerateAttributeForPosition(position, "Speed", random, 60, 95),
            Strength = GenerateAttributeForPosition(position, "Strength", random, 60, 95),
            Agility = GenerateAttributeForPosition(position, "Agility", random, 60, 95),
            Awareness = GenerateAttributeForPosition(position, "Awareness", random, 60, 95),
            
            // Position-specific skills
            Passing = GenerateAttributeForPosition(position, "Passing", random, 60, 95),
            Catching = GenerateAttributeForPosition(position, "Catching", random, 60, 95),
            Rushing = GenerateAttributeForPosition(position, "Rushing", random, 60, 95),
            Blocking = GenerateAttributeForPosition(position, "Blocking", random, 60, 95),
            Tackling = GenerateAttributeForPosition(position, "Tackling", random, 60, 95),
            Coverage = GenerateAttributeForPosition(position, "Coverage", random, 60, 95),
            Kicking = GenerateAttributeForPosition(position, "Kicking", random, 60, 95),
            
            // Default values
            Potential = random.Next(60, 90),
            Progression = random.Next(50, 85),
            Health = 100,
            Morale = random.Next(70, 100),
            Discipline = random.Next(60, 95),
            Fragility = random.Next(20, 60)
        };

        // Calculate overall and set salary
        int overall = OverallRatingCalculator.Calculate(player);
        player.Salary = OverallRatingCalculator.CalculateSalary(player, overall);

        return player;
    }

    public List<Player> GenerateDraftClass(int year, int rounds = 7)
    {
        var draftClass = new List<Player>();
        var random = new Random(year);  // Use year as seed for reproducibility
        int playersPerRound = 32;  // NFL has 32 teams
        int totalPicks = rounds * playersPerRound;

        _logger.LogInformation("Generating draft class for year {Year} with {Rounds} rounds ({TotalPicks} players)", 
            year, rounds, totalPicks);

        // Generate players across all positions
        var positions = new[]
        {
            Positions.QB, Positions.RB, Positions.FB, Positions.WR, Positions.TE,
            Positions.C, Positions.G, Positions.T,
            Positions.DE, Positions.DT, Positions.LB, Positions.OLB,
            Positions.CB, Positions.S, Positions.FS,
            Positions.K, Positions.P, Positions.LS
        };

        for (int i = 0; i < totalPicks; i++)
        {
            var position = positions[random.Next(positions.Length)];
            var player = GenerateDraftProspect(position, year, random);
            draftClass.Add(player);
        }

        _logger.LogInformation("Generated {Count} draft prospects", draftClass.Count);
        return draftClass;
    }

    public List<Player> GenerateMultiplePlayers(int count, int? seed = null)
    {
        var players = new List<Player>();
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        var positions = new[]
        {
            Positions.QB, Positions.RB, Positions.FB, Positions.WR, Positions.TE,
            Positions.C, Positions.G, Positions.T,
            Positions.DE, Positions.DT, Positions.LB, Positions.OLB,
            Positions.CB, Positions.S, Positions.FS,
            Positions.K, Positions.P, Positions.LS
        };

        for (int i = 0; i < count; i++)
        {
            var position = positions[random.Next(positions.Length)];
            var player = GenerateRandomPlayer(position, random.Next());
            players.Add(player);
        }

        _logger.LogInformation("Generated {Count} random players", players.Count);
        return players;
    }

    // Private helper methods

    private Player GenerateDraftProspect(Positions position, int year, Random random)
    {
        var player = new Player
        {
            FirstName = _firstNames![random.Next(_firstNames.Count)],
            LastName = _lastNames![random.Next(_lastNames.Count)],
            Position = position,
            College = _colleges![random.Next(_colleges.Count)],
            Number = GenerateJerseyNumber(position, random),
            Height = GenerateHeight(position, random),
            Weight = GenerateWeight(position, random),
            Age = random.Next(21, 24),  // Draft prospects: 21-23 years old
            Exp = 0,  // Rookies have no experience
            ContractYears = 0,  // Contract assigned at draft
            Salary = 0,  // Salary assigned at draft
            
            // Lower skills for rookies (40-85 range)
            Speed = GenerateAttributeForPosition(position, "Speed", random, 40, 85),
            Strength = GenerateAttributeForPosition(position, "Strength", random, 40, 85),
            Agility = GenerateAttributeForPosition(position, "Agility", random, 40, 85),
            Awareness = GenerateAttributeForPosition(position, "Awareness", random, 40, 85),
            
            // Position-specific skills
            Passing = GenerateAttributeForPosition(position, "Passing", random, 40, 85),
            Catching = GenerateAttributeForPosition(position, "Catching", random, 40, 85),
            Rushing = GenerateAttributeForPosition(position, "Rushing", random, 40, 85),
            Blocking = GenerateAttributeForPosition(position, "Blocking", random, 40, 85),
            Tackling = GenerateAttributeForPosition(position, "Tackling", random, 40, 85),
            Coverage = GenerateAttributeForPosition(position, "Coverage", random, 40, 85),
            Kicking = GenerateAttributeForPosition(position, "Kicking", random, 40, 85),
            
            // Higher potential for rookies
            Potential = random.Next(70, 99),
            Progression = random.Next(60, 95),
            Health = 100,
            Morale = random.Next(80, 100),
            Discipline = random.Next(50, 90),
            Fragility = random.Next(20, 60)
        };

        return player;
    }

    private int GenerateAttributeForPosition(Positions position, string attribute, Random random, int min, int max)
    {
        // Position-specific attribute ranges
        return (position, attribute) switch
        {
            // Quarterbacks
            (Positions.QB, "Passing") => random.Next(Math.Max(min, 70), max),
            (Positions.QB, "Awareness") => random.Next(Math.Max(min, 65), max),
            (Positions.QB, "Agility") => random.Next(min, Math.Min(max, 80)),
            (Positions.QB, "Speed") => random.Next(min, Math.Min(max, 75)),
            
            // Running Backs
            (Positions.RB, "Rushing") => random.Next(Math.Max(min, 70), max),
            (Positions.RB, "Speed") => random.Next(Math.Max(min, 70), max),
            (Positions.RB, "Agility") => random.Next(Math.Max(min, 70), max),
            (Positions.RB, "Catching") => random.Next(min, Math.Min(max, 85)),
            
            // Wide Receivers
            (Positions.WR, "Catching") => random.Next(Math.Max(min, 70), max),
            (Positions.WR, "Speed") => random.Next(Math.Max(min, 75), max),
            (Positions.WR, "Agility") => random.Next(Math.Max(min, 70), max),
            
            // Tight Ends
            (Positions.TE, "Catching") => random.Next(Math.Max(min, 65), max),
            (Positions.TE, "Blocking") => random.Next(Math.Max(min, 65), max),
            
            // Offensive Line
            (Positions.C or Positions.G or Positions.T, "Blocking") => random.Next(Math.Max(min, 70), max),
            (Positions.C or Positions.G or Positions.T, "Strength") => random.Next(Math.Max(min, 70), max),
            
            // Defensive Line
            (Positions.DE or Positions.DT, "Tackling") => random.Next(Math.Max(min, 70), max),
            (Positions.DE or Positions.DT, "Strength") => random.Next(Math.Max(min, 70), max),
            
            // Linebackers
            (Positions.LB or Positions.OLB, "Tackling") => random.Next(Math.Max(min, 70), max),
            (Positions.LB or Positions.OLB, "Coverage") => random.Next(Math.Max(min, 60), max),
            
            // Cornerbacks
            (Positions.CB, "Coverage") => random.Next(Math.Max(min, 75), max),
            (Positions.CB, "Speed") => random.Next(Math.Max(min, 75), max),
            
            // Safeties
            (Positions.S or Positions.FS, "Coverage") => random.Next(Math.Max(min, 70), max),
            (Positions.S or Positions.FS, "Tackling") => random.Next(Math.Max(min, 65), max),
            
            // Kickers/Punters
            (Positions.K or Positions.P, "Kicking") => random.Next(Math.Max(min, 70), max),
            
            // Default: use min-max range
            _ => random.Next(min, max)
        };
    }

    private int GenerateJerseyNumber(Positions position, Random random)
    {
        return position switch
        {
            Positions.QB => random.Next(1, 20),
            Positions.RB or Positions.FB => random.Next(20, 50),
            Positions.WR or Positions.TE => random.Next(10, 90),
            Positions.C or Positions.G or Positions.T => random.Next(50, 80),
            Positions.DE or Positions.DT or Positions.LB or Positions.OLB => random.Next(40, 99),
            Positions.CB or Positions.S or Positions.FS => random.Next(20, 50),
            Positions.K or Positions.P => random.Next(1, 20),
            Positions.LS => random.Next(40, 60),
            _ => random.Next(1, 99)
        };
    }

    private string GenerateHeight(Positions position, Random random)
    {
        var (minInches, maxInches) = position switch
        {
            Positions.QB => (72, 78),      // 6'0" - 6'6"
            Positions.RB => (68, 72),      // 5'8" - 6'0"
            Positions.WR => (70, 76),      // 5'10" - 6'4"
            Positions.TE => (74, 78),      // 6'2" - 6'6"
            Positions.T or Positions.G or Positions.C => (74, 80),  // 6'2" - 6'8"
            Positions.DE => (74, 78),      // 6'2" - 6'6"
            Positions.DT => (72, 76),      // 6'0" - 6'4"
            Positions.LB or Positions.OLB => (72, 76),  // 6'0" - 6'4"
            Positions.CB => (69, 73),      // 5'9" - 6'1"
            Positions.S or Positions.FS => (70, 74),    // 5'10" - 6'2"
            Positions.K or Positions.P => (70, 74),     // 5'10" - 6'2"
            _ => (70, 76)
        };

        int totalInches = random.Next(minInches, maxInches + 1);
        int feet = totalInches / 12;
        int inches = totalInches % 12;
        return $"{feet}-{inches}";
    }

    private int GenerateWeight(Positions position, Random random)
    {
        return position switch
        {
            Positions.QB => random.Next(210, 240),
            Positions.RB => random.Next(200, 230),
            Positions.FB => random.Next(230, 260),
            Positions.WR => random.Next(180, 220),
            Positions.TE => random.Next(240, 270),
            Positions.C or Positions.G => random.Next(290, 330),
            Positions.T => random.Next(300, 340),
            Positions.DE => random.Next(260, 290),
            Positions.DT => random.Next(290, 330),
            Positions.LB or Positions.OLB => random.Next(230, 260),
            Positions.CB => random.Next(180, 210),
            Positions.S or Positions.FS => random.Next(200, 220),
            Positions.K or Positions.P => random.Next(180, 210),
            Positions.LS => random.Next(240, 260),
            _ => random.Next(200, 250)
        };
    }
}
