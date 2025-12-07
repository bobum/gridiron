using System;
using System.Collections.Generic;
using System.Linq;
using DomainObjects;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests.Services
{
    public class ScheduleGeneratorServiceTests
    {
        private readonly Mock<ILogger<ScheduleGeneratorService>> _mockLogger;
        private readonly ScheduleGeneratorService _service;

        public ScheduleGeneratorServiceTests()
        {
            _mockLogger = new Mock<ILogger<ScheduleGeneratorService>>();
            _service = new ScheduleGeneratorService(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ScheduleGeneratorService(null));
        }

        [Fact]
        public void GenerateSchedule_NullSeason_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.GenerateSchedule(null));
        }

        [Fact]
        public void GenerateSchedule_NullLeague_ThrowsArgumentException()
        {
            var season = new Season { League = null };
            Assert.Throws<ArgumentException>(() => _service.GenerateSchedule(season));
        }

        [Fact]
        public void GenerateSchedule_TooManyWeeks_ThrowsInvalidOperationException()
        {
            var season = new Season
            {
                League = new League(),
                RegularSeasonWeeks = 100 // Assuming max is less than 100
            };
            Assert.Throws<InvalidOperationException>(() => _service.GenerateSchedule(season));
        }

        [Fact]
        public void ValidateLeagueStructure_NullLeague_ReturnsFailure()
        {
            var result = _service.ValidateLeagueStructure(null);
            Assert.False(result.IsValid);
            Assert.Contains("League cannot be null", result.Errors);
        }

        [Fact]
        public void ValidateLeagueStructure_NoConferences_ReturnsFailure()
        {
            var league = new League { Conferences = new List<Conference>() };
            var result = _service.ValidateLeagueStructure(league);
            Assert.False(result.IsValid);
            Assert.Contains("League must have at least one conference", result.Errors);
        }

        [Fact]
        public void GenerateSchedule_ValidLeague_GeneratesSchedule()
        {
            // Setup a minimal valid league: 2 conferences, 2 divisions each, 4 teams each
            var league = CreateValidLeague();
            var season = new Season
            {
                Id = 1,
                League = league,
                RegularSeasonWeeks = 17,
                Year = 2024,
                Weeks = new List<SeasonWeek>()
            };

            var result = _service.GenerateSchedule(season);

            Assert.NotNull(result);
            Assert.Equal(18, result.Weeks.Count); // 17 weeks + 1 bye week logic (or similar)
            Assert.True(result.Weeks.All(w => w.SeasonId == season.Id));
            // Verify games are generated
            var totalGames = result.Weeks.Sum(w => w.Games.Count);
            Assert.True(totalGames > 0);
        }

        [Fact]
        public void ValidateLeagueStructure_DifferentDivisionCounts_ReturnsFailure()
        {
            var league = new League
            {
                Conferences = new List<Conference>
                {
                    new Conference { Divisions = new List<Division> { new Division(), new Division() } },
                    new Conference { Divisions = new List<Division> { new Division() } }
                }
            };
            var result = _service.ValidateLeagueStructure(league);
            Assert.False(result.IsValid);
            Assert.Contains("All conferences must have the same number of divisions", result.Errors);
        }

        [Fact]
        public void ValidateLeagueStructure_DifferentTeamCounts_ReturnsFailure()
        {
            var league = new League
            {
                Conferences = new List<Conference>
                {
                    new Conference
                    {
                        Divisions = new List<Division>
                        {
                            new Division { Teams = new List<Team> { new Team { Name = "T1" }, new Team { Name = "T2" } } },
                            new Division { Teams = new List<Team> { new Team { Name = "T3" } } }
                        }
                    }
                }
            };
            var result = _service.ValidateLeagueStructure(league);
            Assert.False(result.IsValid);
            Assert.Contains("All divisions must have the same number of teams", result.Errors);
        }

        [Fact]
        public void ValidateLeagueStructure_TooFewTeamsPerDivision_ReturnsFailure()
        {
            var league = new League
            {
                Conferences = new List<Conference>
                {
                    new Conference
                    {
                        Divisions = new List<Division>
                        {
                            new Division { Teams = new List<Team> { new Team { Name = "T1" } } }
                        }
                    }
                }
            };
            var result = _service.ValidateLeagueStructure(league);
            Assert.False(result.IsValid);
            Assert.Contains("Each division must have at least 2 teams", result.Errors);
        }

        private League CreateValidLeague()
        {
            var league = new League
            {
                Id = 1,
                Conferences = new List<Conference>()
            };

            int teamIdCounter = 1;
            for (int c = 0; c < 2; c++)
            {
                var conference = new Conference
                {
                    Id = c + 1,
                    Name = $"Conference {c + 1}",
                    Divisions = new List<Division>()
                };

                for (int d = 0; d < 4; d++) // 4 divisions per conference
                {
                    var division = new Division
                    {
                        Id = (c * 4) + d + 1,
                        Name = $"Division {d + 1}",
                        Teams = new List<Team>()
                    };

                    for (int t = 0; t < 4; t++) // 4 teams per division
                    {
                        division.Teams.Add(new Team
                        {
                            Id = teamIdCounter++,
                            Name = $"Team {teamIdCounter}",
                            DivisionId = division.Id
                        });
                    }

                    conference.Divisions.Add(division);
                }

                league.Conferences.Add(conference);
            }

            return league;
        }
    }
}
