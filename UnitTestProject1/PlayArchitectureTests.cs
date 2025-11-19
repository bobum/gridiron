using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PlayArchitectureTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();

        #region Vanilla/Basic Play Tests

        [TestMethod]
        public void VanillaRunPlay_SingleCarrier_NoFumble()
        {
            // Arrange
            var play = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
                EndFieldPosition = 32,
                YardsGained = 7
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 7,
                Direction = RunDirection.Middle,
                EndedInFumble = false
            });

            // Assert
            Assert.AreEqual(PlayType.Run, play.PlayType);
            Assert.AreEqual(rb, play.InitialBallCarrier);
            Assert.AreEqual(rb, play.FinalBallCarrier);
            Assert.AreEqual(7, play.TotalYards);
            Assert.IsFalse(play.HadFumbles);
            Assert.AreEqual(1, play.RunSegments.Count);
        }

        [TestMethod]
        public void VanillaPassPlay_Complete_NoLateral()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second,
                StartFieldPosition = 30,
                EndFieldPosition = 45,
                YardsGained = 15
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 10,
                YardsAfterCatch = 5
            });

            // Assert
            Assert.AreEqual(PlayType.Pass, play.PlayType);
            Assert.AreEqual(qb, play.PrimaryPasser);
            Assert.AreEqual(wr, play.FinalReceiver);
            Assert.IsTrue(play.IsComplete);
            Assert.AreEqual(15, play.TotalYards);
            Assert.AreEqual(10, play.TotalAirYards);
            Assert.IsFalse(play.HadLaterals);
            Assert.IsFalse(play.HadFumbles);
        }

        [TestMethod]
        public void VanillaPassPlay_Incomplete()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Third,
                StartFieldPosition = 40,
                EndFieldPosition = 40,
                YardsGained = 0
            };

            var qb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = false,
                Type = PassType.Deep,
                AirYards = 25,
                YardsAfterCatch = 0
            });

            // Assert
            Assert.IsFalse(play.IsComplete);
            Assert.AreEqual(0, play.TotalYards);
            Assert.AreEqual(25, play.TotalAirYards);
        }

        [TestMethod]
        public void VanillaKickoffPlay_Touchback()
        {
            // Arrange
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                Touchback = true,
                KickDistance = 70
            };

            var kicker = _teams.HomeTeam.KickoffOffenseDepthChart.Chart[Positions.K][0];
            play.Kicker = kicker;

            // Assert
            Assert.AreEqual(PlayType.Kickoff, play.PlayType);
            Assert.IsTrue(play.Touchback);
            Assert.AreEqual(0, play.ReturnSegments.Count);
            Assert.AreEqual(kicker, play.Kicker);
        }

        [TestMethod]
        public void VanillaKickoffPlay_WithReturn()
        {
            // Arrange
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                KickDistance = 65,
                Touchback = false
            };

            var returner = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = 25,
                EndedInFumble = false
            });

            // Assert
            Assert.IsFalse(play.Touchback);
            Assert.AreEqual(1, play.ReturnSegments.Count);
            Assert.AreEqual(returner, play.InitialReturner);
            Assert.AreEqual(25, play.TotalReturnYards);
        }

        [TestMethod]
        public void VanillaPuntPlay_FairCatch()
        {
            // Arrange
            var play = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                FairCatch = true,
                PuntDistance = 45,
                HangTime = 4.5
            };

            var punter = _teams.HomeTeam.PuntOffenseDepthChart.Chart[Positions.P][0];
            play.Punter = punter;

            // Assert
            Assert.AreEqual(PlayType.Punt, play.PlayType);
            Assert.IsTrue(play.FairCatch);
            Assert.AreEqual(0, play.ReturnSegments.Count);
            Assert.AreEqual(45, play.PuntDistance);
        }

        [TestMethod]
        public void VanillaFieldGoalPlay_Good()
        {
            // Arrange
            var play = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                IsGood = true,
                AttemptDistance = 42,
                Blocked = false,
                IsExtraPoint = false
            };

            var kicker = _teams.HomeTeam.FieldGoalOffenseDepthChart.Chart[Positions.K][0];
            var holder = _teams.HomeTeam.FieldGoalOffenseDepthChart.Chart[Positions.H][0];
            play.Kicker = kicker;
            play.Holder = holder;

            // Assert
            Assert.AreEqual(PlayType.FieldGoal, play.PlayType);
            Assert.IsTrue(play.IsGood);
            Assert.IsFalse(play.Blocked);
            Assert.AreEqual(42, play.AttemptDistance);
        }

        #endregion

        #region Multiple Fumbles Tests

        [TestMethod]
        public void RunPlay_TwoFumbles_SameTeamRecoversBoth()
        {
            // Arrange - RB fumbles, offense recovers, runs more, fumbles again, offense recovers
            var play = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var te = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0];

            // First segment: RB runs for 5 yards, then fumbles
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 5,
                Direction = RunDirection.Left,
                EndedInFumble = true,
                FumbledBy = rb,
                RecoveredBy = wr
            });

            // Second segment: WR picks it up, runs for 3 yards, fumbles
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = wr,
                YardsGained = 3,
                Direction = RunDirection.Middle,
                EndedInFumble = true,
                FumbledBy = wr,
                RecoveredBy = te
            });

            // Third segment: TE picks it up, runs for 2 yards
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = te,
                YardsGained = 2,
                Direction = RunDirection.Right,
                EndedInFumble = false
            });

            // Assert
            Assert.AreEqual(3, play.RunSegments.Count);
            Assert.IsTrue(play.HadFumbles);
            Assert.AreEqual(rb, play.InitialBallCarrier);
            Assert.AreEqual(te, play.FinalBallCarrier);
            Assert.AreEqual(10, play.TotalYards);
            Assert.AreEqual(2, play.RunSegments.Count(s => s.EndedInFumble));
        }

        [TestMethod]
        public void RunPlay_FumbleRecoveredByDefense()
        {
            // Arrange
            var play = new RunPlay
            {
                Possession = Possession.Home,
                PossessionChange = true
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            var lb = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0];

            // RB runs for 3 yards, fumbles, defense recovers and runs back 15
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 3,
                Direction = RunDirection.Right,
                EndedInFumble = true,
                FumbledBy = rb,
                RecoveredBy = lb
            });

            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = lb,
                YardsGained = -15, // Negative because it's a return against the offense
                Direction = RunDirection.Left,
                EndedInFumble = false
            });

            // Assert
            Assert.IsTrue(play.HadFumbles);
            Assert.IsTrue(play.PossessionChange);
            Assert.AreEqual(rb, play.InitialBallCarrier);
            Assert.AreEqual(lb, play.FinalBallCarrier);
            Assert.AreEqual(-12, play.TotalYards); // 3 - 15
        }

        [TestMethod]
        public void PassPlay_FumbleAfterCatch()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Home,
                PossessionChange = false
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];

            // Complete pass, receiver runs, fumbles, teammate recovers
            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 8,
                YardsAfterCatch = 5,
                EndedInFumble = true,
                FumbledBy = wr,
                RecoveredBy = rb
            });

            // RB continues as a run segment (this is how fumble recovery could work)
            // Note: In reality, this might stay as pass play stats, but architecture allows it

            // Assert
            Assert.IsTrue(play.IsComplete);
            Assert.IsTrue(play.HadFumbles);
            Assert.AreEqual(1, play.PassSegments.Count(s => s.EndedInFumble));
        }

        #endregion

        #region Lateral Pass Tests

        [TestMethod]
        public void PassPlay_OneLateral_TwoPassers()
        {
            // Arrange - QB throws to WR, WR laterals to RB
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];

            // First pass: QB to WR
            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 5,
                YardsAfterCatch = 3
            });

            // Lateral: WR to RB
            play.PassSegments.Add(new PassSegment
            {
                Passer = wr,
                Receiver = rb,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -2, // Backwards pass
                YardsAfterCatch = 7
            });

            // Assert
            Assert.AreEqual(2, play.PassSegments.Count);
            Assert.IsTrue(play.HadLaterals);
            Assert.AreEqual(qb, play.PrimaryPasser);
            Assert.AreEqual(rb, play.FinalReceiver);
            Assert.AreEqual(13, play.TotalYards); // 5+3-2+7
            Assert.AreEqual(3, play.TotalAirYards); // 5 + (-2)
        }

        [TestMethod]
        public void PassPlay_MultipleLaterals_ThreePassers()
        {
            // Arrange - QB to WR, WR to TE, TE to RB (hook and ladder play)
            var play = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Fourth
            };

            var qb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var te = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.TE][0];
            var rb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.RB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 8,
                YardsAfterCatch = 2
            });

            play.PassSegments.Add(new PassSegment
            {
                Passer = wr,
                Receiver = te,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -1,
                YardsAfterCatch = 5
            });

            play.PassSegments.Add(new PassSegment
            {
                Passer = te,
                Receiver = rb,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -3,
                YardsAfterCatch = 15
            });

            // Assert
            Assert.AreEqual(3, play.PassSegments.Count);
            Assert.IsTrue(play.HadLaterals);
            Assert.AreEqual(qb, play.PrimaryPasser);
            Assert.AreEqual(rb, play.FinalReceiver);
            Assert.AreEqual(26, play.TotalYards); // 8+2-1+5-3+15
        }

        [TestMethod]
        public void PassPlay_LateralWithFumble()
        {
            // Arrange - QB to WR, WR laterals to RB, RB fumbles, defense recovers
            var play = new PassPlay
            {
                Possession = Possession.Home,
                PossessionChange = true
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 6,
                YardsAfterCatch = 4
            });

            play.PassSegments.Add(new PassSegment
            {
                Passer = wr,
                Receiver = rb,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -2,
                YardsAfterCatch = 1,
                EndedInFumble = true
            });

            // Assert
            Assert.IsTrue(play.HadLaterals);
            Assert.IsTrue(play.HadFumbles);
            Assert.IsTrue(play.PossessionChange);
        }

        #endregion

        #region Interception Tests

        [TestMethod]
        public void PassPlay_Interception_NoReturn()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Interception = true,
                PossessionChange = true
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var cb = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = false,
                Type = PassType.Deep,
                AirYards = 30,
                YardsAfterCatch = 0
            });

            play.InterceptionDetails = new Interception
            {
                InterceptedBy = cb,
                InterceptionYardLine = 15,
                ReturnYards = 0
            };

            // Assert
            Assert.IsTrue(play.Interception);
            Assert.IsFalse(play.IsComplete);
            Assert.IsTrue(play.PossessionChange);
            Assert.IsNotNull(play.InterceptionDetails);
            Assert.AreEqual(cb, play.InterceptionDetails.InterceptedBy);
        }

        [TestMethod]
        public void PassPlay_Interception_WithReturn()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Away,
                Interception = true,
                PossessionChange = true
            };

            var qb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var fs = _teams.HomeTeam.DefenseDepthChart.Chart[Positions.FS][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = false,
                Type = PassType.Deep,
                AirYards = 40,
                YardsAfterCatch = 0
            });

            play.InterceptionDetails = new Interception
            {
                InterceptedBy = fs,
                InterceptionYardLine = 35,
                ReturnYards = 45
            };

            // Assert
            Assert.IsTrue(play.Interception);
            Assert.AreEqual(45, play.InterceptionDetails.ReturnYards);
            Assert.AreEqual(fs, play.InterceptionDetails.InterceptedBy);
        }

        [TestMethod]
        public void PassPlay_InterceptionReturn_WithFumble()
        {
            // Arrange - Interception, returner fumbles, offense recovers
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Interception = true,
                PossessionChange = false // Fumble back to original offense
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var cb = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0];
            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = false,
                Type = PassType.Short,
                AirYards = 15,
                YardsAfterCatch = 0
            });

            play.InterceptionDetails = new Interception
            {
                InterceptedBy = cb,
                InterceptionYardLine = 30,
                ReturnYards = 10,
                FumbledDuringReturn = true,
                RecoveredBy = rb
            };

            // Assert
            Assert.IsTrue(play.Interception);
            Assert.IsTrue(play.InterceptionDetails.FumbledDuringReturn);
            Assert.IsFalse(play.PossessionChange); // Back to original offense
            Assert.AreEqual(rb, play.InterceptionDetails.RecoveredBy);
        }

        #endregion

        #region Blocked Kick Tests

        [TestMethod]
        public void FieldGoalPlay_Blocked_NoReturn()
        {
            // Arrange
            var play = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                IsGood = false,
                Blocked = true,
                AttemptDistance = 48
            };

            var kicker = _teams.HomeTeam.FieldGoalOffenseDepthChart.Chart[Positions.K][0];
            var dt = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0];

            play.Kicker = kicker;
            play.BlockedBy = dt;

            // Assert
            Assert.IsTrue(play.Blocked);
            Assert.IsFalse(play.IsGood);
            Assert.AreEqual(dt, play.BlockedBy);
            Assert.IsNull(play.BlockReturnSegments);
        }

        [TestMethod]
        public void FieldGoalPlay_Blocked_WithReturn()
        {
            // Arrange
            var play = new FieldGoalPlay
            {
                Possession = Possession.Away,
                Down = Downs.Fourth,
                IsGood = false,
                Blocked = true,
                AttemptDistance = 52
            };

            var kicker = _teams.VisitorTeam.FieldGoalOffenseDepthChart.Chart[Positions.K][0];
            var de = _teams.HomeTeam.DefenseDepthChart.Chart[Positions.DE][0];
            var lb = _teams.HomeTeam.DefenseDepthChart.Chart[Positions.LB][0];

            play.Kicker = kicker;
            play.BlockedBy = de;
            play.BlockReturnSegments = new List<ReturnSegment>
            {
                new ReturnSegment
                {
                    BallCarrier = lb,
                    YardsGained = 35,
                    EndedInFumble = false
                }
            };

            // Assert
            Assert.IsTrue(play.Blocked);
            Assert.IsNotNull(play.BlockReturnSegments);
            Assert.AreEqual(1, play.BlockReturnSegments.Count);
            Assert.AreEqual(35, play.BlockReturnSegments[0].YardsGained);
        }

        [TestMethod]
        public void PuntPlay_Blocked_RecoveredByKickingTeam()
        {
            // Arrange - Blocked punt recovered by punting team (still their ball)
            var play = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                Blocked = true,
                PuntDistance = 0
            };

            var punter = _teams.HomeTeam.PuntOffenseDepthChart.Chart[Positions.P][0];
            var de = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0];
            var te = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0];

            play.Punter = punter;
            play.BlockedBy = de;
            play.RecoveredBy = te;
            play.RecoveryYards = 2; // Advanced 2 yards after recovery

            // Assert
            Assert.IsTrue(play.Blocked);
            Assert.AreEqual(te, play.RecoveredBy);
            Assert.AreEqual(2, play.RecoveryYards);
        }

        [TestMethod]
        public void PuntPlay_Blocked_ReturnedForTouchdown()
        {
            // Arrange
            var play = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                Blocked = true,
                IsTouchdown = true,
                PuntDistance = 0
            };

            var punter = _teams.HomeTeam.PuntOffenseDepthChart.Chart[Positions.P][0];
            var lb = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0];

            play.Punter = punter;
            play.BlockedBy = lb;
            play.ReturnSegments = new List<ReturnSegment>
            {
                new ReturnSegment
                {
                    BallCarrier = lb,
                    YardsGained = 20, // Returned to end zone
                    EndedInFumble = false
                }
            };

            // Assert
            Assert.IsTrue(play.Blocked);
            Assert.IsTrue(play.IsTouchdown);
            Assert.AreEqual(20, play.ReturnSegments[0].YardsGained);
        }

        #endregion

        #region Kickoff Special Scenarios

        [TestMethod]
        public void KickoffPlay_OnsideKick_Recovered()
        {
            // Arrange
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                OnsideKick = true,
                KickDistance = 12
            };

            var kicker = _teams.HomeTeam.KickoffOffenseDepthChart.Chart[Positions.K][0];
            var te = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0];

            play.Kicker = kicker;
            play.RecoveredBy = te;
            play.PossessionChange = false; // Kicking team retained possession

            // Assert
            Assert.IsTrue(play.OnsideKick);
            Assert.IsFalse(play.PossessionChange);
            Assert.AreEqual(te, play.RecoveredBy);
        }

        [TestMethod]
        public void KickoffPlay_Return_MultipleFumbles()
        {
            // Arrange - Return man fumbles, teammate picks up, fumbles again, third player recovers
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                KickDistance = 65,
                PossessionChange = false
            };

            var wr1 = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var wr2 = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][1];
            var rb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.RB][0];

            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = wr1,
                YardsGained = 15,
                EndedInFumble = true,
                FumbledBy = wr1,
                RecoveredBy = wr2
            });

            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = wr2,
                YardsGained = 5,
                EndedInFumble = true,
                FumbledBy = wr2,
                RecoveredBy = rb
            });

            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = rb,
                YardsGained = 10,
                EndedInFumble = false
            });

            // Assert
            Assert.AreEqual(3, play.ReturnSegments.Count);
            Assert.AreEqual(2, play.ReturnSegments.Count(s => s.EndedInFumble));
            Assert.AreEqual(30, play.TotalReturnYards);
            Assert.AreEqual(wr1, play.InitialReturner);
            Assert.AreEqual(rb, play.FinalReturner);
        }

        #endregion

        #region Touchdown Scenarios

        [TestMethod]
        public void RunPlay_Touchdown_SimpleRun()
        {
            // Arrange
            var play = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 97,
                EndFieldPosition = 100,
                YardsGained = 3,
                IsTouchdown = true
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 3,
                Direction = RunDirection.Middle,
                EndedInFumble = false
            });

            // Assert
            Assert.IsTrue(play.IsTouchdown);
            Assert.AreEqual(100, play.EndFieldPosition);
        }

        [TestMethod]
        public void PassPlay_Touchdown_AfterLaterals()
        {
            // Arrange - Pass caught at 5 yard line, lateral, lateral, touchdown
            var play = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Third,
                IsTouchdown = true
            };

            var qb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var te = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.TE][0];
            var rb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.RB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 3,
                YardsAfterCatch = 1
            });

            play.PassSegments.Add(new PassSegment
            {
                Passer = wr,
                Receiver = te,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -1,
                YardsAfterCatch = 2
            });

            play.PassSegments.Add(new PassSegment
            {
                Passer = te,
                Receiver = rb,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -2,
                YardsAfterCatch = 3 // Scores!
            });

            // Assert
            Assert.IsTrue(play.IsTouchdown);
            Assert.IsTrue(play.HadLaterals);
            Assert.AreEqual(3, play.PassSegments.Count);
        }

        [TestMethod]
        public void InterceptionReturn_Touchdown()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Interception = true,
                PossessionChange = true,
                IsTouchdown = true
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var cb = _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0];

            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr,
                IsComplete = false,
                Type = PassType.Deep,
                AirYards = 35
            });

            play.InterceptionDetails = new Interception
            {
                InterceptedBy = cb,
                InterceptionYardLine = 35,
                ReturnYards = 65 // Pick six!
            };

            // Assert
            Assert.IsTrue(play.Interception);
            Assert.IsTrue(play.IsTouchdown);
            Assert.AreEqual(65, play.InterceptionDetails.ReturnYards);
        }

        #endregion

        #region Complex Combination Tests

        [TestMethod]
        public void CrazyPlay_PassLateralFumbleRecoveredLateralTouchdown()
        {
            // Arrange - The craziest play: QB passes to WR1, laterals to WR2, WR2 fumbles,
            // TE recovers, laterals to RB, RB scores
            var play = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                IsTouchdown = true
            };

            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var wr1 = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var wr2 = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1];
            var te = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0];
            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];

            // QB to WR1
            play.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = wr1,
                IsComplete = true,
                Type = PassType.Short,
                AirYards = 10,
                YardsAfterCatch = 5
            });

            // WR1 lateral to WR2 who fumbles
            play.PassSegments.Add(new PassSegment
            {
                Passer = wr1,
                Receiver = wr2,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -3,
                YardsAfterCatch = 2,
                EndedInFumble = true,
                FumbledBy = wr2,
                RecoveredBy = te
            });

            // TE picks up fumble, laterals to RB
            play.PassSegments.Add(new PassSegment
            {
                Passer = te,
                Receiver = rb,
                IsComplete = true,
                Type = PassType.Lateral,
                AirYards = -1,
                YardsAfterCatch = 15 // RB scores!
            });

            // Assert
            Assert.AreEqual(3, play.PassSegments.Count);
            Assert.IsTrue(play.HadLaterals);
            Assert.IsTrue(play.HadFumbles);
            Assert.IsTrue(play.IsTouchdown);
            Assert.AreEqual(qb, play.PrimaryPasser);
            Assert.AreEqual(rb, play.FinalReceiver);
            Assert.AreEqual(28, play.TotalYards); // 10+5-3+2-1+15
        }

        [TestMethod]
        public void CrazyPlay_KickoffReturnLateralFumbleRecoveryTouchdown()
        {
            // Arrange - Kickoff return with lateral and fumble recovery for TD
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                IsTouchdown = true,
                KickDistance = 65
            };

            var wr1 = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var wr2 = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][1];
            var rb = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.RB][0];
            var te = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.TE][0];

            // Initial returner runs 20, gets hit, fumbles
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = wr1,
                YardsGained = 20,
                EndedInFumble = true,
                FumbledBy = wr1,
                RecoveredBy = wr2
            });

            // WR2 picks it up, runs 15 yards
            play.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = wr2,
                YardsGained = 15,
                EndedInFumble = false
            });

            // Note: Laterals on kick returns would need special handling
            // This demonstrates fumble recovery continuation

            // Assert
            Assert.IsTrue(play.IsTouchdown);
            Assert.AreEqual(2, play.ReturnSegments.Count);
            Assert.AreEqual(35, play.TotalReturnYards);
            Assert.AreEqual(1, play.ReturnSegments.Count(s => s.EndedInFumble));
        }

        [TestMethod]
        public void PuntReturn_MuffedCatch_RecoveredByKickingTeam()
        {
            // Arrange - Returner muffs punt, kicking team recovers
            var play = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                PuntDistance = 45,
                MuffedCatch = true,
                PossessionChange = false // Kicking team recovers
            };

            var punter = _teams.HomeTeam.PuntOffenseDepthChart.Chart[Positions.P][0];
            var wr = _teams.VisitorTeam.OffenseDepthChart.Chart[Positions.WR][0];
            var te = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0];

            play.Punter = punter;
            play.MuffedBy = wr;
            play.RecoveredBy = te;

            // Assert
            Assert.IsTrue(play.MuffedCatch);
            Assert.IsFalse(play.PossessionChange);
            Assert.AreEqual(wr, play.MuffedBy);
            Assert.AreEqual(te, play.RecoveredBy);
        }

        #endregion

        #region Penalty Scenarios

        [TestMethod]
        public void RunPlay_WithPenalty()
        {
            // Arrange
            var play = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            var t = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0];

            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 15,
                Direction = RunDirection.Right
            });

            play.Penalties.Add(new Penalty
            {
                CommittedBy = t,
                PenaltyType = PenaltyNames.OffensiveHolding,
                Yards = 10,
                Accepted = true
            });

            // Assert
            Assert.AreEqual(1, play.Penalties.Count);
            Assert.IsTrue(play.Penalties[0].Accepted);
            Assert.AreEqual(10, play.Penalties[0].Yards);
        }

        #endregion

        #region Time Expiration Tests

        [TestMethod]
        public void Play_ExpiresQuarter()
        {
            // Arrange
            var play = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Third,
                StartTime = 5,
                StopTime = 0,
                ElapsedTime = 5.5,
                QuarterExpired = true
            };

            // Assert
            Assert.IsTrue(play.QuarterExpired);
            Assert.IsFalse(play.HalfExpired);
            Assert.IsFalse(play.GameExpired);
        }

        [TestMethod]
        public void Play_ExpiresHalf()
        {
            // Arrange
            var play = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second,
                ElapsedTime = 3.2,
                QuarterExpired = true,
                HalfExpired = true
            };

            // Assert
            Assert.IsTrue(play.QuarterExpired);
            Assert.IsTrue(play.HalfExpired);
            Assert.IsFalse(play.GameExpired);
        }

        [TestMethod]
        public void Play_ExpiresGame()
        {
            // Arrange
            var play = new KickoffPlay
            {
                Possession = Possession.Home,
                QuarterExpired = true,
                HalfExpired = true,
                GameExpired = true
            };

            // Assert
            Assert.IsTrue(play.QuarterExpired);
            Assert.IsTrue(play.HalfExpired);
            Assert.IsTrue(play.GameExpired);
        }

        #endregion
    }
}
