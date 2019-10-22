using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Activities;
using ActivityLibrary;
using DomainObjects;

namespace UnitTestProject
{
    [TestClass]
    public class FlowTests
    {
        [TestMethod]
        public void Flow_GameFlowDoesCoinToss()
        {
            var teams = new Teams();

            var result = WorkflowInvoker.Invoke(new GameFlow(),
                new Dictionary<string, object>
                { {"HomeTeam", teams.HomeTeam },
                    {"AwayTeam", teams.VisitorTeam }
                });
            var game = result["Game"] as Game;
            Assert.IsTrue(game.Possession != Possession.None);
        }

        [TestMethod]
        public void Flow_GameFlowSetsKickoffAsFirstPlay()
        {
            var teams = new Teams();

            var result = WorkflowInvoker.Invoke(new GameFlow(),
                new Dictionary<string, object>
                { {"HomeTeam", teams.HomeTeam },
                    {"AwayTeam", teams.VisitorTeam }
                });
            var game = result["Game"] as Game;
            Assert.AreEqual(game.CurrentPlay.PlayType, PlayType.Kickoff);
        }

        [TestMethod]
        public void Flow_PlayFlowPerformsPenaltyCheck()
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
            Assert.IsNotNull(game.CurrentPlay.Penalties);
        }
    }
}
