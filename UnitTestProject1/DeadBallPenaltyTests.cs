using DomainObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Services;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests for dead ball (pre-snap) penalties that prevent play execution.
    /// Verifies that PenaltyEnforcement correctly identifies which penalties are dead ball fouls.
    ///
    /// Dead ball fouls occur before the snap and prevent the play from executing.
    /// Per NFL rules and GameFlow.cs implementation (lines 275-326), these penalties:
    /// - Cause the play to abort without being run
    /// - Are enforced immediately
    /// - Down is replayed (or advanced in rare cases)
    /// - Cannot result in a touchdown or safety
    /// </summary>
    [TestClass]
    public class DeadBallPenaltyTests
    {
        private PenaltyEnforcement? _penaltyEnforcement;

        [TestInitialize]
        public void Setup()
        {
            _penaltyEnforcement = new PenaltyEnforcement(NullLogger.Instance);
        }

        #region Dead Ball Foul Identification Tests

        [TestMethod]
        public void FalseStart_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.FalseStart);

            // Assert
            Assert.IsTrue(isDeadBall, "FalseStart should be a dead ball foul");
        }

        [TestMethod]
        public void Encroachment_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.Encroachment);

            // Assert
            Assert.IsTrue(isDeadBall, "Encroachment should be a dead ball foul");
        }

        [TestMethod]
        public void DelayOfGame_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.DelayofGame);

            // Assert
            Assert.IsTrue(isDeadBall, "DelayofGame should be a dead ball foul");
        }

        [TestMethod]
        public void DefensiveDelayOfGame_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.DefensiveDelayofGame);

            // Assert
            Assert.IsTrue(isDeadBall, "DefensiveDelayofGame should be a dead ball foul");
        }

        [TestMethod]
        public void Offensive12OnField_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.Offensive12OnField);

            // Assert
            Assert.IsTrue(isDeadBall, "Offensive12OnField should be a dead ball foul");
        }

        [TestMethod]
        public void Defensive12OnField_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.Defensive12OnField);

            // Assert
            Assert.IsTrue(isDeadBall, "Defensive12OnField should be a dead ball foul");
        }

        [TestMethod]
        public void IllegalSubstitution_IsDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.IllegalSubstitution);

            // Assert
            Assert.IsTrue(isDeadBall, "IllegalSubstitution should be a dead ball foul");
        }

        #endregion

        #region Non-Dead Ball Foul Tests (Negative Tests)

        [TestMethod]
        public void OffensiveHolding_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.OffensiveHolding);

            // Assert
            Assert.IsFalse(isDeadBall, "OffensiveHolding occurs during the play, not before");
        }

        [TestMethod]
        public void DefensiveOffside_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.DefensiveOffside);

            // Assert
            Assert.IsFalse(isDeadBall, "DefensiveOffside allows a 'free play' to continue");
        }

        [TestMethod]
        public void DefensiveHolding_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.DefensiveHolding);

            // Assert
            Assert.IsFalse(isDeadBall, "DefensiveHolding occurs during the play");
        }

        [TestMethod]
        public void DefensivePassInterference_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.DefensivePassInterference);

            // Assert
            Assert.IsFalse(isDeadBall, "DPI occurs during the play");
        }

        [TestMethod]
        public void RoughingThePasser_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.RoughingthePasser);

            // Assert
            Assert.IsFalse(isDeadBall, "Roughing the passer occurs after the snap");
        }

        [TestMethod]
        public void NeutralZoneInfraction_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.NeutralZoneInfraction);

            // Assert
            Assert.IsFalse(isDeadBall, "Neutral zone infraction allows play to continue");
        }

        [TestMethod]
        public void IntentionalGrounding_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.IntentionalGrounding);

            // Assert
            Assert.IsFalse(isDeadBall, "Intentional grounding occurs during the play");
        }

        [TestMethod]
        public void UnnecessaryRoughness_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.UnnecessaryRoughness);

            // Assert
            Assert.IsFalse(isDeadBall, "Unnecessary roughness can occur during or after the play");
        }

        [TestMethod]
        public void FaceMask_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.FaceMask15Yards);

            // Assert
            Assert.IsFalse(isDeadBall, "Facemask occurs during the play");
        }

        [TestMethod]
        public void IllegalContact_IsNotDeadBallFoul()
        {
            // Act
            var isDeadBall = _penaltyEnforcement!.IsDeadBallFoul(PenaltyNames.IllegalContact);

            // Assert
            Assert.IsFalse(isDeadBall, "Illegal contact occurs during the play");
        }

        #endregion
    }
}
