using DomainObjects;
using GameManagement.Configuration;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Generates NFL-style regular season schedules.
/// Handles division matchups, conference matchups, inter-conference games, and bye weeks.
/// </summary>
public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly ILogger<ScheduleGeneratorService> _logger;

    public ScheduleGeneratorService(ILogger<ScheduleGeneratorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Season GenerateSchedule(Season season, int? seed = null)
    {
        if (season == null)
            throw new ArgumentNullException(nameof(season));

        if (season.League == null)
            throw new ArgumentException("Season must have League loaded with full structure", nameof(season));

        // Validate regular season weeks against cap
        if (season.RegularSeasonWeeks > ScheduleConstants.MaxRegularSeasonWeeks)
        {
            throw new InvalidOperationException(
                $"Season has {season.RegularSeasonWeeks} regular season weeks, but maximum allowed is {ScheduleConstants.MaxRegularSeasonWeeks}.");
        }

        var validation = ValidateLeagueStructure(season.League);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"League structure is not valid for schedule generation: {string.Join(", ", validation.Errors)}");
        }

        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        // Get all teams organized by division
        var conferences = season.League.Conferences;
        var allTeams = conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .ToList();

        var teamCount = allTeams.Count;
        var weeksNeeded = season.RegularSeasonWeeks + 1; // +1 for bye week coverage

        _logger.LogInformation(
            "Generating schedule for season {SeasonId} with {TeamCount} teams over {Weeks} weeks",
            season.Id, teamCount, weeksNeeded);

        // Create season weeks
        season.Weeks.Clear();
        for (int week = 1; week <= weeksNeeded; week++)
        {
            season.Weeks.Add(new SeasonWeek
            {
                SeasonId = season.Id,
                WeekNumber = week,
                Phase = SeasonPhase.RegularSeason,
                Status = WeekStatus.Scheduled
            });
        }

        // Build matchup pools
        var matchups = new List<ScheduledMatchup>();

        // 1. Division games (6 per team - play each division rival twice)
        matchups.AddRange(GenerateDivisionMatchups(conferences));

        // 2. Conference cross-division games (4 per team)
        matchups.AddRange(GenerateConferenceCrossDivisionMatchups(conferences, season.Year, random));

        // 3. Inter-conference games (4 per team)
        matchups.AddRange(GenerateInterConferenceMatchups(conferences, season.Year, random));

        // 4. Remaining games (to fill 17 games - typically 2-3 more based on structure)
        var gamesPerTeam = season.RegularSeasonWeeks;
        matchups.AddRange(GenerateRemainingMatchups(conferences, matchups, gamesPerTeam, random));

        // Shuffle matchups for variety
        matchups = matchups.OrderBy(_ => random.Next()).ToList();

        // Assign matchups to weeks with bye week handling
        AssignMatchupsToWeeks(season, matchups, allTeams, random);

        // Balance home/away (target 8-9 or 9-8 split)
        BalanceHomeAway(season, allTeams);

        var totalGames = season.Weeks.Sum(w => w.Games.Count);
        _logger.LogInformation(
            "Generated {TotalGames} games across {Weeks} weeks for season {SeasonId}",
            totalGames, season.Weeks.Count, season.Id);

        return season;
    }

    public ScheduleValidationResult ValidateLeagueStructure(League league)
    {
        if (league == null)
            return ScheduleValidationResult.Failure("League cannot be null");

        if (league.Conferences == null || league.Conferences.Count == 0)
            return ScheduleValidationResult.Failure("League must have at least one conference");

        var errors = new List<string>();
        var warnings = new List<string>();

        // Hard cap: Maximum conferences
        if (league.Conferences.Count > ScheduleConstants.MaxConferences)
        {
            errors.Add($"League cannot have more than {ScheduleConstants.MaxConferences} conferences. Found {league.Conferences.Count}.");
        }

        // Check for consistent structure
        var conferenceDivisionCounts = league.Conferences.Select(c => c.Divisions?.Count ?? 0).Distinct().ToList();
        if (conferenceDivisionCounts.Count > 1)
        {
            errors.Add("All conferences must have the same number of divisions");
        }

        var divisionsPerConference = conferenceDivisionCounts.FirstOrDefault();

        // Hard cap: Maximum divisions per conference
        if (divisionsPerConference > ScheduleConstants.MaxDivisionsPerConference)
        {
            errors.Add($"Each conference cannot have more than {ScheduleConstants.MaxDivisionsPerConference} divisions. Found {divisionsPerConference}.");
        }

        var divisionTeamCounts = league.Conferences
            .SelectMany(c => c.Divisions ?? new List<Division>())
            .Select(d => d.Teams?.Count ?? 0)
            .Distinct()
            .ToList();

        if (divisionTeamCounts.Count > 1)
        {
            errors.Add("All divisions must have the same number of teams");
        }

        var teamsPerDivision = divisionTeamCounts.FirstOrDefault();

        // Hard cap: Maximum teams per division
        if (teamsPerDivision > ScheduleConstants.MaxTeamsPerDivision)
        {
            errors.Add($"Each division cannot have more than {ScheduleConstants.MaxTeamsPerDivision} teams. Found {teamsPerDivision}.");
        }

        if (teamsPerDivision < 2)
        {
            errors.Add("Each division must have at least 2 teams");
        }

        // Total team count check
        var totalTeams = league.Conferences
            .SelectMany(c => c.Divisions ?? new List<Division>())
            .SelectMany(d => d.Teams ?? new List<Team>())
            .Count();

        if (totalTeams > ScheduleConstants.MaxTotalTeams)
        {
            errors.Add($"League cannot have more than {ScheduleConstants.MaxTotalTeams} total teams. Found {totalTeams}.");
        }

        // Warnings for non-optimal (but valid) configurations
        if (league.Conferences.Count < ScheduleConstants.MaxConferences)
        {
            warnings.Add($"NFL-style scheduling is optimized for {ScheduleConstants.MaxConferences} conferences. League has {league.Conferences.Count}.");
        }

        if (divisionsPerConference > 0 && divisionsPerConference < ScheduleConstants.MaxDivisionsPerConference)
        {
            warnings.Add($"NFL-style scheduling is optimized for {ScheduleConstants.MaxDivisionsPerConference} divisions per conference. Found {divisionsPerConference}.");
        }

        if (teamsPerDivision > 0 && teamsPerDivision < ScheduleConstants.MaxTeamsPerDivision)
        {
            warnings.Add($"NFL-style scheduling is optimized for {ScheduleConstants.MaxTeamsPerDivision} teams per division. Found {teamsPerDivision}.");
        }

        return new ScheduleValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Generate division matchups - each team plays division rivals twice (home and away)
    /// </summary>
    private List<ScheduledMatchup> GenerateDivisionMatchups(List<Conference> conferences)
    {
        var matchups = new List<ScheduledMatchup>();

        foreach (var conference in conferences)
        {
            foreach (var division in conference.Divisions)
            {
                var teams = division.Teams;
                for (int i = 0; i < teams.Count; i++)
                {
                    for (int j = i + 1; j < teams.Count; j++)
                    {
                        // Each pair plays twice - once home, once away
                        matchups.Add(new ScheduledMatchup
                        {
                            HomeTeamId = teams[i].Id,
                            AwayTeamId = teams[j].Id,
                            MatchupType = MatchupType.Division
                        });
                        matchups.Add(new ScheduledMatchup
                        {
                            HomeTeamId = teams[j].Id,
                            AwayTeamId = teams[i].Id,
                            MatchupType = MatchupType.Division
                        });
                    }
                }
            }
        }

        return matchups;
    }

    /// <summary>
    /// Generate conference cross-division matchups (4 games vs another division in same conference)
    /// </summary>
    private List<ScheduledMatchup> GenerateConferenceCrossDivisionMatchups(
        List<Conference> conferences, int year, Random random)
    {
        var matchups = new List<ScheduledMatchup>();

        foreach (var conference in conferences)
        {
            var divisions = conference.Divisions;
            if (divisions.Count < 2) continue;

            // Rotate which divisions play each other based on year
            var divisionPairings = GetRotatingDivisionPairings(divisions.Count, year);

            foreach (var (divIndex1, divIndex2) in divisionPairings)
            {
                var div1Teams = divisions[divIndex1].Teams;
                var div2Teams = divisions[divIndex2].Teams;

                // Each team from div1 plays each team from div2 once
                foreach (var team1 in div1Teams)
                {
                    foreach (var team2 in div2Teams)
                    {
                        var isHome = random.Next(2) == 0;
                        matchups.Add(new ScheduledMatchup
                        {
                            HomeTeamId = isHome ? team1.Id : team2.Id,
                            AwayTeamId = isHome ? team2.Id : team1.Id,
                            MatchupType = MatchupType.ConferenceCrossDivision
                        });
                    }
                }
            }
        }

        return matchups;
    }

    /// <summary>
    /// Generate inter-conference matchups (4 games vs a division in the other conference)
    /// </summary>
    private List<ScheduledMatchup> GenerateInterConferenceMatchups(
        List<Conference> conferences, int year, Random random)
    {
        var matchups = new List<ScheduledMatchup>();

        if (conferences.Count < 2) return matchups;

        // For simplicity, pair divisions by index (can be enhanced with rotation)
        var conf1 = conferences[0];
        var conf2 = conferences[1];

        var divisionCount = Math.Min(conf1.Divisions.Count, conf2.Divisions.Count);

        // Rotate which divisions play each other based on year
        var offset = year % divisionCount;

        for (int i = 0; i < divisionCount; i++)
        {
            var div1 = conf1.Divisions[i];
            var div2 = conf2.Divisions[(i + offset) % divisionCount];

            // Each team from div1 plays each team from div2 once
            foreach (var team1 in div1.Teams)
            {
                foreach (var team2 in div2.Teams)
                {
                    var isHome = random.Next(2) == 0;
                    matchups.Add(new ScheduledMatchup
                    {
                        HomeTeamId = isHome ? team1.Id : team2.Id,
                        AwayTeamId = isHome ? team2.Id : team1.Id,
                        MatchupType = MatchupType.InterConference
                    });
                }
            }
        }

        return matchups;
    }

    /// <summary>
    /// Generate remaining matchups to fill the schedule to the required games per team
    /// </summary>
    private List<ScheduledMatchup> GenerateRemainingMatchups(
        List<Conference> conferences, List<ScheduledMatchup> existingMatchups, int gamesPerTeam, Random random)
    {
        var matchups = new List<ScheduledMatchup>();

        // Count existing games per team
        var gamesCount = new Dictionary<int, int>();
        foreach (var matchup in existingMatchups)
        {
            gamesCount.TryAdd(matchup.HomeTeamId, 0);
            gamesCount.TryAdd(matchup.AwayTeamId, 0);
            gamesCount[matchup.HomeTeamId]++;
            gamesCount[matchup.AwayTeamId]++;
        }

        // Get all teams
        var allTeams = conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .ToList();

        // Find teams that need more games
        var teamsNeedingGames = allTeams
            .Where(t => gamesCount.GetValueOrDefault(t.Id, 0) < gamesPerTeam)
            .OrderBy(t => gamesCount.GetValueOrDefault(t.Id, 0))
            .ToList();

        // Create matchups for teams with fewer games (same-place finishers concept)
        // For now, use random pairing from same conference
        while (teamsNeedingGames.Count >= 2)
        {
            var team1 = teamsNeedingGames[0];
            var team1Games = gamesCount.GetValueOrDefault(team1.Id, 0);

            if (team1Games >= gamesPerTeam)
            {
                teamsNeedingGames.RemoveAt(0);
                continue;
            }

            // Find a valid opponent from the same conference preferably
            Team? opponent = null;
            for (int i = 1; i < teamsNeedingGames.Count; i++)
            {
                var potential = teamsNeedingGames[i];
                var potentialGames = gamesCount.GetValueOrDefault(potential.Id, 0);

                // Don't exceed game limit
                if (potentialGames >= gamesPerTeam) continue;

                // Don't create duplicate matchup (check both directions)
                var alreadyPlaying = existingMatchups.Concat(matchups).Any(m =>
                    (m.HomeTeamId == team1.Id && m.AwayTeamId == potential.Id) ||
                    (m.HomeTeamId == potential.Id && m.AwayTeamId == team1.Id));

                if (!alreadyPlaying)
                {
                    opponent = potential;
                    break;
                }
            }

            if (opponent != null)
            {
                var isHome = random.Next(2) == 0;
                matchups.Add(new ScheduledMatchup
                {
                    HomeTeamId = isHome ? team1.Id : opponent.Id,
                    AwayTeamId = isHome ? opponent.Id : team1.Id,
                    MatchupType = MatchupType.SamePlaceFinisher
                });

                gamesCount[team1.Id] = gamesCount.GetValueOrDefault(team1.Id, 0) + 1;
                gamesCount[opponent.Id] = gamesCount.GetValueOrDefault(opponent.Id, 0) + 1;
            }
            else
            {
                // No valid opponent found for this team - remove it to prevent infinite loop
                // This can happen when all potential opponents have already played this team
                teamsNeedingGames.RemoveAt(0);
                continue;
            }

            // Refresh the list
            teamsNeedingGames = allTeams
                .Where(t => gamesCount.GetValueOrDefault(t.Id, 0) < gamesPerTeam)
                .OrderBy(t => gamesCount.GetValueOrDefault(t.Id, 0))
                .ToList();
        }

        return matchups;
    }

    /// <summary>
    /// Assign matchups to weeks, ensuring no team plays twice in a week and handling bye weeks
    /// </summary>
    private void AssignMatchupsToWeeks(Season season, List<ScheduledMatchup> matchups, List<Team> allTeams, Random random)
    {
        var weeks = season.Weeks.OrderBy(w => w.WeekNumber).ToList();
        var teamByeWeeks = new Dictionary<int, int>();
        var teamGamesPerWeek = weeks.ToDictionary(w => w.WeekNumber, _ => new HashSet<int>());

        // Assign bye weeks evenly (weeks 5-14 typically in NFL)
        var byeWeekRange = Enumerable.Range(5, Math.Min(10, weeks.Count - 4)).ToList();
        var teamsPerByeWeek = (int)Math.Ceiling((double)allTeams.Count / byeWeekRange.Count);

        var shuffledTeams = allTeams.OrderBy(_ => random.Next()).ToList();
        var byeWeekIndex = 0;
        var teamsInCurrentByeWeek = 0;

        foreach (var team in shuffledTeams)
        {
            if (byeWeekIndex < byeWeekRange.Count)
            {
                teamByeWeeks[team.Id] = byeWeekRange[byeWeekIndex];
                teamsInCurrentByeWeek++;

                if (teamsInCurrentByeWeek >= teamsPerByeWeek)
                {
                    byeWeekIndex++;
                    teamsInCurrentByeWeek = 0;
                }
            }
        }

        // Assign matchups to weeks
        var unassignedMatchups = new Queue<ScheduledMatchup>(matchups.OrderBy(_ => random.Next()));

        while (unassignedMatchups.Count > 0)
        {
            var matchup = unassignedMatchups.Dequeue();
            var assigned = false;

            // Try to find a valid week for this matchup
            foreach (var week in weeks.OrderBy(_ => random.Next()))
            {
                var weekNum = week.WeekNumber;
                var teamsInWeek = teamGamesPerWeek[weekNum];

                // Check if either team already plays this week
                if (teamsInWeek.Contains(matchup.HomeTeamId) || teamsInWeek.Contains(matchup.AwayTeamId))
                    continue;

                // Check if either team has bye this week
                if (teamByeWeeks.GetValueOrDefault(matchup.HomeTeamId) == weekNum ||
                    teamByeWeeks.GetValueOrDefault(matchup.AwayTeamId) == weekNum)
                    continue;

                // Assign the game
                week.Games.Add(new Game
                {
                    HomeTeamId = matchup.HomeTeamId,
                    AwayTeamId = matchup.AwayTeamId,
                    SeasonWeekId = week.Id,
                    IsComplete = false
                });

                teamsInWeek.Add(matchup.HomeTeamId);
                teamsInWeek.Add(matchup.AwayTeamId);
                assigned = true;
                break;
            }

            if (!assigned)
            {
                _logger.LogWarning(
                    "Could not assign matchup {Home} vs {Away} to any week",
                    matchup.HomeTeamId, matchup.AwayTeamId);
            }
        }
    }

    /// <summary>
    /// Balance home/away games to achieve 8-9 or 9-8 split
    /// </summary>
    private void BalanceHomeAway(Season season, List<Team> allTeams)
    {
        var homeGames = new Dictionary<int, int>();
        var awayGames = new Dictionary<int, int>();

        foreach (var team in allTeams)
        {
            homeGames[team.Id] = 0;
            awayGames[team.Id] = 0;
        }

        // Count current home/away
        foreach (var week in season.Weeks)
        {
            foreach (var game in week.Games)
            {
                homeGames[game.HomeTeamId]++;
                awayGames[game.AwayTeamId]++;
            }
        }

        // Swap home/away for games where it improves balance
        foreach (var week in season.Weeks)
        {
            foreach (var game in week.Games.ToList())
            {
                var homeTeamHomeCount = homeGames[game.HomeTeamId];
                var homeTeamAwayCount = awayGames[game.HomeTeamId];
                var awayTeamHomeCount = homeGames[game.AwayTeamId];
                var awayTeamAwayCount = awayGames[game.AwayTeamId];

                // Check if swapping improves balance
                var currentImbalance = Math.Abs(homeTeamHomeCount - homeTeamAwayCount) +
                                       Math.Abs(awayTeamHomeCount - awayTeamAwayCount);

                var swappedImbalance = Math.Abs(homeTeamHomeCount - 1 - (homeTeamAwayCount + 1)) +
                                       Math.Abs(awayTeamHomeCount + 1 - (awayTeamAwayCount - 1));

                if (swappedImbalance < currentImbalance)
                {
                    // Swap
                    var temp = game.HomeTeamId;
                    game.HomeTeamId = game.AwayTeamId;
                    game.AwayTeamId = temp;

                    homeGames[game.HomeTeamId]++;
                    homeGames[game.AwayTeamId]--;
                    awayGames[game.HomeTeamId]--;
                    awayGames[game.AwayTeamId]++;
                }
            }
        }
    }

    /// <summary>
    /// Get rotating division pairings based on year
    /// </summary>
    private List<(int, int)> GetRotatingDivisionPairings(int divisionCount, int year)
    {
        var pairings = new List<(int, int)>();

        // Simple rotation - each division plays one other division
        var offset = year % (divisionCount - 1) + 1;

        for (int i = 0; i < divisionCount / 2; i++)
        {
            var div1 = i;
            var div2 = (i + offset) % divisionCount;
            if (div2 == div1) div2 = (div2 + 1) % divisionCount;
            pairings.Add((div1, div2));
        }

        return pairings;
    }

    /// <summary>
    /// Internal class to represent a scheduled matchup before week assignment
    /// </summary>
    private class ScheduledMatchup
    {
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public MatchupType MatchupType { get; set; }
    }

    private enum MatchupType
    {
        Division,
        ConferenceCrossDivision,
        InterConference,
        SamePlaceFinisher
    }
}
