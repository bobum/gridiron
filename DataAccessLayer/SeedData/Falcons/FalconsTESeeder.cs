using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons tight ends.
    /// </summary>
    public static class FalconsTESeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 81,
                    LastName = "Hooper",
                    FirstName = "Austin",
                    Position = Positions.TE,
                    Height = "6-4",
                    Weight = 248,
                    Age = 21,
                    Exp = 0,
                    College = "Stanford",
                    Speed = 75,
                    Strength = 85,
                    Agility = 80,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 70,
                    Rushing = 60,
                    Blocking = 85,
                    Tackling = 60,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 72
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 82,
                    LastName = "Perkins",
                    FirstName = "Joshua",
                    Position = Positions.TE,
                    Height = "6-4",
                    Weight = 227,
                    Age = 23,
                    Exp = 0,
                    College = "Washington",
                    Speed = 75,
                    Strength = 85,
                    Agility = 80,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 70,
                    Rushing = 60,
                    Blocking = 85,
                    Tackling = 60,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 70
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 83,
                    LastName = "Tamme",
                    FirstName = "Jacob",
                    Position = Positions.TE,
                    Height = "6-3",
                    Weight = 230,
                    Age = 31,
                    Exp = 9,
                    College = "Kentucky",
                    Speed = 75,
                    Strength = 85,
                    Agility = 80,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 70,
                    Rushing = 60,
                    Blocking = 85,
                    Tackling = 60,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 89
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 80,
                    LastName = "Toilolo",
                    FirstName = "Levine",
                    Position = Positions.TE,
                    Height = "6-8",
                    Weight = 265,
                    Age = 25,
                    Exp = 4,
                    College = "Stanford",
                    Speed = 75,
                    Strength = 85,
                    Agility = 80,
                    Awareness = 80,
                    Fragility = 15,
                    Morale = 85,
                    Passing = 10,
                    Catching = 70,
                    Rushing = 60,
                    Blocking = 85,
                    Tackling = 60,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 81
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} TEs");
        }
    }
}
