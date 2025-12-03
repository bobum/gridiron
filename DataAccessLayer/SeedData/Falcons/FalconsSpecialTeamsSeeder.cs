using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons special teams (K, P, LS).
    /// </summary>
    public static class FalconsSpecialTeamsSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                // Kicker
                new Player
                {
                    TeamId = teamId,
                    Number = 3,
                    LastName = "Bryant",
                    FirstName = "Matt",
                    Position = Positions.K,
                    Height = "5-9",
                    Weight = 203,
                    Age = 41,
                    Exp = 15,
                    College = "Baylor",
                    Speed = 60,
                    Strength = 65,
                    Agility = 70,
                    Awareness = 85,
                    Fragility = 10,
                    Morale = 95,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 15,
                    Blocking = 20,
                    Tackling = 20,
                    Coverage = 10,
                    Kicking = 95,
                    Potential = 80,
                    Progression = 75,
                    Health = 95,
                    Discipline = 100
                },

                // Punter
                new Player
                {
                    TeamId = teamId,
                    Number = 5,
                    LastName = "Bosher",
                    FirstName = "Matt",
                    Position = Positions.P,
                    Height = "6-0",
                    Weight = 208,
                    Age = 28,
                    Exp = 6,
                    College = "Miami (Fla.)",
                    Speed = 65,
                    Strength = 60,
                    Agility = 70,
                    Awareness = 80,
                    Fragility = 12,
                    Morale = 90,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 15,
                    Blocking = 20,
                    Tackling = 20,
                    Coverage = 10,
                    Kicking = 92,
                    Potential = 85,
                    Progression = 80,
                    Health = 95,
                    Discipline = 99
                },

                // Long Snapper
                new Player
                {
                    TeamId = teamId,
                    Number = 47,
                    LastName = "Harris",
                    FirstName = "Josh",
                    Position = Positions.LS,
                    Height = "6-1",
                    Weight = 224,
                    Age = 27,
                    Exp = 5,
                    College = "Auburn",
                    Speed = 60,
                    Strength = 80,
                    Agility = 70,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 80,
                    Tackling = 30,
                    Coverage = 20,
                    Kicking = 10,
                    Potential = 80,
                    Progression = 75,
                    Health = 90,
                    Discipline = 83
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} Special Teams (K/P/LS)");
        }
    }
}
