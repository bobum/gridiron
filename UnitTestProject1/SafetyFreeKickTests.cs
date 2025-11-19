using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using StateLibrary.PlayResults;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests to verify that a free kick occurs after a safety with correct possession change.
    /// Per NFL rules: After a safety, the team scored upon must perform a free kick (punt or place kick)
    /// from their own 20-yard line to the team that scored the safety.
    /// </summary>
    [TestClass]
    public class SafetyFreeKickTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void Safety_ResultsInFreeKickFromTwentyYardLine()
        {
            // Arrange - Set up a game where home team will be tackled in their own end zone
            var game = _testGame.GetGame();
            var rng = new SeedableRandom();

            // Add initial kickoff to simulate game in progress
            var kickoff = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false
            };
            game.Plays.Add(kickoff);

            // Set up a run play where home team commits a safety
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                YardsGained = -5,
                StartFieldPosition = 3,
                EndFieldPosition = 0,
                IsSafety = true,
                PossessionChange = true,
                Result = new InMemoryLogger<RunPlay>()
            };

            game.CurrentPlay = runPlay;
            game.FieldPosition = 3;
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - Process the safety through result handling
            var runResult = new RunResult();
            runResult.Execute(game);

            // Verify safety was scored correctly
            Assert.IsTrue(runPlay.IsSafety, "Play should be a safety");
            Assert.AreEqual(0, game.HomeScore, "Home score should be 0");
            Assert.AreEqual(2, game.AwayScore, "Away should have scored 2 points for safety");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change after safety");

            // Add the safety play to the game's plays list (simulating PostPlay)
            game.Plays.Add(runPlay);

            // Now execute PrePlay to determine the next play
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert - Next play should be a free kick (Punt) from the 20-yard line
            Assert.IsNotNull(game.CurrentPlay, "There should be a current play");
            Assert.AreEqual(PlayType.Punt, game.CurrentPlay.PlayType,
                "Next play after safety should be a Punt (free kick)");

            // The team that committed the safety (Home) should be kicking
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team (who committed safety) should have possession for free kick");

            // Field position should be at home's 20-yard line (position 20 in absolute system)
            Assert.AreEqual(20, game.FieldPosition,
                "Free kick should be from the 20-yard line");
        }

        [TestMethod]
        public void Safety_ByAwayTeam_HomeTeamPerformsFreeKick()
        {
            // Arrange - Set up a game where away team forces a safety on home team
            var game = _testGame.GetGame();
            var rng = new SeedableRandom();

            // Add initial kickoff
            var kickoff = new KickoffPlay
            {
                Possession = Possession.Away,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false
            };
            game.Plays.Add(kickoff);

            // Away team sacked in their own end zone by home defense
            var passPlay = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Second,
                YardsGained = -8,
                StartFieldPosition = 6,
                EndFieldPosition = 0,
                IsSafety = true,
                PossessionChange = true,
                Result = new InMemoryLogger<PassPlay>()
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 6;
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - Process the safety
            var passResult = new PassResult();
            passResult.Execute(game);

            // Verify safety scored
            Assert.AreEqual(2, game.HomeScore, "Home should have 2 points for safety");
            Assert.AreEqual(0, game.AwayScore, "Away score should be 0");

            game.Plays.Add(passPlay);

            // Execute PrePlay
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert - Away team should perform free kick
            Assert.AreEqual(PlayType.Punt, game.CurrentPlay.PlayType,
                "Next play should be a Punt (free kick)");
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Away team (who committed safety) should kick");

            // Field position should be at away's 20-yard line (position 80 in absolute system where 0-49 is home territory, 50-100 is away)
            Assert.AreEqual(80, game.FieldPosition,
                "Free kick should be from away team's 20-yard line (absolute position 80)");
        }

        [TestMethod]
        public void SafetyOnPunt_ResultsInFreeKick()
        {
            // Arrange - Punt results in safety (bad snap into end zone)
            var game = _testGame.GetGame();
            var rng = new SeedableRandom();

            var kickoff = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0
            };
            game.Plays.Add(kickoff);

            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                GoodSnap = false,
                YardsGained = -4,
                StartFieldPosition = 4,
                EndFieldPosition = 0,
                IsSafety = true,
                PossessionChange = true,
                Punter = new Player { LastName = "Punter", Position = Positions.P },
                Result = new InMemoryLogger<PuntPlay>()
            };

            game.CurrentPlay = puntPlay;
            game.FieldPosition = 4;
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            Assert.AreEqual(2, game.AwayScore, "Away should score 2 for safety");

            game.Plays.Add(puntPlay);

            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert - Should result in free kick
            Assert.AreEqual(PlayType.Punt, game.CurrentPlay.PlayType,
                "Next play after punt safety should be a Punt (free kick)");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team should perform free kick");
            Assert.AreEqual(20, game.FieldPosition,
                "Free kick from home's 20-yard line");
        }

        [TestMethod]
        public void Safety_CompleteFlow_FreeKickAndPossessionChange()
        {
            // This is the comprehensive integration test that verifies the COMPLETE flow:
            // 1. Safety occurs (Home commits, Away scores 2 points)
            // 2. Next play is a free kick (punt) from Home's 20-yard line
            // 3. Free kick is executed
            // 4. After free kick, possession changes to Away team

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(12345); // Fixed seed for reproducibility
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Initial kickoff to start game
            var kickoff = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false,
                Result = logger
            };
            game.Plays.Add(kickoff);

            // ===== STEP 1: Safety occurs =====
            // Home team gets tackled in their own end zone
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.Second,
                StartFieldPosition = 2,
                YardsGained = -2,
                EndFieldPosition = 0,
                Result = logger
            };

            game.CurrentPlay = runPlay;
            game.FieldPosition = 2;
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Execute the run play (which will set IsSafety in the actual execution)
            runPlay.IsSafety = true;
            runPlay.PossessionChange = true;

            var runResult = new RunResult();
            runResult.Execute(game);

            // Verify safety scored
            Assert.IsTrue(runPlay.IsSafety, "Play should be marked as safety");
            Assert.AreEqual(0, game.HomeScore, "Home should have 0 points");
            Assert.AreEqual(2, game.AwayScore, "Away should have 2 points from safety");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change flag set");

            // Add safety play to game (simulating PostPlay)
            game.Plays.Add(runPlay);

            // ===== STEP 2: PrePlay determines next play should be free kick =====
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Verify free kick setup
            Assert.IsNotNull(game.CurrentPlay, "Should have a current play");
            Assert.AreEqual(PlayType.Punt, game.CurrentPlay.PlayType,
                "Next play after safety MUST be a Punt (free kick)");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team (who committed safety) must perform the free kick");
            Assert.AreEqual(20, game.FieldPosition,
                "Free kick MUST be from Home's 20-yard line (absolute position 20)");
            Assert.AreEqual(Downs.None, game.CurrentPlay.Down,
                "Free kick should have Down = None (it's not a regular down)");

            // Verify players are on field for the punt
            var freeKickPunt = (PuntPlay)game.CurrentPlay;
            Assert.IsNotNull(freeKickPunt.Punter, "Free kick should have a punter assigned");
            Assert.AreEqual(11, freeKickPunt.OffensePlayersOnField.Count,
                "Should have 11 offensive players for free kick");
            Assert.AreEqual(11, freeKickPunt.DefensePlayersOnField.Count,
                "Should have 11 defensive players for free kick");

            // ===== STEP 3: Execute the free kick (punt) =====
            var punt = new Punt(rng);
            punt.Execute(game);

            // Let the punt result process
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // ===== STEP 4: Verify possession changed to team that scored safety =====
            Assert.IsTrue(freeKickPunt.PossessionChange,
                "Free kick should result in possession change");

            // Add free kick to plays
            game.Plays.Add(freeKickPunt);

            // Determine next play to see who has possession
            var prePlay2 = new PrePlay(rng);
            prePlay2.Execute(game);

            // The next play should be for the Away team (who scored the safety)
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "After free kick, possession should be with Away team (who scored the safety)");
            Assert.AreEqual(Downs.First, game.CurrentDown,
                "After free kick, should be 1st down");
            Assert.AreEqual(10, game.YardsToGo,
                "After free kick, should be 10 yards to go");

            // Verify we're not doing another kickoff or punt
            Assert.IsTrue(game.CurrentPlay.PlayType == PlayType.Run || game.CurrentPlay.PlayType == PlayType.Pass,
                "After free kick, should be normal offensive play (run or pass)");
        }
    }
}
