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
            var punter = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.P);

            Assert.IsNotNull(punter, "Punt offense must have a punter (P)");
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
            var longSnapper = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.C);

            Assert.IsNotNull(longSnapper, "Punt offense must have a long snapper (C)");
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
            var hasPunter = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.P);
            Assert.IsTrue(hasPunter, "Punt team should include a punter from PuntOffenseDepthChart");
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
            var holder = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.P);

            Assert.IsNotNull(holder, "Field goal unit must have a holder (P)");
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
            var longSnapper = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.C);

            Assert.IsNotNull(longSnapper, "Field goal unit must have a long snapper (C)");
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
            _game.CurrentPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                ElapsedTime = 1.5
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);

            var hasQB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var hasRB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.RB);

            Assert.IsTrue(hasQB, "Run play should have a QB");
            Assert.IsTrue(hasRB, "Run play should have a RB");
        }

        [TestMethod]
        public void PassPlay_StillSubstitutesCorrectly()
        {
            // Arrange
            _game.CurrentPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second,
                ElapsedTime = 1.5
            };

            // Act
            _prePlay.Execute(_game);

            // Assert
            Assert.IsNotNull(_game.CurrentPlay.OffensePlayersOnField);
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);

            var hasQB = _game.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var receivers = _game.CurrentPlay.OffensePlayersOnField.Count(p => p.Position == Positions.WR);

            Assert.IsTrue(hasQB, "Pass play should have a QB");
            Assert.AreEqual(3, receivers, "Pass play should have 3 WRs");
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
            var punter = _game.CurrentPlay.OffensePlayersOnField
                .FirstOrDefault(p => p.Position == Positions.P);

            Assert.IsNotNull(punter, "Away team punt offense must have a punter");
            Assert.AreEqual(11, _game.CurrentPlay.OffensePlayersOnField.Count);
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
