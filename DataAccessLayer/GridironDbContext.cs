using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    /// <summary>
    /// Entity Framework Core DbContext for Gridiron football simulation
    /// Handles persistence of teams, players, games, and play-by-play data to Azure SQL
    /// </summary>
    public class GridironDbContext : DbContext
    {
        public GridironDbContext(DbContextOptions<GridironDbContext> options)
            : base(options)
        {
        }

        // Entity sets
        public DbSet<League> Leagues { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<PlayByPlay> PlayByPlays { get; set; }

        // Player generation data
        public DbSet<FirstName> FirstNames { get; set; }
        public DbSet<LastName> LastNames { get; set; }
        public DbSet<College> Colleges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // LEAGUE CONFIGURATION
            // ========================================
            modelBuilder.Entity<League>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Name).IsRequired().HasMaxLength(100);

                // League has many Conferences (one-to-many)
                entity.HasMany(l => l.Conferences)
                      .WithOne()
                      .HasForeignKey(c => c.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete conferences if league deleted
            });

            // ========================================
            // CONFERENCE CONFIGURATION
            // ========================================
            modelBuilder.Entity<Conference>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

                // Conference has many Divisions (one-to-many)
                entity.HasMany(c => c.Divisions)
                      .WithOne()
                      .HasForeignKey(d => d.ConferenceId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete divisions if conference deleted
            });

            // ========================================
            // DIVISION CONFIGURATION
            // ========================================
            modelBuilder.Entity<Division>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);

                // Division has many Teams (one-to-many)
                entity.HasMany(d => d.Teams)
                      .WithOne()
                      .HasForeignKey(t => t.DivisionId)
                      .OnDelete(DeleteBehavior.SetNull);  // If division deleted, teams remain but without division
            });

            // ========================================
            // TEAM CONFIGURATION
            // ========================================
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.Property(t => t.City).HasMaxLength(100);

                // Team has many Players (one-to-many)
                entity.HasMany(t => t.Players)
                      .WithOne()
                      .HasForeignKey(p => p.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);  // If team deleted, players become free agents

                // Ignore complex properties that we're not persisting yet
                entity.Ignore(t => t.Stats);
                entity.Ignore(t => t.TeamStats);
                entity.Ignore(t => t.OffenseDepthChart);
                entity.Ignore(t => t.DefenseDepthChart);
                entity.Ignore(t => t.FieldGoalOffenseDepthChart);
                entity.Ignore(t => t.FieldGoalDefenseDepthChart);
                entity.Ignore(t => t.KickoffOffenseDepthChart);
                entity.Ignore(t => t.KickoffDefenseDepthChart);
                entity.Ignore(t => t.PuntOffenseDepthChart);
                entity.Ignore(t => t.PuntDefenseDepthChart);
                entity.Ignore(t => t.HeadCoach);
                entity.Ignore(t => t.OffensiveCoordinator);
                entity.Ignore(t => t.DefensiveCoordinator);
                entity.Ignore(t => t.SpecialTeamsCoordinator);
                entity.Ignore(t => t.AssistantCoaches);
                entity.Ignore(t => t.HeadAthleticTrainer);
                entity.Ignore(t => t.TeamDoctor);
                entity.Ignore(t => t.DirectorOfScouting);
                entity.Ignore(t => t.CollegeScouts);
                entity.Ignore(t => t.ProScouts);
            });

            // ========================================
            // PLAYER CONFIGURATION
            // ========================================
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FirstName).HasMaxLength(50);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(50);
                entity.Property(p => p.College).HasMaxLength(100);
                entity.Property(p => p.Height).HasMaxLength(10);

                // Ignore stat dictionaries for now (can add JSON serialization later if needed)
                entity.Ignore(p => p.Stats);
                entity.Ignore(p => p.SeasonStats);
                entity.Ignore(p => p.CareerStats);
                
                // Ignore injury tracking (runtime only)
                entity.Ignore(p => p.CurrentInjury);
                entity.Ignore(p => p.IsInjured);
            });

            // ========================================
            // GAME CONFIGURATION
            // ========================================
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.Id);

                // Game has two teams (many-to-one relationships)
                entity.HasOne(g => g.HomeTeam)
                      .WithMany()
                      .HasForeignKey(g => g.HomeTeamId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't delete games if team is deleted

                entity.HasOne(g => g.AwayTeam)
                      .WithMany()
                      .HasForeignKey(g => g.AwayTeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ignore runtime/state properties
                entity.Ignore(g => g.CurrentPlay);
                entity.Ignore(g => g.Plays);
                entity.Ignore(g => g.Logger);
                entity.Ignore(g => g.Halves);
                entity.Ignore(g => g.CurrentQuarter);
                entity.Ignore(g => g.CurrentHalf);
                entity.Ignore(g => g.TimeRemaining);
            });

            // ========================================
            // PLAYBYPLAY CONFIGURATION
            // ========================================
            modelBuilder.Entity<PlayByPlay>(entity =>
            {
                entity.HasKey(p => p.Id);

                // PlayByPlay belongs to one Game (one-to-one bidirectional)
                entity.HasOne(p => p.Game)
                      .WithOne(g => g.PlayByPlay)
                      .HasForeignKey<PlayByPlay>(p => p.GameId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete playbyplay if game is deleted

                // Store JSON as large text
                entity.Property(p => p.PlaysJson).HasColumnType("nvarchar(max)");
                entity.Property(p => p.PlayByPlayLog).HasColumnType("nvarchar(max)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ========================================
            // PLAYER GENERATION DATA CONFIGURATION
            // ========================================
            modelBuilder.Entity<FirstName>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Name).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<LastName>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Name).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<College>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });
        }
    }
}
