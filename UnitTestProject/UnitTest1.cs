using System;
using System.Activities;
using ActivityLibrary;
using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ItMakesAGame()
        {
            var teams = new Teams();

            PreGame act = new PreGame
            {
                HomeTeam = new InArgument<Team>((ctx) => teams.HomeTeam),
                AwayTeam = new InArgument<Team>((ctx) => teams.VisitorTeam)
            };

            Game game = WorkflowInvoker.Invoke<Game>(act);

            Assert.AreEqual(52, game.HomeTeam.Players.Count);
            Assert.AreEqual(53, game.AwayTeam.Players.Count);
            Assert.AreEqual(3600, game.TimeRemaining);
            Assert.AreEqual(Posession.None, game.Posession);
            Assert.IsNull(game.CurrentPlay.Penalty);
        }

        [TestMethod]
        public void ItDoesTheCoinTossAndChangesPosession()
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

            Game game = WorkflowInvoker.Invoke<Game>(activity);
            Assert.IsInstanceOfType(game.Posession, typeof(Posession));
        }

        [TestMethod]
        public void FumbleChangesPosession()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var activity = new Fumble
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            Game game = WorkflowInvoker.Invoke<Game>(activity);
            Assert.IsTrue(game.Posession != Posession.None);
            Assert.IsTrue(game.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void InterceptionChangesPosession()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var activity = new Interception
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            Game game = WorkflowInvoker.Invoke<Game>(activity);
            Assert.IsTrue(game.Posession != Posession.None);
            Assert.IsTrue(game.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void ItMakesARandomNumber()
        {
            CryptoRandom rng = new CryptoRandom();
            var nextInt = rng.Next();
            Assert.IsInstanceOfType(nextInt, typeof(Int32));

            var nextUnder10 = rng.Next(10);
            Assert.IsTrue(nextUnder10 < 10);

            var nextBetween18And22 = rng.Next(18, 22);
            Assert.IsTrue(nextBetween18And22 >= 18 && nextBetween18And22 < 22);

            var nextDouble = rng.NextDouble();
            Assert.IsInstanceOfType(nextDouble, typeof(Double));
        }

        [TestMethod]
        public void ItHasPenalties()
        {
            Assert.IsTrue(Penalties.List.Count > 0);
        }

        [TestMethod]
        public void ItDoesAPenaltyCheck()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var act = new PenaltyCheck()
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            Game game = WorkflowInvoker.Invoke<Game>(act);
            Assert.IsNotNull(game.CurrentPlay.Penalty);
        }

        [TestMethod]
        public void GameFlowDoesCoinToss()
        {
            var teams = new Teams();

            var result = WorkflowInvoker.Invoke(new GameFlow(),
                new Dictionary<string, object>
                { {"HomeTeam", teams.HomeTeam },
                    {"AwayTeam", teams.VisitorTeam }
                });
            var game = result["Game"] as Game;
            Assert.IsTrue(game.Posession != Posession.None);
        }

        [TestMethod]
        public void PlayFlowPerformsPenaltyCheck()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var result = WorkflowInvoker.Invoke(new PlayFlow(),
                new Dictionary<string, object> {
                    { "Game", newGame }
                });
            var game = result["Game"] as Game;
            Assert.IsNotNull(game.CurrentPlay.Penalty);
        }
    }
}
