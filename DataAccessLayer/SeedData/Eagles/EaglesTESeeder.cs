using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Philadelphia Eagles tight ends.
    /// </summary>
    public static class EaglesTESeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 86,
                    LastName = "Ertz",
                    FirstName = "Zach",
                    Position = Positions.TE,
                    Height = "6-5",
                    Weight = 250,
                    Age = 25,
                    Exp = 4,
                    College = "Stanford",
                    Speed = 76,
                    Strength = 86,
                    Agility = 81,
                    Awareness = 81,
                    Fragility = 16,
                    Morale = 86,
                    Passing = 11,
                    Catching = 71,
                    Rushing = 61,
                    Blocking = 86,
                    Tackling = 61,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 86,
                    Progression = 81,
                    Health = 93,
                    Discipline = 77
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 87,
                    LastName = "Celek",
                    FirstName = "Brent",
                    Position = Positions.TE,
                    Height = "6-4",
                    Weight = 255,
                    Age = 31,
                    Exp = 10,
                    College = "Cincinnati",
                    Speed = 76,
                    Strength = 86,
                    Agility = 81,
                    Awareness = 81,
                    Fragility = 16,
                    Morale = 86,
                    Passing = 11,
                    Catching = 71,
                    Rushing = 61,
                    Blocking = 86,
                    Tackling = 61,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 86,
                    Progression = 81,
                    Health = 93,
                    Discipline = 81
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 47,
                    LastName = "Burton",
                    FirstName = "Trey",
                    Position = Positions.TE,
                    Height = "6-3",
                    Weight = 235,
                    Age = 24,
                    Exp = 3,
                    College = "Florida",
                    Speed = 76,
                    Strength = 86,
                    Agility = 81,
                    Awareness = 81,
                    Fragility = 16,
                    Morale = 86,
                    Passing = 11,
                    Catching = 71,
                    Rushing = 61,
                    Blocking = 86,
                    Tackling = 61,
                    Coverage = 31,
                    Kicking = 11,
                    Potential = 86,
                    Progression = 81,
                    Health = 93,
                    Discipline = 77
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} TEs");
        }
    }
}
