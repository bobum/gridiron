using DomainObjects;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Seeds the two teams (without players)
    /// </summary>
    public static class TeamSeeder
    {
        public static async Task SeedTeamsAsync(GridironDbContext db)
        {
            var falcons = new Team
            {
                City = "Atlanta",
                Name = "Falcons",
                Budget = 200000000,
                Championships = 0,
                Wins = 0,
                Losses = 0,
                Ties = 0,
                FanSupport = 85,
                Chemistry = 80
            };

            var eagles = new Team
            {
                City = "Philadelphia",
                Name = "Eagles",
                Budget = 210000000,
                Championships = 1,
                Wins = 0,
                Losses = 0,
                Ties = 0,
                FanSupport = 90,
                Chemistry = 85
            };

            db.Teams.Add(falcons);
            db.Teams.Add(eagles);
            await db.SaveChangesAsync();
        }
    }
}
