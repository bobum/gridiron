using DomainObjects;
using static DomainObjects.StatTypes;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds Atlanta Falcons defensive linemen (DE, DT).
    /// </summary>
    public static class FalconsDLSeeder
    {
        public static async Task SeedAsync(GridironDbContext db, int teamId)
        {
            var players = new[]
            {
                // Defensive Ends
                new Player
                {
                    TeamId = teamId,
                    Number = 90,
                    LastName = "Shelby",
                    FirstName = "Derrick",
                    Position = Positions.DE,
                    Height = "6-2",
                    Weight = 280,
                    Age = 27,
                    Exp = 5,
                    College = "Utah",
                    Speed = 70,
                    Strength = 90,
                    Agility = 75,
                    Awareness = 80,
                    Fragility = 18,
                    Morale = 85,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 90,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 76
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 93,
                    LastName = "Freeney",
                    FirstName = "Dwight",
                    Position = Positions.DE,
                    Height = "6-1",
                    Weight = 268,
                    Age = 36,
                    Exp = 14,
                    College = "Syracuse",
                    Speed = 70,
                    Strength = 90,
                    Agility = 75,
                    Awareness = 80,
                    Fragility = 18,
                    Morale = 85,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 90,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 76
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 94,
                    LastName = "Jackson",
                    FirstName = "Tyson",
                    Position = Positions.DE,
                    Height = "6-4",
                    Weight = 296,
                    Age = 30,
                    Exp = 8,
                    College = "LSU",
                    Speed = 70,
                    Strength = 90,
                    Agility = 75,
                    Awareness = 80,
                    Fragility = 18,
                    Morale = 85,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 90,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 75
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 99,
                    LastName = "Clayborn",
                    FirstName = "Adrian",
                    Position = Positions.DE,
                    Height = "6-3",
                    Weight = 280,
                    Age = 28,
                    Exp = 6,
                    College = "Iowa",
                    Speed = 70,
                    Strength = 90,
                    Agility = 75,
                    Awareness = 80,
                    Fragility = 18,
                    Morale = 85,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 90,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 85,
                    Progression = 80,
                    Health = 92,
                    Discipline = 78
                },

                // Defensive Tackles
                new Player
                {
                    TeamId = teamId,
                    Number = 77,
                    LastName = "Hageman",
                    FirstName = "Ra'Shede",
                    Position = Positions.DT,
                    Height = "6-6",
                    Weight = 318,
                    Age = 26,
                    Exp = 3,
                    College = "Minnesota",
                    Speed = 65,
                    Strength = 95,
                    Agility = 70,
                    Awareness = 80,
                    Fragility = 22,
                    Morale = 88,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 95,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 80,
                    Progression = 70,
                    Health = 90,
                    Discipline = 77
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 95,
                    LastName = "Babineaux",
                    FirstName = "Jonathan",
                    Position = Positions.DT,
                    Height = "6-2",
                    Weight = 300,
                    Age = 34,
                    Exp = 12,
                    College = "Iowa",
                    Speed = 65,
                    Strength = 95,
                    Agility = 70,
                    Awareness = 80,
                    Fragility = 22,
                    Morale = 88,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 95,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 80,
                    Progression = 70,
                    Health = 90,
                    Discipline = 82
                },
                new Player
                {
                    TeamId = teamId,
                    Number = 97,
                    LastName = "Jarrett",
                    FirstName = "Grady",
                    Position = Positions.DT,
                    Height = "6-0",
                    Weight = 305,
                    Age = 23,
                    Exp = 2,
                    College = "Clemson",
                    Speed = 65,
                    Strength = 95,
                    Agility = 70,
                    Awareness = 80,
                    Fragility = 22,
                    Morale = 88,
                    Passing = 10,
                    Catching = 20,
                    Rushing = 20,
                    Blocking = 70,
                    Tackling = 95,
                    Coverage = 40,
                    Kicking = 10,
                    Potential = 80,
                    Progression = 70,
                    Health = 90,
                    Discipline = 70
                }
            };

            db.Players.AddRange(players);
            await db.SaveChangesAsync();
            Console.WriteLine($"  ✓ Added {players.Length} DL (DE/DT)");
        }
    }
}
