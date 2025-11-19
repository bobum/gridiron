using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Philadelphia Eagles quarterbacks
    /// </summary>
    public static class EaglesQBSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 11,
                    LastName = "Wentz",
                    FirstName = "Carson",
                    Position = Positions.QB,
                    Height = "6-5",
                    Weight = 237,
                    Age = 23,
                    Exp = 0,
                    College = "North Dakota State",
                    Speed = 79,
                    Strength = 71,
                    Agility = 86,
                    Awareness = 91,
                    Fragility = 16,
                    Morale = 96,
                    Passing = 93,
                    Catching = 36,
                    Rushing = 61,
                    Blocking = 31,
                    Tackling = 26,
                    Coverage = 21,
                    Kicking = 11,
                    Potential = 91,
                    Progression = 81,
                    Health = 96,
                    Discipline = 82
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 10,
                    LastName = "Daniel",
                    FirstName = "Chase",
                    Position = Positions.QB,
                    Height = "6-0",
                    Weight = 225,
                    Age = 29,
                    Exp = 8,
                    College = "Missouri",
                    Speed = 79,
                    Strength = 71,
                    Agility = 86,
                    Awareness = 91,
                    Fragility = 16,
                    Morale = 96,
                    Passing = 93,
                    Catching = 36,
                    Rushing = 61,
                    Blocking = 31,
                    Tackling = 26,
                    Coverage = 21,
                    Kicking = 11,
                    Potential = 91,
                    Progression = 81,
                    Health = 96,
                    Discipline = 96
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} QBs");
        }
    }
}
