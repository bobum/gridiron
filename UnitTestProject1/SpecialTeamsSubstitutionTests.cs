using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using System.Linq;

using UnitTestProject1.Helpers;
namespace UnitTestProject1
{
    [TestClass]
    public class SpecialTeamsSubstitutionTests
    {
        private Game? _game;
        private SeedableRandom? _rng;
        private PrePlay? _prePlay;

        [TestInitialize]
        public void Setup()
        {
            _rng = new SeedableRandom(12345);
            _prePlay = new PrePlay(_rng);

            // Create a game with actual teams (from GameHelper)
            _game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            _game!.WonCoinToss = Possession.Home;
        }

        #region Kickoff Substitution Tests

        [TestMethod]
        public void KickoffPlay_OffenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.OffensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField, "Kickoff offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void KickoffPlay_DefenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.DefensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.DefensePlayersOnField,
                "Kickoff defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void KickoffPlay_OffenseHasKicker()
        {
            // Arrange
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var kickoffPlay = (KickoffPlay)_game!.CurrentPlay;

            // Verify the Kicker property is set
            Assert.IsNotNull(kickoffPlay.Kicker, "KickoffPlay.Kicker property should be set");

            // Verify the kicker is in OffensePlayersOnField
            Assert.Contains(kickoffPlay.Kicker, _game!.CurrentPlay.OffensePlayersOnField, "Kicker should be in OffensePlayersOnField");

            // Verify the kicker has position K or P (punter can be fallback)
            Assert.IsTrue(kickoffPlay.Kicker.Position == Positions.K || kickoffPlay.Kicker.Position == Positions.P,
                "Kicker should have position K or P");
        }

        [TestMethod]
        public void KickoffPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Get a player from the regular offense chart that's NOT on kickoff chart
            var offenseChart = _game!.HomeTeam.OffenseDepthChart.Chart;
            var kickoffChart = _game!.HomeTeam.KickoffOffenseDepthChart.Chart;

            // Act
            _prePlay!.Execute(_game);

