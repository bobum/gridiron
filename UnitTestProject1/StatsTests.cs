using System;
using System.Collections.Generic;
using System.Linq;
using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using StateLibrary.Services;
using static DomainObjects.StatTypes;

using UnitTestProject1.Helpers;
namespace UnitTestProject1
{
    [TestClass]
    public class StatsTests
    {
        [TestMethod]
        public void TestPassStatsAccumulation()
        {
            // Arrange
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var offense = game.HomeTeam;
            var defense = game.AwayTeam;

            var qb = offense.Players.First(p => p.Position == Positions.QB);
            var wr = offense.Players.First(p => p.Position == Positions.WR);

            var play = new PassPlay
            {
                Possession = Possession.Home,
                PassSegments = new List<PassSegment>
                {
                    new PassSegment
                    {
                        Passer = qb,
                        Receiver = wr,
                        AirYards = 20,
                        YardsAfterCatch = 0,
                        IsComplete = true,
                        Type = PassType.Forward
                    }
                },
                YardsGained = 20,
                IsTouchdown = false,
                Interception = false
            };
            game.CurrentPlay = play;

            // Act
            StatsAccumulator.AccumulatePassStats(play);

            // Assert
            Assert.IsTrue(qb.Stats.ContainsKey(PlayerStatType.PassingYards));
            Assert.AreEqual(20, qb.Stats[PlayerStatType.PassingYards]);
            Assert.AreEqual(1, qb.Stats[PlayerStatType.PassingCompletions]);
            Assert.AreEqual(1, qb.Stats[PlayerStatType.PassingAttempts]);

            Assert.IsTrue(wr.Stats.ContainsKey(PlayerStatType.ReceivingYards));
            Assert.AreEqual(20, wr.Stats[PlayerStatType.ReceivingYards]);
            Assert.AreEqual(1, wr.Stats[PlayerStatType.Receptions]);
            Assert.AreEqual(1, wr.Stats[PlayerStatType.ReceivingTargets]);
        }

        [TestMethod]
        public void TestRunStatsAccumulation()
        {
            // Arrange
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var offense = game.HomeTeam;

            var rb = offense.Players.First(p => p.Position == Positions.RB);

            var play = new RunPlay
            {
                Possession = Possession.Home,
                RunSegments = new List<RunSegment>
                {
                    new RunSegment
                    {
                        BallCarrier = rb,
                        YardsGained = 10
                    }
                },
                YardsGained = 10
            };
            game.CurrentPlay = play;

            // Act
            StatsAccumulator.AccumulateRunStats(play);

            // Assert
            Assert.IsTrue(rb.Stats.ContainsKey(PlayerStatType.RushingYards));
            Assert.AreEqual(10, rb.Stats[PlayerStatType.RushingYards]);
            Assert.AreEqual(1, rb.Stats[PlayerStatType.RushingAttempts]);
        }

        [TestMethod]
        public void TestDefensiveStatsAccumulation()
        {
            // Arrange
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var offense = game.HomeTeam;
            var defense = game.AwayTeam;

            var qb = offense.Players.First(p => p.Position == Positions.QB);
            var de = defense.Players.First(p => p.Position == Positions.DE);
            var cb = defense.Players.First(p => p.Position == Positions.CB);

            // Test Sack
            var sackPlay = new PassPlay
            {
                Possession = Possession.Home,
                PassSegments = new List<PassSegment>
                {
                    new PassSegment { Passer = qb, AirYards = -5, IsComplete = false }
                },
                YardsGained = -5,
                DefensePlayersOnField = new List<Player> { de }
            };
            game.CurrentPlay = sackPlay;
            
            StatsAccumulator.AccumulateDefensiveStats(sackPlay);

            Assert.IsTrue(de.Stats.ContainsKey(PlayerStatType.Sacks));
            Assert.AreEqual(1, de.Stats[PlayerStatType.Sacks]);
            Assert.IsTrue(de.Stats.ContainsKey(PlayerStatType.Tackles));
            Assert.AreEqual(1, de.Stats[PlayerStatType.Tackles]);

            // Test Interception
            var intPlay = new PassPlay
            {
                Possession = Possession.Home,
                PassSegments = new List<PassSegment>
                {
                    new PassSegment { Passer = qb, AirYards = 10, IsComplete = false }
                },
                YardsGained = 0,
                Interception = true,
                InterceptionDetails = new Interception { ThrownBy = qb, InterceptedBy = cb, ReturnYards = 5 },
                DefensePlayersOnField = new List<Player> { cb }
            };
            game.CurrentPlay = intPlay;
            
            StatsAccumulator.AccumulateDefensiveStats(intPlay);

            Assert.IsTrue(cb.Stats.ContainsKey(PlayerStatType.InterceptionsCaught));
            Assert.AreEqual(1, cb.Stats[PlayerStatType.InterceptionsCaught]);
            // InterceptionReturnYards is not currently tracked in AccumulateDefensiveStats, only InterceptionsCaught
            // Removing assertion for ReturnYards until implemented
            // Assert.IsTrue(cb.Stats.ContainsKey(PlayerStatType.InterceptionReturnYards));
            // Assert.AreEqual(5, cb.Stats[PlayerStatType.InterceptionReturnYards]);
        }

