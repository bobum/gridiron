using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Philadelphia Eagles running backs.
    /// </summary>
    public static class EaglesRBSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 24,
                    LastName = "Mathews",
                    FirstName = "Ryan",
                    Position = Positions.RB,
                    Height = "6-0",
                    Weight = 220,
                    Age = 28,
                    Exp = 7,
                    College = "Fresno State",
                    Speed = 96,
                    Strength = 76,
                    Agility = 96,
                    Awareness = 86,
                    Fragility = 21,
                    Morale = 91,
                    Passing = 21,
                    Catching = 76,
                    Rushing = 96,
                    Blocking = 56,
                    Tackling = 41,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 86,
                    Health = 93,
                    Discipline = 80
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 28,
                    LastName = "Smallwood",
                    FirstName = "Wendell",
                    Position = Positions.RB,
                    Height = "5-10",
                    Weight = 208,
                    Age = 22,
                    Exp = 0,
                    College = "West Virginia",
                    Speed = 96,
                    Strength = 76,
                    Agility = 96,
                    Awareness = 86,
                    Fragility = 21,
                    Morale = 91,
                    Passing = 21,
                    Catching = 76,
                    Rushing = 96,
                    Blocking = 56,
                    Tackling = 41,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 86,
                    Health = 93,
                    Discipline = 74
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 34,
                    LastName = "Barner",
                    FirstName = "Kenjon",
                    Position = Positions.RB,
                    Height = "5-9",
                    Weight = 195,
                    Age = 27,
                    Exp = 3,
                    College = "Oregon",
                    Speed = 96,
                    Strength = 76,
                    Agility = 96,
                    Awareness = 86,
                    Fragility = 21,
                    Morale = 91,
                    Passing = 21,
                    Catching = 76,
                    Rushing = 96,
                    Blocking = 56,
                    Tackling = 41,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 86,
                    Health = 93,
                    Discipline = 79
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 43,
                    LastName = "Sproles",
                    FirstName = "Darren",
                    Position = Positions.RB,
                    Height = "5-6",
                    Weight = 190,
                    Age = 33,
                    Exp = 12,
                    College = "Kansas State",
                    Speed = 96,
                    Strength = 76,
                    Agility = 96,
                    Awareness = 86,
                    Fragility = 21,
                    Morale = 91,
                    Passing = 21,
                    Catching = 76,
                    Rushing = 96,
                    Blocking = 56,
                    Tackling = 41,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 89,
                    Progression = 86,
                    Health = 93,
                    Discipline = 84
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} RBs");
        }
    }
}
