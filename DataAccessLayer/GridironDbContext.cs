using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    /// <summary>
    /// Entity Framework Core DbContext for Gridiron football simulation
    /// Handles persistence of teams, players, games, and play-by-play data to Azure SQL.
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

        public DbSet<PlayerGameStat> PlayerGameStats { get; set; }

        public DbSet<Season> Seasons { get; set; }

        public DbSet<SeasonWeek> SeasonWeeks { get; set; }

        // User and authorization
        public DbSet<User> Users { get; set; }

        public DbSet<UserLeagueRole> UserLeagueRoles { get; set; }

        // Player generation data
        public DbSet<FirstName> FirstNames { get; set; }

        public DbSet<LastName> LastNames { get; set; }

        public DbSet<College> Colleges { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Handle SQLite concurrency token update
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entry in ChangeTracker.Entries<Season>())
                {
                    if (entry.State == EntityState.Modified)
                    {
                        entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

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

                // League has many Seasons (one-to-many)
                entity.HasMany(l => l.Seasons)
                      .WithOne(s => s.League)
                      .HasForeignKey(s => s.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete seasons if league deleted

                // League optionally has a current season
                entity.HasOne(l => l.CurrentSeason)
                      .WithMany()
                      .HasForeignKey(l => l.CurrentSeasonId)
                      .OnDelete(DeleteBehavior.NoAction);  // NoAction to avoid circular cascade path with Seasons.LeagueId

                // Soft delete query filter - exclude deleted leagues from queries
                entity.HasQueryFilter(l => !l.IsDeleted);
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

                // Soft delete query filter - exclude deleted conferences from queries
                entity.HasQueryFilter(c => !c.IsDeleted);
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

                // Soft delete query filter - exclude deleted divisions from queries
                entity.HasQueryFilter(d => !d.IsDeleted);
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

                // Soft delete query filter - exclude deleted teams from queries
                entity.HasQueryFilter(t => !t.IsDeleted);
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

                // Ignore injury tracking (runtime only)
                entity.Ignore(p => p.CurrentInjury);
                entity.Ignore(p => p.IsInjured);

                // Soft delete query filter - exclude deleted players from queries
                entity.HasQueryFilter(p => !p.IsDeleted);
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

                // Game optionally belongs to a SeasonWeek
                entity.HasOne(g => g.SeasonWeek)
                      .WithMany(sw => sw.Games)
                      .HasForeignKey(g => g.SeasonWeekId)
                      .OnDelete(DeleteBehavior.SetNull);  // Keep game if week deleted

                entity.HasOne(g => g.AwayTeam)
                      .WithMany()
                      .HasForeignKey(g => g.AwayTeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Soft delete query filter - exclude deleted games from queries
                entity.HasQueryFilter(g => !g.IsDeleted);
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

                // Store JSON and logs as large text
                // Note: Let EF Core choose appropriate column type based on provider
                // SQL Server: nvarchar(max), SQLite: TEXT, etc.
                // Removed explicit HasColumnType to support multiple databases

                // Soft delete query filter - exclude deleted play-by-plays from queries
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // ========================================
            // PLAYER GAME STAT CONFIGURATION
            // ========================================
            modelBuilder.Entity<PlayerGameStat>(entity =>
            {
                entity.HasKey(pgs => pgs.Id);

                // PlayerGameStat belongs to one Player (many-to-one)
                entity.HasOne(pgs => pgs.Player)
                      .WithMany()
                      .HasForeignKey(pgs => pgs.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // PlayerGameStat belongs to one Game (many-to-one)
                entity.HasOne(pgs => pgs.Game)
                      .WithMany()
                      .HasForeignKey(pgs => pgs.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Soft delete query filter
                entity.HasQueryFilter(pgs => !pgs.IsDeleted);
            });

            // ========================================
            // SEASON CONFIGURATION
            // ========================================
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(s => s.Id);

                // Season belongs to one League (many-to-one)
                entity.HasOne(s => s.League)
                      .WithMany(l => l.Seasons)
                      .HasForeignKey(s => s.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Season has many SeasonWeeks (one-to-many)
                entity.HasMany(s => s.Weeks)
                      .WithOne(sw => sw.Season)
                      .HasForeignKey(sw => sw.SeasonId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Season optionally has a champion team
                entity.HasOne(s => s.ChampionTeam)
                      .WithMany()
                      .HasForeignKey(s => s.ChampionTeamId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Soft delete query filter
                entity.HasQueryFilter(s => !s.IsDeleted);

                // SQLite specific configuration for RowVersion
                if (this.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    entity.Property(s => s.RowVersion)
                          .ValueGeneratedNever();
                }
            });

            // ========================================
            // SEASONWEEK CONFIGURATION
            // ========================================
            modelBuilder.Entity<SeasonWeek>(entity =>
            {
                entity.HasKey(sw => sw.Id);

                // SeasonWeek belongs to one Season (many-to-one)
                entity.HasOne(sw => sw.Season)
                      .WithMany(s => s.Weeks)
                      .HasForeignKey(sw => sw.SeasonId)
                      .OnDelete(DeleteBehavior.Cascade);

                // SeasonWeek has many Games (one-to-many)
                entity.HasMany(sw => sw.Games)
                      .WithOne(g => g.SeasonWeek)
                      .HasForeignKey(g => g.SeasonWeekId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Index for efficient queries by season and week number
                entity.HasIndex(sw => new { sw.SeasonId, sw.WeekNumber });

                // Soft delete query filter
                entity.HasQueryFilter(sw => !sw.IsDeleted);
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

            // ========================================
            // USER CONFIGURATION
            // ========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.AzureAdObjectId).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);

                // Unique index on AzureAdObjectId for fast lookups
                entity.HasIndex(u => u.AzureAdObjectId).IsUnique();

                // User has many league roles (one-to-many)
                entity.HasMany(u => u.LeagueRoles)
                      .WithOne(ulr => ulr.User)
                      .HasForeignKey(ulr => ulr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete roles if user is deleted

                // Soft delete query filter - exclude deleted users from queries
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // ========================================
            // USERLEAGUEROLE CONFIGURATION
            // ========================================
            modelBuilder.Entity<UserLeagueRole>(entity =>
            {
                entity.HasKey(ulr => ulr.Id);

                // UserLeagueRole belongs to one User (many-to-one)
                entity.HasOne(ulr => ulr.User)
                      .WithMany(u => u.LeagueRoles)
                      .HasForeignKey(ulr => ulr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // UserLeagueRole belongs to one League (many-to-one)
                entity.HasOne(ulr => ulr.League)
                      .WithMany()
                      .HasForeignKey(ulr => ulr.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);  // Delete role if league is deleted

                // UserLeagueRole optionally belongs to one Team (many-to-one, nullable for Commissioners)
                entity.HasOne(ulr => ulr.Team)
                      .WithMany()
                      .HasForeignKey(ulr => ulr.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);  // Set to null if team is deleted (keep role, just remove team assignment)

                // Composite index for fast authorization queries
                entity.HasIndex(ulr => new { ulr.UserId, ulr.LeagueId });

                // Prevent duplicate role assignments (unique constraint)
                entity.HasIndex(ulr => new { ulr.UserId, ulr.LeagueId, ulr.TeamId }).IsUnique();

                // Soft delete query filter - exclude deleted roles from queries
                entity.HasQueryFilter(ulr => !ulr.IsDeleted);
            });
        }
    }
}