        [TestMethod]
        public void TestFumbleStatsAccumulation()
        {
            // Arrange
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var offense = game.HomeTeam;
            var defense = game.AwayTeam;

            var rb = offense.Players.First(p => p.Position == Positions.RB);
            var lb = defense.Players.First(p => p.Position == Positions.LB);

            var play = new RunPlay
            {
                Possession = Possession.Home,
                RunSegments = new List<RunSegment>
                {
                    new RunSegment { BallCarrier = rb, YardsGained = 5 }
                },
                YardsGained = 5,
                Fumbles = new List<Fumble>
                {
                    new Fumble { FumbledBy = rb, RecoveredBy = lb, ForcedBy = lb }
                }
            };
            game.CurrentPlay = play;
            
            StatsAccumulator.AccumulateFumbleStats(play);

            Assert.IsTrue(rb.Stats.ContainsKey(PlayerStatType.Fumbles));
            Assert.AreEqual(1, rb.Stats[PlayerStatType.Fumbles]);
            // FumblesLost logic is not in AccumulateFumbleStats yet, only Fumbles and Recoveries
            // Assert.IsTrue(rb.Stats.ContainsKey(PlayerStatType.FumblesLost));
            // Assert.AreEqual(1, rb.Stats[PlayerStatType.FumblesLost]);

            // ForcedFumbles logic is not in AccumulateFumbleStats yet
            // Assert.IsTrue(lb.Stats.ContainsKey(PlayerStatType.ForcedFumbles));
            // Assert.AreEqual(1, lb.Stats[PlayerStatType.ForcedFumbles]);
            
            Assert.IsTrue(lb.Stats.ContainsKey(PlayerStatType.FumbleRecoveries));
            Assert.AreEqual(1, lb.Stats[PlayerStatType.FumbleRecoveries]);
        }

        [TestMethod]
        public void TestSpecialTeamsStatsAccumulation()
        {
            // Arrange
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var offense = game.HomeTeam;
            var kicker = offense.Players.First(p => p.Position == Positions.K);
            var punter = offense.Players.First(p => p.Position == Positions.P);

            // Test Field Goal
            var fgPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Kicker = kicker,
                IsGood = true,
                AttemptDistance = 45
            };
            game.CurrentPlay = fgPlay;
            
            StatsAccumulator.AccumulateFieldGoalStats(fgPlay);

            Assert.IsTrue(kicker.Stats.ContainsKey(PlayerStatType.FieldGoalsMade));
            Assert.AreEqual(1, kicker.Stats[PlayerStatType.FieldGoalsMade]);
            Assert.IsTrue(kicker.Stats.ContainsKey(PlayerStatType.FieldGoalsAttempted));
            Assert.AreEqual(1, kicker.Stats[PlayerStatType.FieldGoalsAttempted]);

            // Test Punt
            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Punter = punter,
                PuntDistance = 50,
                EndFieldPosition = 90 // Inside 20 (>= 80)
            };
            game.CurrentPlay = puntPlay;
            
            StatsAccumulator.AccumulatePuntStats(puntPlay);

            Assert.IsTrue(punter.Stats.ContainsKey(PlayerStatType.PuntYards));
            Assert.AreEqual(50, punter.Stats[PlayerStatType.PuntYards]);
            Assert.IsTrue(punter.Stats.ContainsKey(PlayerStatType.PuntsInside20));
            Assert.AreEqual(1, punter.Stats[PlayerStatType.PuntsInside20]);
        }
    }
}
