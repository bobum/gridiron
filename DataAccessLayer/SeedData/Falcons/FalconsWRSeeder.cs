using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons wide receivers
    /// </summary>
    public static class FalconsWRSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 11,
                    LastName = "Jones",
                    FirstName = "Julio",
                    Position = Positions.WR,
                    Height = "6-3",
                    Weight = 220,
                    Age = 27,
                    Exp = 6,
                    College = "Alabama",
                    Speed = 95,
                    Strength = 70,
                    Agility = 98,
                    Awareness = 85,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 15,
                    Catching = 95,
                    Rushing = 60,
                    Blocking = 40,
                    Tackling = 30,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 85,
                    Health = 95,
                    Discipline = 85
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 12,
                    LastName = "Sanu",
                    FirstName = "Mohamed",
                    Position = Positions.WR,
                    Height = "6-2",
                    Weight = 210,
                    Age = 27,
                    Exp = 5,
                    College = "Rutgers",
                    Speed = 95,
                    Strength = 70,
                    Agility = 98,
                    Awareness = 85,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 15,
                    Catching = 95,
                    Rushing = 60,
                    Blocking = 40,
                    Tackling = 30,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 85,
                    Health = 95,
                    Discipline = 81
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 18,
                    LastName = "Gabriel",
                    FirstName = "Taylor",
                    Position = Positions.WR,
                    Height = "5-8",
                    Weight = 167,
                    Age = 25,
                    Exp = 3,
                    College = "Abilene Christian",
                    Speed = 95,
                    Strength = 70,
                    Agility = 98,
                    Awareness = 85,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 15,
                    Catching = 95,
                    Rushing = 60,
                    Blocking = 40,
                    Tackling = 30,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 85,
                    Health = 95,
                    Discipline = 82
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 16,
                    LastName = "Hardy",
                    FirstName = "Justin",
                    Position = Positions.WR,
                    Height = "5-10",
                    Weight = 192,
                    Age = 24,
                    Exp = 2,
                    College = "East Carolina",
                    Speed = 95,
                    Strength = 70,
                    Agility = 98,
                    Awareness = 85,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 15,
                    Catching = 95,
                    Rushing = 60,
                    Blocking = 40,
                    Tackling = 30,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 85,
                    Health = 95,
                    Discipline = 75
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 19,
                    LastName = "Robinson",
                    FirstName = "Aldrick",
                    Position = Positions.WR,
                    Height = "5-10",
                    Weight = 187,
                    Age = 28,
                    Exp = 4,
                    College = "Southern Methodist",
                    Speed = 95,
                    Strength = 70,
                    Agility = 98,
                    Awareness = 85,
                    Fragility = 15,
                    Morale = 90,
                    Passing = 15,
                    Catching = 95,
                    Rushing = 60,
                    Blocking = 40,
                    Tackling = 30,
                    Coverage = 30,
                    Kicking = 10,
                    Potential = 90,
                    Progression = 85,
                    Health = 95,
                    Discipline = 75
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} WRs");
        }
    }
}
