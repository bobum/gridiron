﻿using System;
using System.Activities;
using ActivityLibrary;
using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual(Posession.Home, game.Posession);
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

            var act = new CoinToss
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            Game game = WorkflowInvoker.Invoke<Game>(act);
            Console.WriteLine(game.Posession);
            Assert.IsInstanceOfType(game.Posession, typeof(Posession));
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
    }
}
