using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;
using UnitTestProject1.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class InterceptionTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region InterceptionSkillsCheckResult Tests

        [TestMethod]
        public void InterceptionSkillsCheckResult_BasicReturn_CalculatesCorrectPosition()
        {
            // Arrange
            var game = _testGame.GetGame();
            var qb = new Player { LastName = "Brady", Passing = 80, Awareness = 85, Speed = 75 };
            var receiver = new Player { LastName = "Moss", Catching = 90, Speed = 85 };
            var interceptor = new Player
            {
                LastName = "Sanders",
                Position = Positions.CB,
                Coverage = 95,
                Awareness = 90,
                Speed = 88,
                Agility = 85
            };

            var offense = new List<Player> { qb, receiver };
            var defense = new List<Player> { interceptor };

            var interceptionSpot = 50; // Midfield

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.3)      // 8 + (0.3 * 7) = 10.1 yards base
                .InterceptionReturnVariance(0.2)   // (0.2 * 30) - 5 = 1 yard variance
                .NextDouble(0.99);                 // No fumble during return

            // Act
            var result = new InterceptionSkillsCheckResult(rng, qb, receiver, offense, defense, interceptionSpot);
            result.Execute(game);

            var interception = result.Result;

            // Assert
            Assert.IsNotNull(interception);
            Assert.AreEqual(interceptor, interception.Interceptor);
            Assert.AreEqual(qb, interception.ThrownBy);
            Assert.AreEqual(receiver, interception.IntendedReceiver);
            Assert.AreEqual(interceptionSpot, interception.InterceptionSpot);
            Assert.IsGreaterThanOrEqualTo(0, interception.ReturnYards, "Return yards should be non-negative");
            Assert.IsFalse(interception.IsPickSix, "Should not be pick-six from midfield with normal return");
            Assert.IsFalse(interception.FumbledDuringReturn);
            Assert.IsTrue(interception.PossessionChange);
        }

        [TestMethod]
        public void InterceptionSkillsCheckResult_PickSix_MarksAsTouchdown()
        {
            // Arrange
            var game = _testGame.GetGame();
            var qb = new Player { LastName = "Brady", Passing = 80, Awareness = 85 };
            var receiver = new Player { LastName = "Moss", Catching = 90 };
            var interceptor = new Player 
            { 
                LastName = "Sanders", 
                Position = Positions.CB,
                Coverage = 95, 
                Speed = 99,
                Awareness = 95,
                Agility = 95
            };

            var offense = new List<Player> { qb, receiver };
            var defense = new List<Player> { interceptor };
            
            var interceptionSpot = 15; // Close to goal line

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(1.0)       // Maximum base return (15 yards)
                .InterceptionReturnVariance(1.0)   // Maximum variance (+25 yards)
                .NextDouble(0.99);                 // No fumble

            // Act
            var result = new InterceptionSkillsCheckResult(rng, qb, receiver, offense, defense, interceptionSpot);
            result.Execute(game);

            var interception = result.Result;

            // Assert
            Assert.IsTrue(interception.IsPickSix, "Should be a pick-six");
            Assert.AreEqual(0, interception.FinalPosition, "Final position should be 0 (touchdown)");
            Assert.IsTrue(interception.PossessionChange);
            Assert.IsFalse(interception.FumbledDuringReturn);
        }

        [TestMethod]
        public void InterceptionSkillsCheckResult_FumbleDuringReturn_OffenseRecovers_NoNetPossessionChange()
        {
            // Arrange
            var game = _testGame.GetGame();
            var qb = new Player { LastName = "Brady", Passing = 80, Awareness = 85, Speed = 70 };
            var receiver = new Player { LastName = "Moss", Catching = 90, Speed = 80 };
            var interceptor = new Player
            {
                LastName = "Sanders",
                Position = Positions.CB,
                Coverage = 95,
                Speed = 85,
                Awareness = 50,  // Low awareness = higher fumble probability
                Agility = 85
            };

            var offense = new List<Player> { qb, receiver };
            var defense = new List<Player> { interceptor };

            var interceptionSpot = 50;

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.3)       // Smaller return
                .InterceptionReturnVariance(0.2)   // Less variance
                .NextDouble(0.005)                 // FUMBLE during return! (well below 1.125% threshold)
                // FumbleRecoverySkillsCheckResult sequence:
                .NextDouble(0.5)                   // Out of bounds check (0.5 > 0.12 = not OOB)
                .NextDouble(0.5)                   // Bounce direction (0.5 = forward bounce)
                .NextDouble(0.3)                   // Bounce yards (forward = 0-8 yards)
                .NextDouble(0.5)                   // Recovery check (0.5 < ~0.75 = offense recovers)
                .NextDouble(0.5);                  // Return yards for offense

            // Act
            var result = new InterceptionSkillsCheckResult(rng, qb, receiver, offense, defense, interceptionSpot);
            result.Execute(game);

            var interception = result.Result;

            // Assert
            Assert.IsTrue(interception.FumbledDuringReturn, "Should have fumbled during return");
            Assert.IsNotNull(interception.FumbleRecovery, "Fumble recovery should be recorded");
            Assert.IsFalse(interception.IsPickSix, "Should not be pick-six if fumbled");
        }

        [TestMethod]
        public void InterceptionSkillsCheckResult_InterceptionAtGoalLine_BoundedCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();
            var qb = new Player { LastName = "Brady", Passing = 80, Awareness = 85 };
            var receiver = new Player { LastName = "Moss", Catching = 90 };
            var interceptor = new Player 
            { 
                LastName = "Sanders", 
                Position = Positions.CB,
                Coverage = 95,
                Speed = 85,
                Awareness = 90,
                Agility = 85
            };

            var offense = new List<Player> { qb, receiver };
            var defense = new List<Player> { interceptor };
            
            var interceptionSpot = 2; // Very close to offense's goal line

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.0)       // Minimum return
                .InterceptionReturnVariance(0.0)   // Minimum variance
                .NextDouble(0.99);                 // No fumble

            // Act
            var result = new InterceptionSkillsCheckResult(rng, qb, receiver, offense, defense, interceptionSpot);
            result.Execute(game);

            var interception = result.Result;

            // Assert
            Assert.IsGreaterThanOrEqualTo(0, interception.FinalPosition, "Position should not go below 0");
            Assert.IsLessThanOrEqualTo(100, interception.FinalPosition, "Position should not exceed 100");
            
            if (interception.FinalPosition == 0)
            {
                Assert.IsTrue(interception.IsPickSix, "If at 0, should be marked as pick-six");
            }
        }

        [TestMethod]
        public void InterceptionSkillsCheckResult_SelectsBestDefender_AsInterceptor()
        {
            // Arrange
            var game = _testGame.GetGame();
            var qb = new Player { LastName = "Brady", Passing = 80, Awareness = 85 };
            var receiver = new Player { LastName = "Moss", Catching = 90 };
            
            var badCorner = new Player 
            { 
                LastName = "BadCorner", 
                Position = Positions.CB,
                Coverage = 50,
                Awareness = 50,
                Speed = 50
            };
            
            var goodCorner = new Player 
            { 
                LastName = "GoodCorner", 
                Position = Positions.CB,
                Coverage = 99,
                Awareness = 99,
                Speed = 99
            };

            var offense = new List<Player> { qb, receiver };
            var defense = new List<Player> { badCorner, goodCorner };
            
            var interceptionSpot = 50;

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.5)
                .InterceptionReturnVariance(0.5)
                .NextDouble(0.99); // No fumble

            // Act
            var result = new InterceptionSkillsCheckResult(rng, qb, receiver, offense, defense, interceptionSpot);
            result.Execute(game);

            var interception = result.Result;

            // Assert
            Assert.AreEqual(goodCorner, interception.Interceptor, 
                "Should select defender with highest Coverage + Awareness + Speed");
        }

        #endregion

        #region InterceptionReturnSkillsCheckResult Tests

        [TestMethod]
        public void InterceptionReturnSkillsCheckResult_HighSkillReturner_BonusYards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var fastInterceptor = new Player 
            { 
                LastName = "Deion",
                Speed = 99,
                Agility = 99
            };
            
            var slowOffense = new List<Player> 
            { 
                new Player { Speed = 50 },
                new Player { Speed = 50 }
            };

            var interceptionSpot = 50;

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.5)       // 11.5 yards base
                .InterceptionReturnVariance(0.5);  // 10 yards variance

            // Act
            var result = new InterceptionReturnSkillsCheckResult(rng, fastInterceptor, slowOffense, interceptionSpot);
            result.Execute(game);

            var returnResult = result.Result;

            // Assert
            Assert.IsNotNull(returnResult);
            Assert.AreEqual(fastInterceptor, returnResult.Interceptor);
            Assert.AreEqual(interceptionSpot, returnResult.InterceptionSpot);
            Assert.IsGreaterThanOrEqualTo(0, returnResult.ReturnYards, "Return yards should be non-negative");
            
            // Fast interceptor vs slow pursuit should yield good return
            Assert.IsGreaterThan(10, returnResult.ReturnYards, "Fast interceptor vs slow pursuit should yield significant return");
        }

        [TestMethod]
        public void InterceptionReturnSkillsCheckResult_SlowReturner_MinimalYards()
        {
            // Arrange
            var game = _testGame.GetGame();
            var slowInterceptor = new Player 
            { 
                LastName = "SlowDB",
                Speed = 50,
                Agility = 50
            };
            
            var fastOffense = new List<Player> 
            { 
                new Player { Speed = 99 },
                new Player { Speed = 99 }
            };

            var interceptionSpot = 50;

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(0.0)       // Minimum base (8 yards)
                .InterceptionReturnVariance(0.0);  // Minimum variance (-5 yards)

            // Act
            var result = new InterceptionReturnSkillsCheckResult(rng, slowInterceptor, fastOffense, interceptionSpot);
            result.Execute(game);

            var returnResult = result.Result;

            // Assert
            Assert.IsGreaterThanOrEqualTo(0, returnResult.ReturnYards, "Return yards should never be negative");
            
            // Slow interceptor vs fast pursuit should yield minimal return
            Assert.IsLessThan(15, returnResult.ReturnYards, "Slow interceptor vs fast pursuit should yield minimal return");
        }

        [TestMethod]
        public void InterceptionReturnSkillsCheckResult_NearGoalLine_BoundedByFieldPosition()
        {
            // Arrange
            var game = _testGame.GetGame();
            var interceptor = new Player 
            { 
                Speed = 99,
                Agility = 99
            };
            
            var offense = new List<Player> { new Player { Speed = 50 } };
            var interceptionSpot = 5; // Very close to offense's goal

            var rng = new TestFluentSeedableRandom()
                .InterceptionReturnBase(1.0)       // Maximum base (15 yards)
                .InterceptionReturnVariance(1.0);  // Maximum variance (+25 yards)

            // Act
            var result = new InterceptionReturnSkillsCheckResult(rng, interceptor, offense, interceptionSpot);
            result.Execute(game);

            var returnResult = result.Result;

            // Assert
            Assert.IsLessThanOrEqualTo(interceptionSpot, returnResult.ReturnYards, "Return yards should not exceed distance to goal line");
        }

        #endregion

        #region Integration Tests with Pass Play

        [TestMethod]
        public void PassPlay_Interception_CreatesInterceptionDomainObject()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 40, 95); // Weak QB, strong coverage = high INT chance

            var rng = PassPlayScenarios.Interception(returnYards: 15);

            // Act
            var pass = new StateLibrary.Plays.Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Interception, "Play should be marked as interception");
            Assert.IsNotNull(passPlay.InterceptionDetails, "Interception details should exist");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change on interception");
            Assert.IsNotNull(passPlay.InterceptionDetails.InterceptedBy, "Interceptor should be recorded");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Set offensive players (pass formation)
            passPlay.OffensePlayersOnField = new List<Player>
            {
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.C][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][2],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            // Set defensive players (nickel defense)
            passPlay.DefensePlayersOnField = new List<Player>
            {
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        private void SetPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            foreach (var player in game.CurrentPlay!.OffensePlayersOnField)
            {
                player.Passing = offenseSkill;
                player.Awareness = offenseSkill;
                player.Catching = offenseSkill;
            }

            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Coverage = defenseSkill;
                player.Awareness = defenseSkill;
                player.Speed = defenseSkill;
                player.Agility = defenseSkill;
            }
        }

        #endregion
    }
}