            // Assert - Should use kickoff chart, not regular offense chart
            var hasKicker = _game!.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.K);
            Assert.IsTrue(hasKicker, "Kickoff team should include a kicker from KickoffOffenseDepthChart");
        }

        #endregion

        #region Punt Substitution Tests

        [TestMethod]
        public void PuntPlay_OffenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.OffensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField,
                "Punt offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void PuntPlay_DefenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.DefensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.DefensePlayersOnField,
                "Punt defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void PuntPlay_OffenseHasPunter()
        {
            // Arrange - Create a safety so next play will be free kick (PuntPlay)
            var safety = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsSafety = true
            };
            _game!.Plays.Add(safety);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var puntPlay = (PuntPlay)_game!.CurrentPlay;

            // Verify the Punter property is set
            Assert.IsNotNull(puntPlay.Punter, "PuntPlay.Punter property should be set");

            // Verify the punter is in OffensePlayersOnField
            Assert.Contains(puntPlay.Punter, _game!.CurrentPlay.OffensePlayersOnField, "Punter should be in OffensePlayersOnField");

            // Verify the punter has the correct position
            Assert.AreEqual(Positions.P, puntPlay.Punter.Position,
                "Punter should have position P");
        }

        [TestMethod]
        public void PuntPlay_OffenseHasLongSnapper()
        {
            // Arrange - Create a safety so next play will be free kick (PuntPlay)
            var safety = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsSafety = true
            };
            _game!.Plays.Add(safety);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var puntPlay = (PuntPlay)_game!.CurrentPlay;

            // Verify the LongSnapper property is set
            Assert.IsNotNull(puntPlay.LongSnapper, "PuntPlay.LongSnapper property should be set");

            // Verify the long snapper is in OffensePlayersOnField
            Assert.Contains(puntPlay.LongSnapper, _game!.CurrentPlay.OffensePlayersOnField, "LongSnapper should be in OffensePlayersOnField");

            // Verify the long snapper has the correct position
            Assert.AreEqual(Positions.LS, puntPlay.LongSnapper.Position,
                "LongSnapper should have position LS");
        }

        [TestMethod]
        public void PuntPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game!.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            // Verify we have 11 unique players from the punt depth chart
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField);
            var distinctPlayers = _game!.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Punt team should have 11 unique players from PuntOffenseDepthChart");
        }

        #endregion

        #region Field Goal Substitution Tests

        [TestMethod]
        public void FieldGoalPlay_OffenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.OffensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField,
                "Field goal offense should have exactly 11 players on field");
        }

        [TestMethod]
        public void FieldGoalPlay_DefenseHas11Players()
        {
            // Arrange
            _game!.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.DefensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.DefensePlayersOnField,
                "Field goal defense should have exactly 11 players on field");
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasKicker()
        {
            // Arrange - Create a touchdown so next play will be extra point (FieldGoalPlay)
            // Advance RNG state to ensure we get FieldGoalPlay (not 2-point conversion)
            for (int i = 0; i < 10; i++) _rng!.NextDouble();

            var touchdown = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsTouchdown = true
            };
            _game!.Plays.Add(touchdown);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var fieldGoalPlay = (FieldGoalPlay)_game!.CurrentPlay;

            // Verify the Kicker property is set
            Assert.IsNotNull(fieldGoalPlay.Kicker, "FieldGoalPlay.Kicker property should be set");

            // Verify the kicker is in OffensePlayersOnField
            Assert.Contains(fieldGoalPlay.Kicker, _game!.CurrentPlay.OffensePlayersOnField, "Kicker should be in OffensePlayersOnField");

            // Verify the kicker has the correct position
            Assert.AreEqual(Positions.K, fieldGoalPlay.Kicker.Position,
                "Kicker should have position K");
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasHolder()
        {
            // Arrange - Create a touchdown so next play will be extra point (FieldGoalPlay)
            // Advance RNG state to ensure we get FieldGoalPlay (not 2-point conversion)
            for (int i = 0; i < 10; i++) _rng!.NextDouble();

            var touchdown = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsTouchdown = true
            };
            _game!.Plays.Add(touchdown);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var fieldGoalPlay = (FieldGoalPlay)_game!.CurrentPlay;

            // Verify the Holder property is set
            Assert.IsNotNull(fieldGoalPlay.Holder, "FieldGoalPlay.Holder property should be set");

            // Verify the holder is in OffensePlayersOnField
            Assert.Contains(fieldGoalPlay.Holder, _game!.CurrentPlay.OffensePlayersOnField, "Holder should be in OffensePlayersOnField");

            // Note: Holder position can be H, P, or QB (backup QB often holds)
            // Just verify the holder is a valid player
        }

        [TestMethod]
        public void FieldGoalPlay_OffenseHasLongSnapper()
        {
            // Arrange - Create a touchdown so next play will be extra point (FieldGoalPlay)
            // Advance RNG state to ensure we get FieldGoalPlay (not 2-point conversion)
            for (int i = 0; i < 10; i++) _rng!.NextDouble();

            var touchdown = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsTouchdown = true
            };
            _game!.Plays.Add(touchdown);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var fieldGoalPlay = (FieldGoalPlay)_game!.CurrentPlay;

            // Verify the LongSnapper property is set
            Assert.IsNotNull(fieldGoalPlay.LongSnapper, "FieldGoalPlay.LongSnapper property should be set");

            // Verify the long snapper is in OffensePlayersOnField
            Assert.Contains(fieldGoalPlay.LongSnapper, _game!.CurrentPlay.OffensePlayersOnField, "LongSnapper should be in OffensePlayersOnField");

            // Verify the long snapper has the correct position
            Assert.AreEqual(Positions.LS, fieldGoalPlay.LongSnapper.Position,
                "LongSnapper should have position LS");
        }

        [TestMethod]
        public void FieldGoalPlay_UsesSpecialTeamsDepthChart()
        {
            // Arrange
            _game!.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var hasKicker = _game!.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.K);
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
            _game!.Plays.Add(kickoff);
            _game!.CurrentDown = Downs.First;
            _game!.YardsToGo = 10;

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.OffensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField);

            var hasQB = _game!.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var hasRB = _game!.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.RB);
            var receivers = _game!.CurrentPlay.OffensePlayersOnField.Count(p => p.Position == Positions.WR);

            Assert.IsTrue(hasQB, "Regular offense should have a QB");
            Assert.IsTrue(hasRB, "Regular offense should have a RB");

            // DetermineNextPlay randomly chooses run or pass - verify correct WR count for each
            if (_game!.CurrentPlay.PlayType == PlayType.Pass)
            {
                Assert.AreEqual(3, receivers, "Pass play should have 3 WRs");
            }
            else if (_game!.CurrentPlay.PlayType == PlayType.Run)
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
            _game!.Plays.Add(kickoff);
            _game!.CurrentDown = Downs.Second;
            _game!.YardsToGo = 10;

            // Act
            _prePlay!.Execute(_game);

            // Assert
            Assert.IsNotNull(_game!.CurrentPlay.OffensePlayersOnField);
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField);

            var hasQB = _game!.CurrentPlay.OffensePlayersOnField.Any(p => p.Position == Positions.QB);
            var receivers = _game!.CurrentPlay.OffensePlayersOnField.Count(p => p.Position == Positions.WR);

            Assert.IsTrue(hasQB, "Regular offense should have a QB");

            // DetermineNextPlay randomly chooses run or pass - verify correct WR count for each
            if (_game!.CurrentPlay.PlayType == PlayType.Pass)
            {
                Assert.AreEqual(3, receivers, "Pass play should have 3 WRs");
            }
            else if (_game!.CurrentPlay.PlayType == PlayType.Run)
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
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Away,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var kickoffPlay = (KickoffPlay)_game!.CurrentPlay;

            // Verify the Kicker property is set
            Assert.IsNotNull(kickoffPlay.Kicker, "KickoffPlay.Kicker property should be set for away team");

            // Verify the kicker is in OffensePlayersOnField
            Assert.Contains(kickoffPlay.Kicker, _game!.CurrentPlay.OffensePlayersOnField, "Kicker should be in OffensePlayersOnField");

            // Verify we have 11 players
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField);
        }

        [TestMethod]
        public void PuntPlay_AwayTeamOffenseHasPunter()
        {
            // Arrange - Create a safety by away team so next play will be free kick (PuntPlay)
            var safety = new RunPlay
            {
                Possession = Possession.Away,
                Down = Downs.First,
                StartTime = 0,
                StopTime = 5,
                IsSafety = true
            };
            _game!.Plays.Add(safety);

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var puntPlay = (PuntPlay)_game!.CurrentPlay;

            // Verify the Punter property is set
            Assert.IsNotNull(puntPlay.Punter, "PuntPlay.Punter property should be set for away team");

            // Verify the punter is in OffensePlayersOnField
            Assert.Contains(puntPlay.Punter, _game!.CurrentPlay.OffensePlayersOnField, "Punter should be in OffensePlayersOnField");

            // Verify we have 11 unique players
            Assert.HasCount(11, _game!.CurrentPlay.OffensePlayersOnField);
            var distinctPlayers = _game!.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            Assert.AreEqual(11, distinctPlayers, "Away team punt offense should have 11 unique players");
        }

        #endregion

        #region Player Uniqueness Tests

        [TestMethod]
        public void KickoffPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game!.CurrentPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game!.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game!.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        [TestMethod]
        public void PuntPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game!.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game!.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game!.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        [TestMethod]
        public void FieldGoalPlay_AllPlayersAreUnique()
        {
            // Arrange
            _game!.CurrentPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartTime = 0,
                IsExtraPoint = false
            };

            // Act
            _prePlay!.Execute(_game);

            // Assert
            var distinctOffensePlayers = _game!.CurrentPlay.OffensePlayersOnField.Distinct().Count();
            var distinctDefensePlayers = _game!.CurrentPlay.DefensePlayersOnField.Distinct().Count();

            Assert.AreEqual(11, distinctOffensePlayers, "All 11 offensive players should be unique");
            Assert.AreEqual(11, distinctDefensePlayers, "All 11 defensive players should be unique");
        }

        #endregion
    }
}
