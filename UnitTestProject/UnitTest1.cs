using System;
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
                HomeTeam = new InArgument<Team>((ctx) => teams.HomeTeam), //MyComplexObject = new InArgument<MyComplexObject>((ctx) => _complexObject)
                AwayTeam = new InArgument<Team>((ctx) => teams.VisitorTeam)
            };

            Game game = WorkflowInvoker.Invoke<Game>(act);

            Assert.AreEqual(52, game.HomeTeam.Players.Count);
            Assert.AreEqual(53, game.AwayTeam.Players.Count);
        }
    }
}
