using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons quarterbacks.
    /// </summary>
    public static class FalconsQBSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 2,
                    LastName = "Ryan",
                    FirstName = "Matt",
                    Position = Positions.QB,
                    Height = "6-4",
                    Weight = 217,
                    Age = 31,
                    Exp = 9,
                    College = "Boston College",
                    Speed = 78,
                    Strength = 70,
                    Agility = 85,
                    Awareness = 90,
                    Fragility = 15,
                    Morale = 95,
                    Passing = 92,
                    Catching = 35,
                    Rushing = 60,
                    Blocking = 30,
                    Tackling = 25,
                    Coverage = 20,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 80,
                    Health = 95,
                    Discipline = 95
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 8,
                    LastName = "Schaub",
                    FirstName = "Matt",
                    Position = Positions.QB,
                    Height = "6-6",
                    Weight = 245,
                    Age = 35,
                    Exp = 13,
                    College = "Virginia",
                    Speed = 75,
                    Strength = 70,
                    Agility = 80,
                    Awareness = 88,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 88,
                    Catching = 35,
                    Rushing = 55,
                    Blocking = 30,
                    Tackling = 25,
                    Coverage = 20,
                    Kicking = 10,
                    Potential = 88,
                    Progression = 78,
                    Health = 93,
                    Discipline = 100
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} QBs");
        }
    }
}
