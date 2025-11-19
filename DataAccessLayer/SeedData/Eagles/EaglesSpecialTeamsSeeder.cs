using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Philadelphia Eagles special teams (K, P, LS)
    /// </summary>
    public static class EaglesSpecialTeamsSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                new Player
                {
                    TeamId = teamId,
                    Number = 6,
                    LastName = "Sturgis",
                    FirstName = "Caleb",
                    Position = Positions.K,
                    Height = "5-9",
                    Weight = 192,
                    Age = 27,
                    Exp = 4,
                    College = "Florida",
                    Speed = 61,
                    Strength = 66,
                    Agility = 71,
                    Awareness = 86,
                    Fragility = 11,
                    Morale = 96,
                    Passing = 11,
                    Catching = 21,
                    Rushing = 16,
                    Blocking = 21,
                    Tackling = 21,
                    Coverage = 11,
                    Kicking = 96,
                    Potential = 81,
                    Progression = 76,
                    Health = 96,
                    Discipline = 92
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 8,
                    LastName = "Jones",
                    FirstName = "Donnie",
                    Position = Positions.P,
                    Height = "6-2",
                    Weight = 221,
                    Age = 36,
                    Exp = 13,
                    College = "LSU",
                    Speed = 66,
                    Strength = 61,
                    Agility = 71,
                    Awareness = 81,
                    Fragility = 13,
                    Morale = 91,
                    Passing = 11,
                    Catching = 21,
                    Rushing = 16,
                    Blocking = 21,
                    Tackling = 21,
                    Coverage = 11,
                    Kicking = 93,
                    Potential = 86,
                    Progression = 81,
                    Health = 96,
                    Discipline = 100
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 46,
                    LastName = "Dorenbos",
                    FirstName = "Jon",
                    Position = Positions.LS,
                    Height = "6-0",
                    Weight = 250,
                    Age = 36,
                    Exp = 14,
                    College = "UTEP",
                    Speed = 61,
                    Strength = 81,
                    Agility = 71,
                    Awareness = 81,
                    Fragility = 16,
                    Morale = 86,
                    Passing = 11,
                    Catching = 21,
                    Rushing = 21,
                    Blocking = 81,
                    Tackling = 31,
                    Coverage = 21,
                    Kicking = 11,
                    Potential = 81,
                    Progression = 76,
                    Health = 91,
                    Discipline = 93
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 45,
                    LastName = "Bellamy",
                    FirstName = "Aaron",
                    Position = Positions.FB,
                    Height = "6-0",
                    Weight = 238,
                    Age = 26,
                    Exp = 3,
                    College = "Penn State",
                    Speed = 74,
                    Strength = 84,
                    Agility = 78,
                    Awareness = 79,
                    Fragility = 17,
                    Morale = 84,
                    Passing = 11,
                    Catching = 58,
                    Rushing = 78,
                    Blocking = 84,
                    Tackling = 58,
                    Coverage = 29,
                    Kicking = 11,
                    Potential = 83,
                    Progression = 79,
                    Health = 91,
                    Discipline = 81
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} Special Teams (K/P/LS/FB)");
        }
    }
}
