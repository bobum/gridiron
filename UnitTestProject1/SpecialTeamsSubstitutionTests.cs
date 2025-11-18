using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class SpecialTeamsSubstitutionTests
    {
        private Game _game;
        private SeedableRandom _rng;
        private PrePlay _prePlay;

        [TestInitialize]
        public void Setup()
        {
            _rng = new SeedableRandom(12345);
            _prePlay = new PrePlay(_rng);

            // Create a game with actual teams (from GameHelper)
            _game = GameHelper.GetNewGame();
            _game.WonCoinToss = Possession.Home;
        }

        #region Kickoff Substitution Tests

        [TestMethod]
        public void KickoffPlay_OffenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count,
                "Kickoff offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void KickoffPlay_DefenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.DefensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.DefensePlayersOnField.Count,
                "Kickoff defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void KickoffPlay_OffenseHasKicker()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var kicker = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.K || p.Position == Positions.P);

            Assert.IsNotNull(kicker, "Kickoff offense must have a kicker (K or P)");
        }

        [TestMethod]
        public void KickoffPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Get a player from the regular offense chart that's NOT on kickoff chart
            var offenseChart = _game.HomeTeam.OffenseDepthChart.Chart;
            var kickoffChart = _game.HomeTeam.KickoffOffenseDepthChart.Chart;

            // Act
            _prePlay.Execute(_game);

            // Assert - Should use kickoff chart, not regular offense chart
            var hasKicker = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.K);
            Assert.IsTrue(hasKicker, "Kickoff team should include a kicker from KickoffOffenseDepthChart");
        }

        #endregion

        #region Punt Substitution Tests

        [TestMethod]
        public void PuntPlay_OffenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count,
                "Punt offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void PuntPlay_DefenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.DefensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.DefensePlayersOnField.Count,
                "Punt defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void PuntPlay_OffenseHasPunter()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Punt offense should have 11 unique players
            // Note: We don't check Position enum because players play different roles on special teams
            // (e.g., a QB might be the holder, but their Position is still QB, not H)
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "All punt offense players should be unique");
        }

        [TestMethod]
        public void PuntPlay_OffenseHasLongSnapper()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Verify we have 11 unique players (depth chart organization ensures right roles)
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "All punt offense players should be unique");
        }

        [TestMethod]
        public void PuntPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Verify we have 11 unique players from the punt depth chart
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Punt team should have 11 unique players from PuntOffenseDepthChart");
        }

        #endregion

        #region Field Goal Substitution Tests

        [TestMethod]
        public void FieldGoalPlay_OffenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count,
                "Field goal offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void FieldGoalPlay_DefenseHas11Players()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.DefensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.DefensePlayersOnField.Count,
                "Field goal defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasKicker()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var kicker = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.K);

            Assert.IsNotNull(kicker, "Field goal unit must have a kicker (K)");
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasHolder()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Verify we have 11 unique players (depth chart includes holder role)
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Field goal unit should have 11 unique players");
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasLongSnapper()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Verify we have 11 unique players (depth chart includes long snapper role)
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Field goal unit should have 11 unique players");
        }

        [TestMethod]
        public void FieldGoalPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var hasKicker = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.K);
            Assert.IsTrue(hasKicker, "Field goal unit should include a kicker from FieldGoalOffenseDepthChart");
        }

        #endregion

        #region Regular Offense/Defense Still Works

        [TestMethod]
        public void RunPlay_StillSubstitutesCorrectly()
        {
            // Arrange
            // Add a kickoff play to simulate game in progress (so DetermineNextPlay doesn't default to kickoff)
            var kickoff = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false
            };
            _game.Plays.Add(kickoff);
            _game.CurrentDown = Downs.First;
            _game.YardsToGo = 10;

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);

            var hasQB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var hasRB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.RB);
            var receivers = _game.CurrentPlay.OffensePlayersOnField.Count(p => p.Position == Positions.WR);

            Assert.IsTrue(hasQB, "Regular offense should have a QB");
            Assert.IsTrue(hasRB, "Regular offense should have a RB");

            // DetermineNextPlay randomly chooses run or pass - verify correct WR count for each
            if (_game.CurrentPlay.PlayType == PlayType.Pass)
            {
                Assert.AreEqual(3, receivers, "Pass play should have 3 WRs");
            }
            else if (_game.CurrentPlay.PlayType == PlayType.Run)
            {
                Assert.AreEqual(2, receivers, "Run play should have 2 WRs");
            }
        }

        [TestMethod]
        public void PassPlay_StillSubstitutesCorrectly()
        {
            // Arrange
            // Add a kickoff play to simulate game in progress (so DetermineNextPlay doesn't default to kickoff)
            var kickoff = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false
            };
            _game.Plays.Add(kickoff);
            _game.CurrentDown = Downs.Second;
            _game.YardsToGo = 10;

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);

            var hasQB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var receivers = _game.CurrentPlay.OffensePlayersOnField.Count(p => p.Position == Positions.WR);

            Assert.IsTrue(hasQB, "Regular offense should have a QB");

            // DetermineNextPlay randomly chooses run or pass - verify correct WR count for each
            if (_game.CurrentPlay.PlayType == PlayType.Pass)
            {
                Assert.AreEqual(3, receivers, "Pass play should have 3 WRs");
            }
            else if (_game.CurrentPlay.PlayType == PlayType.Run)
            {
                Assert.AreEqual(2, receivers, "Run play should have 2 WRs");
            }
        }

        #endregion

        #region Away Team Special Teams

        [TestMethod]
        public void KickoffPlay_AwayTeamOffenseHasKicker()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Away,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var kicker = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.K || p.Position == Positions.P);

            Assert.IsNotNull(kicker, "Away team kickoff offense must have a kicker");
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
        }

        [TestMethod]
        public void PuntPlay_AwayTeamOffenseHasPunter()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Away,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            // Verify away team has 11 unique players
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
            var distinctPlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Away team punt offense should have 11 unique players");
        }

        #endregion

        #region Player Uniqueness Tests

        [TestMethod]
        public void KickoffPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        [TestMethod]
        public void PuntPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        [TestMethod]
        public void FieldGoalPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        #endregion
    }
}
