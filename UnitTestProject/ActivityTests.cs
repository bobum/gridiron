using System;
using System.Activities;
using ActivityLibrary;
using DomainObjects;
using DomainObjects.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject.Helpers;


namespace UnitTestProject
{
    [TestClass]
    public class ActivityTests
    {
        [TestMethod]
        public void Activity_PreGameMakesAGame()
        {
            var teams = new Teams();

            PreGame act = new PreGame
            {
                HomeTeam = new InArgument<Team>((ctx) => teams.HomeTeam),
                AwayTeam = new InArgument<Team>((ctx) => teams.VisitorTeam)
            };

            Game game = WorkflowInvoker.Invoke(act);

            Assert.AreEqual(52, game.HomeTeam.Players.Count);
            Assert.AreEqual(53, game.AwayTeam.Players.Count);
            Assert.AreEqual(3600, game.TimeRemaining);
            Assert.AreEqual(Possession.None, game.Possession);
            Assert.IsNull(game.CurrentPlay);
        }

        [TestMethod]
        public void Activity_CoinTossDoesTheCoinTossAndChangesPossession()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var activity = new CoinToss
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.IsInstanceOfType(newGame.Possession, typeof(Possession));
            Assert.AreNotEqual(newGame.Possession, DomainObjects.Possession.None);
        }

        [TestMethod]
        public void Activity_FumbleKeepsPossession()
        {
            Game newGame = GameHelper.GetNewGame();
            newGame.Possession = Possession.Home;

            var activity = new Fumble
            {
                Game = new InArgument<Game>((ctx) => newGame),
                CurrentPossession = new InArgument<Possession>((ctx) => Possession.Home)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreEqual(newGame.Possession, Possession.Home);
            Assert.IsFalse(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_FumbleChangesPossession()
        {
            Game newGame = GameHelper.GetNewGame();
            newGame.Possession = Possession.Home;

            var activity = new Fumble
            {
                Game = new InArgument<Game>((ctx) => newGame),
                CurrentPossession = new InArgument<Possession>((ctx) => Possession.Away)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreNotEqual(newGame.Possession, Possession.Home);
            Assert.IsTrue(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_InterceptionChangesPossession()
        {
            Game newGame = GameHelper.GetNewGame();

            var activity = new Interception
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.IsTrue(newGame.Possession != Possession.None);
            Assert.IsTrue(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_PrePlaySetsFirstPlayToKickoffChangesPossession()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var activity = new PrePlay
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreEqual(newGame.CurrentPlay.PlayType, PlayType.Kickoff);
        }

        [TestMethod]
        public void Activity_PrePlayHasNoPlaysOnKickoffChangesPossession()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var activity = new PrePlay
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreEqual(newGame.Plays.Count, 0);
        }

        [TestMethod]
        public void Activity_PenaltyCheckDoesAPenaltyCheck()
        {
            Game newGame = GameHelper.GetNewGame();

            var act = new PenaltyCheck()
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke<Game>(act);
            Assert.IsNotNull(newGame.CurrentPlay.Penalties);
        }

        [TestMethod]
        public void Activity_QuarterExpiresQuarters()
        {
            Game newGame = GameHelper.GetNewGame();

            var act = new QuarterExpireCheck()
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };
            newGame.CurrentQuarter.TimeRemaining = 0;
            WorkflowInvoker.Invoke(act);
            Assert.AreEqual(newGame.CurrentQuarter.QuarterType, QuarterType.Second);

            newGame.CurrentQuarter.TimeRemaining = 0;
            WorkflowInvoker.Invoke(act);
            Assert.AreEqual(newGame.CurrentQuarter.QuarterType, QuarterType.Third);

            newGame.CurrentQuarter.TimeRemaining = 0;
            WorkflowInvoker.Invoke(act);
            Assert.AreEqual(newGame.CurrentQuarter.QuarterType, QuarterType.Fourth);
        }

        [TestMethod]
        public void Activity_HalfExpiresHalves()
        {
            Game newGame = GameHelper.GetNewGame();

            var act = new HalfExpireCheck()
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            Assert.AreEqual(newGame.CurrentHalf.HalfType, HalfType.First);

            newGame.CurrentQuarter = newGame.Halves[1].Quarters[0];
            WorkflowInvoker.Invoke(act);
            Assert.AreEqual(newGame.CurrentHalf.HalfType, HalfType.Second);

        }

    }
}