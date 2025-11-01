using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Actions;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PrePlayFormationTests
    {
        private Game SetupGame(Possession possession = Possession.Home)
        {
            var teams = new Teams();
            var game = new Game
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam,
                Plays = new List<IPlay>()
            };
            // Add a dummy play so the next play is not a kickoff
            game.Plays.Add(new RunPlay { Possession = possession });
            return game;
        }

        [TestMethod]
        public void RunPlay_Offense_Has2WR_1TE()
        {
            var game = SetupGame(Possession.Home);
            var rng = new TestSeedableRandom();
            rng.__NextDouble[0] = 0.0; // Always RUN
            var prePlay = new PrePlay(rng);
            game.WonCoinToss = Possession.Home;
            prePlay.Execute(game);

            var offense = game.CurrentPlay.OffensePlayersOnField;
            Assert.AreEqual(2, offense.Count(p => p.Position == Positions.WR), "Should have 2 WRs for RUN");
            Assert.AreEqual(1, offense.Count(p => p.Position == Positions.TE), "Should have 2 TEs for RUN");
            Assert.AreEqual(11, offense.Count, "Should have 11 offensive players");
        }

        [TestMethod]
        public void PassPlay_Offense_Has3WR_0TE()
        {
            var game = SetupGame(Possession.Home);
            var rng = new TestSeedableRandom();
            rng.__NextDouble[0] = 1.0; // Always PASS
            var prePlay = new PrePlay(rng); // Always PASS
            game.WonCoinToss = Possession.Home;
            prePlay.Execute(game);

            var offense = game.CurrentPlay.OffensePlayersOnField;
            Assert.AreEqual(3, offense.Count(p => p.Position == Positions.WR), "Should have 3 WRs for PASS");
            Assert.AreEqual(0, offense.Count(p => p.Position == Positions.TE), "Should have 1 TE for PASS");
            Assert.AreEqual(11, offense.Count, "Should have 11 offensive players");
        }

        [TestMethod]
        public void RunPlay_Defense_Has2DE_2DT_3LB()
        {
            var game = SetupGame(Possession.Home);
            var rng = new TestSeedableRandom();
            rng.__NextDouble[0] = 0.0; // Always RUN
            var prePlay = new PrePlay(rng); // Always RUN
            game.WonCoinToss = Possession.Home;
            prePlay.Execute(game);

            var defense = game.CurrentPlay.DefensePlayersOnField;
            Assert.AreEqual(2, defense.Count(p => p.Position == Positions.DE), "Should have 2 DEs for RUN");
            Assert.AreEqual(2, defense.Count(p => p.Position == Positions.DT), "Should have 2 DTs for RUN");
            Assert.AreEqual(3, defense.Count(p => p.Position == Positions.LB), "Should have 3 LBs for RUN");
            Assert.AreEqual(11, defense.Count, "Should have 11 defensive players");
        }

        [TestMethod]
        public void PassPlay_Defense_Has1DE_2DT_4LB()
        {
            var game = SetupGame(Possession.Home);
            var rng = new TestSeedableRandom();
            rng.__NextDouble[0] = 1.0; // Always PASS
            var prePlay = new PrePlay(rng); // Always PASS
            game.WonCoinToss = Possession.Home;
            prePlay.Execute(game);

            var defense = game.CurrentPlay.DefensePlayersOnField;
            Assert.AreEqual(1, defense.Count(p => p.Position == Positions.DE), "Should have 1 DE for PASS");
            Assert.AreEqual(2, defense.Count(p => p.Position == Positions.DT), "Should have 2 DTs for PASS");
            Assert.AreEqual(4, defense.Count(p => p.Position == Positions.LB), "Should have 4 LBs for PASS");
            Assert.AreEqual(11, defense.Count, "Should have 11 defensive players");
        }                
    }
}
