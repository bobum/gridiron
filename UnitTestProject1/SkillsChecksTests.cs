using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsChecks;
using System.Collections.Generic;
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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01); // Block occurs (< 2.5% for medium FG)

            // Create test players
            var kicker = new Player { Kicking = 70, LastName = "Kicker" };
            var offensiveLine = new List<Player>
            {
                new Player { Position = Positions.T, Strength = 70, Awareness = 65 },
                new Player { Position = Positions.G, Strength = 70, Awareness = 65 }
            };
            var defensiveRushers = new List<Player>
            {
                new Player { Position = Positions.DT, Strength = 80, Speed = 75 }
            };

            var blockResult = new FieldGoalBlockOccurredSkillsCheck(rng, kicker, 47, offensiveLine, defensiveRushers, true);
            blockResult.Execute(game);

            Assert.IsTrue(blockResult.Occurred);
        }

        [TestMethod]
        public void FieldGoalBlockOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99); // Block does not occur (> 2.5% for medium FG)

            // Create test players
            var kicker = new Player { Kicking = 70, LastName = "Kicker" };
            var offensiveLine = new List<Player>
            {
                new Player { Position = Positions.T, Strength = 70, Awareness = 65 },
                new Player { Position = Positions.G, Strength = 70, Awareness = 65 }
            };
            var defensiveRushers = new List<Player>
            {
                new Player { Position = Positions.DT, Strength = 80, Speed = 75 }
            };

            var blockResult = new FieldGoalBlockOccurredSkillsCheck(rng, kicker, 47, offensiveLine, defensiveRushers, true);
            blockResult.Execute(game);

            Assert.IsFalse(blockResult.Occurred);
        }

        [TestMethod]
        public void FumbleOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(2); // Fumble does not occur when value != 1

            var fumbleResult = new FumbleOccurredSkillsCheck(rng);
            fumbleResult.Execute(game);

            Assert.IsFalse(fumbleResult.Occurred);
        }

        [TestMethod]
        public void FumbleOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(1); // Fumble occurs when value == 1

            var fumbleResult = new FumbleOccurredSkillsCheck(rng);
            fumbleResult.Execute(game);

            Assert.IsTrue(fumbleResult.Occurred);
        }

        [TestMethod]
        public void InterceptionOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(1); // Interception occurs when value == 1

            var interceptionResult = new InterceptionOccurredSkillsCheck(rng);
            interceptionResult.Execute(game);

            Assert.IsTrue(interceptionResult.Occurred);
        }

        [TestMethod]
        public void InterceptionOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextInt(2); // Interception does not occur when value != 1

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.0032) // Penalty occurs check (< 0.0033)
                .NextDouble(0.46)   // Team determination (< 0.5 = Away)
                .NextInt(playerIndex); // Player selection

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

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.0034); // Penalty does not occur (>= 0.0033)

            var penaltyCheck = new PenaltyOccurredSkillsCheck(PenaltyOccuredWhen.During, rng);
            penaltyCheck.Execute(game);

            Assert.IsFalse(penaltyCheck.Occurred);
            Assert.IsNull(penaltyCheck.Penalty);
        }

        [TestMethod]
        public void PuntOccurredSkillsCheckFalseTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99); // Block does not occur (> 1% for good snap)

            // Create test players
            var punter = new Player { Kicking = 70, LastName = "Punter" };
            var offensiveLine = new List<Player>
            {
                new Player { Position = Positions.T, Strength = 70, Awareness = 65 },
                new Player { Position = Positions.G, Strength = 70, Awareness = 65 }
            };
            var defensiveRushers = new List<Player>
            {
                new Player { Position = Positions.DE, Strength = 80, Speed = 75 }
            };

            var puntResult = new PuntBlockOccurredSkillsCheck(rng, punter, offensiveLine, defensiveRushers, true);
            puntResult.Execute(game);

            Assert.IsFalse(puntResult.Occurred);
        }

        [TestMethod]
        public void PuntOccurredSkillsCheckTrueTest()
        {
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.005); // Block occurs (< 1% for good snap)

            // Create test players
            var punter = new Player { Kicking = 70, LastName = "Punter" };
            var offensiveLine = new List<Player>
            {
                new Player { Position = Positions.T, Strength = 70, Awareness = 65 },
                new Player { Position = Positions.G, Strength = 70, Awareness = 65 }
            };
            var defensiveRushers = new List<Player>
            {
                new Player { Position = Positions.DE, Strength = 80, Speed = 75 }
            };

            var puntResult = new PuntBlockOccurredSkillsCheck(rng, punter, offensiveLine, defensiveRushers, true);
            puntResult.Execute(game);

            Assert.IsTrue(puntResult.Occurred);
        }
    }
}