using System;
using System.Activities;
using ActivityLibrary;
using DomainObjects;
using DomainObjects.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
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

            Game game = WorkflowInvoker.Invoke<Game>(act);

            Assert.AreEqual(52, game.HomeTeam.Players.Count);
            Assert.AreEqual(53, game.AwayTeam.Players.Count);
            Assert.AreEqual(3600, game.TimeRemaining);
            Assert.AreEqual(Posession.None, game.Posession);
            Assert.IsNull(game.CurrentPlay);
        }

        [TestMethod]
        public void Activity_CoinTossDoesTheCoinTossAndChangesPosession()
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

            WorkflowInvoker.Invoke<Game>(activity);
            Assert.IsInstanceOfType(newGame.Posession, typeof(Posession));
            Assert.AreNotEqual(newGame.Posession, DomainObjects.Posession.None);
        }

        [TestMethod]
        public void Activity_FumbleKeepsPosession()
        {
            Game newGame = GetNewGame();
            newGame.Posession = Posession.Home;

            var activity = new Fumble
            {
                Game = new InArgument<Game>((ctx) => newGame),
                CurrentPosession = new InArgument<Posession>((ctx) => Posession.Home)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreEqual(newGame.Posession, Posession.Home);
            Assert.IsFalse(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_FumbleChangesPosession()
        {
            Game newGame = GetNewGame();
            newGame.Posession = Posession.Home;

            var activity = new Fumble
            {
                Game = new InArgument<Game>((ctx) => newGame),
                CurrentPosession = new InArgument<Posession>((ctx) => Posession.Away)
            };

            WorkflowInvoker.Invoke(activity);
            Assert.AreNotEqual(newGame.Posession, Posession.Home);
            Assert.IsTrue(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_InterceptionChangesPosession()
        {
            Game newGame = GetNewGame();

            var activity = new Interception
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke<Game>(activity);
            Assert.IsTrue(newGame.Posession != Posession.None);
            Assert.IsTrue(newGame.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void Activity_PrePlaySetsFirstPlayToKickoffChangesPosession()
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

            WorkflowInvoker.Invoke<Game>(activity);
            Assert.AreEqual(newGame.CurrentPlay.PlayType, PlayType.Kickoff);
        }

        [TestMethod]
        public void Activity_PrePlayHasNoPlaysOnKickoffChangesPosession()
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

            WorkflowInvoker.Invoke<Game>(activity);
            Assert.AreEqual(newGame.Plays.Count, 0);
        }

        [TestMethod]
        public void DomainObject_GeneratorMakesARandomNumber()
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
        public void DomainObject_PenaltiesHasPenalties()
        {
            Assert.IsTrue(Penalties.List.Count > 0);
        }

        [TestMethod]
        public void Activity_PenaltyCheckDoesAPenaltyCheck()
        {
            Game newGame = GetNewGame();

            var act = new PenaltyCheck()
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            WorkflowInvoker.Invoke<Game>(act);
            Assert.IsNotNull(newGame.CurrentPlay.Penalties);
        }

        [TestMethod]
        public void FirstHalf_CreatesProperQuarters()
        {
            var half = new FirstHalf();

            Assert.IsTrue(half.Quarters[0].QuarterType == QuarterType.First);
            Assert.IsTrue(half.Quarters[1].QuarterType == QuarterType.Second);

        }

        [TestMethod]
        public void SecondHalf_CreatesProperQuarters()
        {
            var half = new SecondHalf();

            Assert.IsTrue(half.Quarters[0].QuarterType == QuarterType.Third);
            Assert.IsTrue(half.Quarters[1].QuarterType == QuarterType.Fourth);

        }


        public Game GetNewGame()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var prePlayActivity = new PrePlay
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };
            
            return WorkflowInvoker.Invoke<Game>(prePlayActivity);
        }
    }
}