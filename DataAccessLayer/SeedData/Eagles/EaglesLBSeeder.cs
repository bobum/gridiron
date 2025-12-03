using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Philadelphia Eagles linebackers.
    /// </summary>
    public static class EaglesLBSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 95,
                    LastName = "Kendricks",
                    FirstName = "Mychal",
                    Position = Positions.LB,
                    Height = "6-0",
                    Weight = 240,
                    Age = 25,
                    Exp = 5,
                    College = "California",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 71
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 53,
                    LastName = "Bradham",
                    FirstName = "Nigel",
                    Position = Positions.LB,
                    Height = "6-2",
                    Weight = 241,
                    Age = 27,
                    Exp = 5,
                    College = "Florida State",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 74
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 58,
                    LastName = "Hicks",
                    FirstName = "Jordan",
                    Position = Positions.LB,
                    Height = "6-1",
                    Weight = 236,
                    Age = 24,
                    Exp = 2,
                    College = "Texas",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 70
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 52,
                    LastName = "Goode",
                    FirstName = "Najee",
                    Position = Positions.LB,
                    Height = "6-0",
                    Weight = 244,
                    Age = 27,
                    Exp = 5,
                    College = "West Virginia",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 73
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 54,
                    LastName = "Grugier-Hill",
                    FirstName = "Kamu",
                    Position = Positions.LB,
                    Height = "6-2",
                    Weight = 220,
                    Age = 22,
                    Exp = 0,
                    College = "Eastern Illinois",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 74
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 50,
                    LastName = "Tulloch",
                    FirstName = "Stephen",
                    Position = Positions.LB,
                    Height = "5-11",
                    Weight = 245,
                    Age = 31,
                    Exp = 11,
                    College = "North Carolina State",
                    Speed = 86,
                    Strength = 81,
                    Agility = 89,
                    Awareness = 79,
                    Fragility = 19,
                    Morale = 86,
                    Passing = 16,
                    Catching = 51,
                    Rushing = 36,
                    Blocking = 56,
                    Tackling = 89,
                    Coverage = 81,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 81,
                    Health = 94,
                    Discipline = 75
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} LBs");
        }
    }
}
