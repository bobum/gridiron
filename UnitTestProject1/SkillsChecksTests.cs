using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsChecks;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class SkillsChecksTests
    {
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void FieldGoalBlockOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 1}};

            var blockResult = new FieldGoalBlockOccurredSkillsCheck(rng);
            blockResult.Execute(game);

            Assert.IsTrue(blockResult.Occurred);
        }

        [TestMethod]
        public void FieldGoalBlockOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 2}};

            var blockResult = new FieldGoalBlockOccurredSkillsCheck(rng);
            blockResult.Execute(game);

            Assert.IsFalse(blockResult.Occurred);
        }

        [TestMethod]
        public void FumbleOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 2}};

            var fumbleResult = new FumbleOccurredSkillsCheck(rng);
            fumbleResult.Execute(game);

            Assert.IsFalse(fumbleResult.Occurred);
        }

        [TestMethod]
        public void FumbleOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 1}};

            var fumbleResult = new FumbleOccurredSkillsCheck(rng);
            fumbleResult.Execute(game);

            Assert.IsTrue(fumbleResult.Occurred);
        }

        [TestMethod]
        public void InterceptionOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 1}};

            var interceptionResult = new InterceptionOccurredSkillsCheck(rng);
            interceptionResult.Execute(game);

            Assert.IsTrue(interceptionResult.Occurred);
        }

        [TestMethod]
        public void InterceptionOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 2}};

            var interceptionResult = new InterceptionOccurredSkillsCheck(rng);
            interceptionResult.Execute(game);

            Assert.IsFalse(interceptionResult.Occurred);
        }

        [TestMethod]
        public void PenaltyOccurredSkillsCheckKickoffTrueOnAwayTeamTest()
        {
            var game = _testGame.GetGame();

            var playerIndex = 2;
            var penaltyOccurredWhen = PenaltyOccuredWhen.During;
            TestSeedableRandom rng = new TestSeedableRandom
                {__NextInt = {[0] = playerIndex}, __NextDouble = {[0] = 0.0032, [1] = 0.46}};

            var penaltyCheck = new PenaltyOccurredSkillsCheck(penaltyOccurredWhen, rng);
            penaltyCheck.Execute(game);

            Assert.IsTrue(penaltyCheck.Occurred);
            Assert.IsNotNull(penaltyCheck.Penalty);
            Assert.AreEqual(penaltyOccurredWhen, penaltyCheck.Penalty.OccuredWhen);
            Assert.AreEqual(Possession.Away, penaltyCheck.Penalty.CalledOn);
            Assert.AreEqual(game.AwayTeam.Players[playerIndex].Number, penaltyCheck.Penalty.Player.Number);
            Assert.AreEqual(PenaltyNames.IllegalBlockAbovetheWaist, penaltyCheck.Penalty.Name);
        }

        [TestMethod]
        public void PenaltyOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();

            TestSeedableRandom rng = new TestSeedableRandom
                {__NextDouble = {[0] = 0.0034}};

            var penaltyCheck = new PenaltyOccurredSkillsCheck(PenaltyOccuredWhen.During, rng);
            penaltyCheck.Execute(game);

            Assert.IsFalse(penaltyCheck.Occurred);
            Assert.IsNull(penaltyCheck.Penalty);
        }

        [TestMethod]
        public void PuntOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 2}};

            var puntResult = new PuntBlockOccurredSkillsCheck(rng);
            puntResult.Execute(game);

            Assert.IsFalse(puntResult.Occurred);
        }

        [TestMethod]
        public void PuntOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 1}};

            var puntResult = new PuntBlockOccurredSkillsCheck(rng);
            puntResult.Execute(game);

            Assert.IsTrue(puntResult.Occurred);
        }
    }
}