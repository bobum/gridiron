using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class GameFieldPositionTests
    {
        private Game _game;

        [TestInitialize]
        public void Setup()
        {
            _game = new Game
            {
                HomeTeam = new Team
                {
                    City = "Buffalo",
                    Name = "Bills"
                },
                AwayTeam = new Team
                {
                    City = "Kansas City",
                    Name = "Chiefs"
                }
            };
        }

        #region GetOffensiveTeam Tests

        [TestMethod]
        public void GetOffensiveTeam_HomePossession_ReturnsHomeTeam()
        {
            var team = _game.GetOffensiveTeam(Possession.Home);
            Assert.AreEqual("Buffalo", team.City);
            Assert.AreEqual("Bills", team.Name);
        }

        [TestMethod]
        public void GetOffensiveTeam_AwayPossession_ReturnsAwayTeam()
        {
            var team = _game.GetOffensiveTeam(Possession.Away);
            Assert.AreEqual("Kansas City", team.City);
            Assert.AreEqual("Chiefs", team.Name);
        }

        #endregion

        #region GetDefensiveTeam Tests

        [TestMethod]
        public void GetDefensiveTeam_HomePossession_ReturnsAwayTeam()
        {
            var team = _game.GetDefensiveTeam(Possession.Home);
            Assert.AreEqual("Kansas City", team.City);
            Assert.AreEqual("Chiefs", team.Name);
        }

        [TestMethod]
        public void GetDefensiveTeam_AwayPossession_ReturnsHomeTeam()
        {
            var team = _game.GetDefensiveTeam(Possession.Away);
            Assert.AreEqual("Buffalo", team.City);
            Assert.AreEqual("Bills", team.Name);
        }

        #endregion

        #region FormatFieldPosition Tests

        [TestMethod]
        public void FormatFieldPosition_HomePossession_OwnSide_ReturnsCorrectFormat()
        {
            _game.FieldPosition = 20;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Buffalo 20", result);
        }

        [TestMethod]
        public void FormatFieldPosition_HomePossession_OpponentSide_ReturnsCorrectFormat()
        {
            _game.FieldPosition = 80;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Kansas City 20", result);
        }

        [TestMethod]
        public void FormatFieldPosition_AwayPossession_OwnSide_ReturnsCorrectFormat()
        {
            _game.FieldPosition = 35;
            var result = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Kansas City 35", result);
        }

        [TestMethod]
        public void FormatFieldPosition_AwayPossession_OpponentSide_ReturnsCorrectFormat()
        {
            _game.FieldPosition = 75;
            var result = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Buffalo 25", result);
        }

        [TestMethod]
        public void FormatFieldPosition_Midfield_Returns50()
        {
            _game.FieldPosition = 50;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("50", result);
        }

        [TestMethod]
        public void FormatFieldPosition_SpecificPosition_HomePossession_ReturnsCorrectFormat()
        {
            // Test the overload that takes a specific field position
            var result = _game.FormatFieldPosition(97, Possession.Home);
            Assert.AreEqual("Kansas City 3", result);
        }

        [TestMethod]
        public void FormatFieldPosition_SpecificPosition_AwayPossession_ReturnsCorrectFormat()
        {
            // Test the overload that takes a specific field position
            var result = _game.FormatFieldPosition(60, Possession.Away);
            Assert.AreEqual("Buffalo 40", result);
        }

        #endregion

        #region FormatFieldPositionWithYardLine Tests

        [TestMethod]
        public void FormatFieldPositionWithYardLine_HomePossession_IncludesSuffix()
        {
            _game.FieldPosition = 20;
            var result = _game.FormatFieldPositionWithYardLine(Possession.Home);
            Assert.AreEqual("Buffalo 20 yard line", result);
        }

        [TestMethod]
        public void FormatFieldPositionWithYardLine_OpponentSide_IncludesSuffix()
        {
            _game.FieldPosition = 80;
            var result = _game.FormatFieldPositionWithYardLine(Possession.Home);
            Assert.AreEqual("Kansas City 20 yard line", result);
        }

        [TestMethod]
        public void FormatFieldPositionWithYardLine_Midfield_IncludesMidfieldNote()
        {
            _game.FieldPosition = 50;
            var result = _game.FormatFieldPositionWithYardLine(Possession.Home);
            Assert.AreEqual("50 yard line (midfield)", result);
        }

        #endregion

        #region Integration Tests - Real Game Scenarios

        [TestMethod]
        public void Scenario_HomeDriveStartsAt25_CorrectFormat()
        {
            // Home team receives kickoff, starts at their own 25
            _game.FieldPosition = 25;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Buffalo 25", result);
        }

        [TestMethod]
        public void Scenario_HomeInRedZone_CorrectFormat()
        {
            // Home team drives to opponent's 15
            _game.FieldPosition = 85;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Kansas City 15", result);
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(85));
        }

        [TestMethod]
        public void Scenario_AwayGoalToGo_CorrectFormat()
        {
            // Away team has goal-to-go at opponent's 5
            _game.FieldPosition = 95;
            var result = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Buffalo 5", result);
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(95));
        }

        [TestMethod]
        public void Scenario_HomeBackedUpInOwnTerritory_CorrectFormat()
        {
            // Home team backed up at their own 2
            _game.FieldPosition = 2;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Buffalo 2", result);
        }

        [TestMethod]
        public void Scenario_AwayCrossesMidfield_CorrectFormat()
        {
            // Away team just crossed midfield to opponent's 48
            _game.FieldPosition = 52;
            var result = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Buffalo 48", result);
        }

        [TestMethod]
        public void Scenario_HomeAtOpponentOneYardLine_CorrectFormat()
        {
            // Home team at opponent's 1-yard line, about to score
            _game.FieldPosition = 99;
            var result = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Kansas City 1", result);
        }

        #endregion

        #region Possession Change Scenarios

        [TestMethod]
        public void Scenario_TurnoverChangesFieldPosition_CorrectFormatForNewPossession()
        {
            // Home team fumbles at their own 30
            _game.FieldPosition = 30;
            var beforeTurnover = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Buffalo 30", beforeTurnover);

            // After turnover, away team now has ball at same spot
            // But from away team's perspective, it's now at Buffalo 30 (opponent's territory)
            var afterTurnover = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Buffalo 30", afterTurnover);
        }

        [TestMethod]
        public void Scenario_InterceptionReturn_FieldPositionFromDefenderPerspective()
        {
            // Home throws INT at opponent's 40 (position 60)
            _game.FieldPosition = 60;
            var homeView = _game.FormatFieldPosition(Possession.Home);
            Assert.AreEqual("Kansas City 40", homeView);

            // Away team (defender) intercepts - same physical spot
            // From defender's perspective, it's now their own 40
            var awayView = _game.FormatFieldPosition(Possession.Away);
            Assert.AreEqual("Buffalo 40", awayView);
        }

        #endregion
    }
}
