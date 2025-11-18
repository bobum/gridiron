using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class FieldPositionHelperTests
    {
        private Team _buffaloTeam;
        private Team _kansasCityTeam;

        [TestInitialize]
        public void Setup()
        {
            _buffaloTeam = new Team
            {
                City = "Buffalo",
                Name = "Bills"
            };

            _kansasCityTeam = new Team
            {
                City = "Kansas City",
                Name = "Chiefs"
            };
        }

        #region FormatFieldPosition Tests

        [TestMethod]
        public void FormatFieldPosition_OwnGoalLine_ReturnsCorrectFormat()
        {
            // Position 0 = Own goal line
            var result = FieldPositionHelper.FormatFieldPosition(0, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 0", result);
        }

        [TestMethod]
        public void FormatFieldPosition_Midfield_Returns50()
        {
            // Position 50 = Midfield
            var result = FieldPositionHelper.FormatFieldPosition(50, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("50", result);
        }

        [TestMethod]
        public void FormatFieldPosition_OpponentGoalLine_ReturnsCorrectFormat()
        {
            // Position 100 = Opponent's goal line (0 yards from their goal)
            var result = FieldPositionHelper.FormatFieldPosition(100, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 0", result);
        }

        [TestMethod]
        public void FormatFieldPosition_OwnSide_ReturnsOffenseTeamName()
        {
            // Position 20 = Own 20-yard line
            var result = FieldPositionHelper.FormatFieldPosition(20, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 20", result);
        }

        [TestMethod]
        public void FormatFieldPosition_OpponentSide_ReturnsDefenseTeamName()
        {
            // Position 60 = Opponent's 40-yard line (100 - 60 = 40)
            var result = FieldPositionHelper.FormatFieldPosition(60, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 40", result);
        }

        [TestMethod]
        public void FormatFieldPosition_RedZone_ReturnsCorrectFormat()
        {
            // Position 80 = Opponent's 20-yard line (red zone)
            var result = FieldPositionHelper.FormatFieldPosition(80, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 20", result);
        }

        [TestMethod]
        public void FormatFieldPosition_NearOpponentGoal_ReturnsCorrectFormat()
        {
            // Position 97 = Opponent's 3-yard line
            var result = FieldPositionHelper.FormatFieldPosition(97, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 3", result);
        }

        [TestMethod]
        public void FormatFieldPosition_OwnOnYardLine_ReturnsCorrectFormat()
        {
            // Position 1 = Own 1-yard line
            var result = FieldPositionHelper.FormatFieldPosition(1, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 1", result);
        }

        [TestMethod]
        public void FormatFieldPosition_NearMidfield_OwnSide_ReturnsCorrectFormat()
        {
            // Position 45 = Own 45-yard line
            var result = FieldPositionHelper.FormatFieldPosition(45, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 45", result);
        }

        [TestMethod]
        public void FormatFieldPosition_NearMidfield_OpponentSide_ReturnsCorrectFormat()
        {
            // Position 55 = Opponent's 45-yard line
            var result = FieldPositionHelper.FormatFieldPosition(55, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 45", result);
        }

        [TestMethod]
        public void FormatFieldPosition_WithoutTeams_UsesGenericFormat()
        {
            // Without team objects, should use "Own" and "Opp"
            var result1 = FieldPositionHelper.FormatFieldPosition(20, null, null);
            Assert.AreEqual("Own 20", result1);

            var result2 = FieldPositionHelper.FormatFieldPosition(60, null, null);
            Assert.AreEqual("Opp 40", result2);
        }

        [TestMethod]
        public void FormatFieldPosition_BelowZero_ClampsToZero()
        {
            var result = FieldPositionHelper.FormatFieldPosition(-5, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 0", result);
        }

        [TestMethod]
        public void FormatFieldPosition_AboveHundred_ClampsToHundred()
        {
            var result = FieldPositionHelper.FormatFieldPosition(105, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 0", result);
        }

        #endregion

        #region FormatFieldPositionWithYardLine Tests

        [TestMethod]
        public void FormatFieldPositionWithYardLine_OwnSide_IncludesYardLineSuffix()
        {
            var result = FieldPositionHelper.FormatFieldPositionWithYardLine(20, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 20 yard line", result);
        }

        [TestMethod]
        public void FormatFieldPositionWithYardLine_OpponentSide_IncludesYardLineSuffix()
        {
            var result = FieldPositionHelper.FormatFieldPositionWithYardLine(80, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 20 yard line", result);
        }

        [TestMethod]
        public void FormatFieldPositionWithYardLine_Midfield_IncludesMidfieldNote()
        {
            var result = FieldPositionHelper.FormatFieldPositionWithYardLine(50, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("50 yard line (midfield)", result);
        }

        #endregion

        #region GetFieldSide Tests

        [TestMethod]
        public void GetFieldSide_OwnSide_ReturnsOwn()
        {
            Assert.AreEqual("Own", FieldPositionHelper.GetFieldSide(0));
            Assert.AreEqual("Own", FieldPositionHelper.GetFieldSide(1));
            Assert.AreEqual("Own", FieldPositionHelper.GetFieldSide(25));
            Assert.AreEqual("Own", FieldPositionHelper.GetFieldSide(49));
        }

        [TestMethod]
        public void GetFieldSide_Midfield_ReturnsMidfield()
        {
            Assert.AreEqual("Midfield", FieldPositionHelper.GetFieldSide(50));
        }

        [TestMethod]
        public void GetFieldSide_OpponentSide_ReturnsOpponent()
        {
            Assert.AreEqual("Opponent", FieldPositionHelper.GetFieldSide(51));
            Assert.AreEqual("Opponent", FieldPositionHelper.GetFieldSide(75));
            Assert.AreEqual("Opponent", FieldPositionHelper.GetFieldSide(99));
            Assert.AreEqual("Opponent", FieldPositionHelper.GetFieldSide(100));
        }

        #endregion

        #region GetYardLine Tests

        [TestMethod]
        public void GetYardLine_OwnSide_ReturnsActualYardLine()
        {
            Assert.AreEqual(0, FieldPositionHelper.GetYardLine(0));
            Assert.AreEqual(20, FieldPositionHelper.GetYardLine(20));
            Assert.AreEqual(45, FieldPositionHelper.GetYardLine(45));
            Assert.AreEqual(50, FieldPositionHelper.GetYardLine(50));
        }

        [TestMethod]
        public void GetYardLine_OpponentSide_ReturnsDistanceFromOpponentGoal()
        {
            Assert.AreEqual(49, FieldPositionHelper.GetYardLine(51));
            Assert.AreEqual(40, FieldPositionHelper.GetYardLine(60));
            Assert.AreEqual(20, FieldPositionHelper.GetYardLine(80));
            Assert.AreEqual(3, FieldPositionHelper.GetYardLine(97));
            Assert.AreEqual(0, FieldPositionHelper.GetYardLine(100));
        }

        #endregion

        #region IsInRedZone Tests

        [TestMethod]
        public void IsInRedZone_BelowRedZone_ReturnsFalse()
        {
            Assert.IsFalse(FieldPositionHelper.IsInRedZone(0));
            Assert.IsFalse(FieldPositionHelper.IsInRedZone(50));
            Assert.IsFalse(FieldPositionHelper.IsInRedZone(79));
        }

        [TestMethod]
        public void IsInRedZone_AtRedZoneBoundary_ReturnsTrue()
        {
            // Red zone starts at opponent's 20 (position 80)
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(80));
        }

        [TestMethod]
        public void IsInRedZone_InRedZone_ReturnsTrue()
        {
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(80));
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(85));
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(90));
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(95));
            Assert.IsTrue(FieldPositionHelper.IsInRedZone(100));
        }

        #endregion

        #region IsGoalToGo Tests

        [TestMethod]
        public void IsGoalToGo_OutsideGoalToGo_ReturnsFalse()
        {
            Assert.IsFalse(FieldPositionHelper.IsGoalToGo(0));
            Assert.IsFalse(FieldPositionHelper.IsGoalToGo(50));
            Assert.IsFalse(FieldPositionHelper.IsGoalToGo(80));
            Assert.IsFalse(FieldPositionHelper.IsGoalToGo(89));
        }

        [TestMethod]
        public void IsGoalToGo_AtBoundary_ReturnsTrue()
        {
            // Goal-to-go starts at opponent's 10 (position 90)
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(90));
        }

        [TestMethod]
        public void IsGoalToGo_InsideGoalToGo_ReturnsTrue()
        {
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(90));
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(95));
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(99));
            Assert.IsTrue(FieldPositionHelper.IsGoalToGo(100));
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void FormatFieldPosition_Boundary49_OwnSide()
        {
            // Position 49 should be on own side
            var result = FieldPositionHelper.FormatFieldPosition(49, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Buffalo 49", result);
        }

        [TestMethod]
        public void FormatFieldPosition_Boundary51_OpponentSide()
        {
            // Position 51 should be on opponent's side (49 yards from goal)
            var result = FieldPositionHelper.FormatFieldPosition(51, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 49", result);
        }

        [TestMethod]
        public void FormatFieldPosition_Position99_OneYardLine()
        {
            // Position 99 = Opponent's 1-yard line
            var result = FieldPositionHelper.FormatFieldPosition(99, _buffaloTeam, _kansasCityTeam);
            Assert.AreEqual("Kansas City 1", result);
        }

        #endregion
    }
}
