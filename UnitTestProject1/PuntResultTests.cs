using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PuntResultTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Blocked Punt Recovery Tests

        [TestMethod]
        public void PuntResult_BlockedPuntDefenseRecoveryTouchdown_Scores6Points()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 92;  // Near goal line
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.Blocked = true;
            play.YardsGained = 10;  // Reaches 100+
            play.PossessionChange = true;  // Defense recovered
            play.IsTouchdown = true;
            play.RecoveredBy = new Player { LastName = "Defender", Position = Positions.DE };
            game.HomeScore = 7;
            game.AwayScore = 10;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(100, game.FieldPosition, "Should be at goal line");
            Assert.AreEqual(7, game.HomeScore, "Home score should not change");
            Assert.AreEqual(16, game.AwayScore, "Away team should score TD (10 + 6)");
            Assert.IsTrue(play.PossessionChange, "Possession should change after TD");
        }

        [TestMethod]
        public void PuntResult_BlockedPuntDefenseRecoveryNoTD_ChangePossession()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;
            var play = (PuntPlay)game.CurrentPlay;
            play.Blocked = true;
            play.YardsGained = 8;
            play.PossessionChange = true;
            play.RecoveredBy = new Player { LastName = "Defender" };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(43, game.FieldPosition, "Field position should advance 8 yards");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.IsTrue(play.PossessionChange, "Possession should have changed");
        }

        [TestMethod]
        public void PuntResult_BlockedPuntOffenseRecovery_TurnoverOnDowns()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            var play = (PuntPlay)game.CurrentPlay;
            play.Blocked = true;
            play.YardsGained = -7;  // Loss
            play.PossessionChange = false;  // Offense recovered
            play.RecoveredBy = new Player { LastName = "Punter" };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(33, game.FieldPosition, "Field position should go back 7 yards");
            Assert.IsTrue(play.PossessionChange, "Should now be turnover on downs");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
        }

        #endregion

        #region Muffed Catch Recovery Tests

        [TestMethod]
        public void PuntResult_MuffedCatchReceivingTeamRecovers_KeepsPossession()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;
            var play = (PuntPlay)game.CurrentPlay;
            play.MuffedCatch = true;
            play.YardsGained = 38;  // Punt + recovery
            play.PossessionChange = true;  // Receiving team recovered
            play.RecoveredBy = new Player { LastName = "Returner" };
            play.MuffedBy = new Player { LastName = "Returner" };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(83, game.FieldPosition, "Field position should be 45 + 38");
            Assert.IsTrue(play.PossessionChange, "Possession should change (receiving team now has it)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }

        [TestMethod]
        public void PuntResult_MuffedCatchPuntingTeamRecovers_RetainsPossession()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;
            play.MuffedCatch = true;
            play.YardsGained = 42;
            play.PossessionChange = false;  // Punting team recovered!
            play.RecoveredBy = new Player { LastName = "Gunner" };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(82, game.FieldPosition, "Field position should advance");
            Assert.IsFalse(play.PossessionChange, "Punting team should keep possession");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
        }

        #endregion

        #region Touchback Tests

        [TestMethod]
        public void PuntResult_Touchback_PlacesBallAt20()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;
            play.Touchback = true;
            play.YardsGained = 65;  // Punt went into end zone

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(80, game.FieldPosition, "Ball should be at 20 yard line (80 from punter's perspective)");
            Assert.AreEqual(80, play.EndFieldPosition, "End position should be 80");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }

        #endregion

        #region Downed and Fair Catch Tests

        [TestMethod]
        public void PuntResult_PuntDowned_UpdatesFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;
            var play = (PuntPlay)game.CurrentPlay;
            play.Downed = true;
            play.YardsGained = 48;
            play.DownedAtYardLine = 83;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(83, game.FieldPosition, "Field position should match punt distance");
            Assert.AreEqual(83, play.EndFieldPosition, "End position should be set");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }

        [TestMethod]
        public void PuntResult_FairCatch_UpdatesFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 42;
            var play = (PuntPlay)game.CurrentPlay;
            play.FairCatch = true;
            play.YardsGained = 38;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(80, game.FieldPosition, "Field position should be 42 + 38");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }

        #endregion

        #region Punt Return Tests

        [TestMethod]
        public void PuntResult_NormalReturn_UpdatesFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 52;  // 45 yard punt + 7 yard return
            var returner = new Player { LastName = "Speedster", Position = Positions.CB };
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = 7,
                EndedInFumble = false
            });

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(92, game.FieldPosition, "Field position should be 40 + 52");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.AreEqual(returner, play.InitialReturner, "Should track returner");
        }

        [TestMethod]
        public void PuntResult_ReturnForTouchdown_Scores6Points()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 88;
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 15;  // Return for TD
            play.IsTouchdown = true;
            var returner = new Player { LastName = "FastGuy", Position = Positions.WR };
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = 5,  // Short punt + good return
                EndedInFumble = false
            });
            game.HomeScore = 14;
            game.AwayScore = 7;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(100, game.FieldPosition, "Should be at goal line");
            Assert.IsTrue(play.IsTouchdown, "Should be marked as TD");
            Assert.AreEqual(14, game.HomeScore, "Home score should not change");
            Assert.AreEqual(13, game.AwayScore, "Away team should score TD (7 + 6)");
            Assert.IsTrue(play.PossessionChange, "Possession should change after TD");
        }

        [TestMethod]
        public void PuntResult_ReturnNearGoalLine_ClampedTo100()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 92;
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 20;  // Would exceed 100
            var returner = new Player { LastName = "Returner" };
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = 12
            });
            game.HomeScore = 7;
            game.AwayScore = 10;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be touchdown");
            Assert.AreEqual(100, game.FieldPosition, "Should clamp at 100");
            Assert.AreEqual(16, game.AwayScore, "Should score TD");
        }

        #endregion

        #region Field Position Update Tests

        [TestMethod]
        public void PuntResult_SetsStartFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;
            var play = (PuntPlay)game.CurrentPlay;
            play.Downed = true;
            play.YardsGained = 45;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(35, play.StartFieldPosition, "Start position should be set to 35");
            Assert.AreEqual(80, play.EndFieldPosition, "End position should be 80 (35 + 45)");
        }

        [TestMethod]
        public void PuntResult_UpdatesGameFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;
            play.FairCatch = true;
            play.YardsGained = 42;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(82, game.FieldPosition, "Game field position should be updated");
            Assert.AreEqual(82, play.EndFieldPosition, "Play end position should match");
        }

        #endregion

        #region Down and Distance Tests

        [TestMethod]
        public void PuntResult_AfterPunt_ResetsToFirstAnd10()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            game.YardsToGo = 5;
            var play = (PuntPlay)game.CurrentPlay;
            play.Downed = true;
            play.YardsGained = 45;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards");
        }

        [TestMethod]
        public void PuntResult_AllScenarios_Reset_ToFirstDown()
        {
            // Test touchback
            var game1 = CreateGameWithPuntPlay();
            var play1 = (PuntPlay)game1.CurrentPlay;
            play1.Touchback = true;
            new PuntResult().Execute(game1);
            Assert.AreEqual(Downs.First, game1.CurrentDown, "Touchback should reset to first down");

            // Test fair catch
            var game2 = CreateGameWithPuntPlay();
            var play2 = (PuntPlay)game2.CurrentPlay;
            play2.FairCatch = true;
            play2.YardsGained = 40;
            new PuntResult().Execute(game2);
            Assert.AreEqual(Downs.First, game2.CurrentDown, "Fair catch should reset to first down");

            // Test return
            var game3 = CreateGameWithPuntPlay();
            var play3 = (PuntPlay)game3.CurrentPlay;
            play3.ReturnSegments.Add(new ReturnSegment { BallCarrier = new Player(), YardsGained = 10 });
            play3.YardsGained = 50;
            new PuntResult().Execute(game3);
            Assert.AreEqual(Downs.First, game3.CurrentDown, "Return should reset to first down");
        }

        #endregion

        #region Possession Change Tests

        [TestMethod]
        public void PuntResult_NormalPunt_ChangesPossession()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;
            play.Downed = true;
            play.YardsGained = 40;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.PossessionChange, "Possession should change on normal punt");
        }

        [TestMethod]
        public void PuntResult_MuffRecoveredByPuntingTeam_NoChange()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;
            play.MuffedCatch = true;
            play.PossessionChange = false;  // Punting team recovered
            play.RecoveredBy = new Player();
            play.YardsGained = 45;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsFalse(play.PossessionChange, "Possession should NOT change when punting team recovers muff");
        }

        #endregion

        #region Penalty Tests

        [TestMethod]
        public void PuntResult_DefensivePenaltyAccepted_AutomaticFirstDown()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            game.YardsToGo = 10;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 35; // Punt goes 35 yards
            play.Downed = true;

            // Add defensive holding penalty (5 yards, automatic first down)
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Defensive Holding",
                    Yards = 5,
                    CalledOn = Possession.Away,
                    AutomaticFirstDown = true,
                    Accepted = false  // Will be set by acceptance logic
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Penalty should be accepted and punting team gets automatic first down
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down from penalty");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.IsFalse(play.PossessionChange, "Punting team should retain possession");
        }

        [TestMethod]
        public void PuntResult_OffensivePenaltyAccepted_Rekick()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 25;
            play.Downed = true;

            // Add offensive holding penalty (10 yards)
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Offensive Holding",
                    Yards = 10,
                    CalledOn = Possession.Home,
                    Accepted = false  // Will be set by acceptance logic
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Penalty enforced, punt must be rekicked from further back
            Assert.AreEqual(30, game.FieldPosition, "Should move back 10 yards (40 - 10)");
        }

        [TestMethod]
        public void PuntResult_OffsettingPenalties_Rekick()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 30;

            // Add offsetting penalties
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Offensive Holding",
                    Yards = 10,
                    CalledOn = Possession.Home,
                    Accepted = true
                },
                new Penalty
                {
                    Name = "Defensive Pass Interference",
                    Yards = 15,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Offsetting penalties, rekick from same spot
            Assert.AreEqual(40, game.FieldPosition, "Field position should remain unchanged");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should still be fourth down for rekick");
        }

        [TestMethod]
        public void PuntResult_PenaltyOnReturn_EnforcedFromSpot()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 40; // Punt and return net 40 yards
            play.ReturnSegments = new System.Collections.Generic.List<Segment>
            {
                new Segment { Distance = 15, Direction = 1 }
            };

            // Add defensive holding penalty during return (10 yards)
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Holding on Return",
                    Yards = 10,
                    CalledOn = Possession.Away,  // Receiving team penalty
                    Accepted = false  // Will be set by acceptance logic
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Penalty enforced against receiving team
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
        }

        [TestMethod]
        public void PuntResult_PenaltyPushesPunterIntoEndZone_Safety()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 5; // Near own goal line
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 15;
            play.Downed = true;

            // Add offensive penalty that pushes team into own end zone
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Illegal Formation",
                    Yards = 10,
                    CalledOn = Possession.Home,  // Punting team
                    Accepted = true
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Should result in safety
            Assert.AreEqual(0, game.FieldPosition, "Should be at goal line");
            Assert.IsTrue(play.IsSafety, "Should be a safety");
        }

        [TestMethod]
        public void PuntResult_DefensivePenaltyOnTouchdown_StillScores()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 20;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 80; // Return for TD
            play.IsTouchdown = true;
            play.ReturnSegments = new System.Collections.Generic.List<Segment>
            {
                new Segment { Distance = 80, Direction = 1 }
            };

            // Add defensive penalty (doesn't negate TD)
            play.Penalties = new System.Collections.Generic.List<Penalty>
            {
                new Penalty
                {
                    Name = "Unnecessary Roughness",
                    Yards = 15,
                    CalledOn = Possession.Home,  // Punting team (defense on return)
                    Accepted = false  // Should be declined since TD stands
                }
            };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - TD should stand
            Assert.AreEqual(100, game.FieldPosition, "Should be at goal line");
            Assert.IsTrue(play.IsTouchdown, "Should be a touchdown");
        }

        [TestMethod]
        public void PuntResult_NoPenalties_NormalExecution()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;
            play.YardsGained = 30;
            play.Downed = true;

            // No penalties
            play.Penalties = new System.Collections.Generic.List<Penalty>();

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert - Normal punt execution
            Assert.AreEqual(70, game.FieldPosition, "Should advance 30 yards (40 + 30)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPuntPlay()
        {
            var game = _testGame.GetGame();

            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartFieldPosition = 0,
                YardsGained = 0
            };

            puntPlay.Punter = new Player
            {
                LastName = "Punter",
                Position = Positions.P,
                Kicking = 70
            };

            game.CurrentPlay = puntPlay;
            game.FieldPosition = 40;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Fourth;
            game.HomeScore = 0;
            game.AwayScore = 0;

            return game;
        }

        #endregion
    }
}
