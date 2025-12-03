using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons running backs and fullbacks.
    /// </summary>
    public static class FalconsRBSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 24,
                    LastName = "Freeman",
                    FirstName = "Devonta",
                    Position = Positions.RB,
                    Height = "5-8",
                    Weight = 206,
                    Age = 24,
                    Exp = 3,
                    College = "Florida State",
                    Speed = 95,
                    Strength = 75,
                    Agility = 95,
                    Awareness = 85,
                    Fragility = 20,
                    Morale = 90,
                    Passing = 20,
                    Catching = 75,
                    Rushing = 95,
                    Blocking = 55,
                    Tackling = 40,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 88,
                    Progression = 85,
                    Health = 92,
                    Discipline = 73
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 26,
                    LastName = "Coleman",
                    FirstName = "Tevin",
                    Position = Positions.RB,
                    Height = "6-1",
                    Weight = 210,
                    Age = 23,
                    Exp = 2,
                    College = "Indiana",
                    Speed = 95,
                    Strength = 75,
                    Agility = 95,
                    Awareness = 85,
                    Fragility = 20,
                    Morale = 90,
                    Passing = 20,
                    Catching = 75,
                    Rushing = 95,
                    Blocking = 55,
                    Tackling = 40,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 88,
                    Progression = 85,
                    Health = 92,
                    Discipline = 76
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 42,
                    LastName = "DiMarco",
                    FirstName = "Patrick",
                    Position = Positions.FB,
                    Height = "6-1",
                    Weight = 234,
                    Age = 27,
                    Exp = 5,
                    College = "South Carolina",
                    Speed = 75,
                    Strength = 85,
                    Agility = 80,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 60,
                    Rushing = 80,
                    Blocking = 85,
                    Tackling = 60,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 82
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} RBs/FBs");
        }
    }
}
