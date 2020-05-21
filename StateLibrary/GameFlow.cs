using System;
using DomainObjects;
using Stateless;
using Stateless.Graph;
using StateLibrary.Actions;
using StateLibrary.Plays;

namespace StateLibrary
{
    public class GameFlow
    {
        private readonly Game _game;
        enum Trigger
        {
            Snap,
            TeamsSelected,
            CoinTossed,
            Fumble,
            PlayResult,
            FieldGoalBlocked,
            PuntBlocked,
            Intercepted,
            HalfExpired,
            HalftimeOver,
            GameExpired,
            NextPlay,
            StartGameFlow
        }

        enum State
        {
            PreGame,
            CoinToss,
            PrePlay,
            FieldGoal,
            RunPlay,
            Kickoff,
            Punt,
            PassPlay,
            FieldGoalBlock,
            PuntBlock,
            InterceptionReturn,
            FumbleReturn,
            FieldGoalResult,
            RunPlayResult,
            KickoffResult,
            PuntResult,
            PassPlayResult,
            PostPlay,
            Halftime,
            PostGame,
            InitializeGame
        }

        //we start in the InitializeGame state
        private State _state = State.InitializeGame;

        private readonly StateMachine<State, Trigger> _machine;
        
        public GameFlow(Game game)
        {
            _game = game;

            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _machine.Configure(State.InitializeGame)
                .Permit(Trigger.StartGameFlow, State.PreGame);

            //in the PreGame state, on TeamsSelected - transition to CoinTossState
            _machine.Configure(State.PreGame)
                .OnEntry(DoPreGame, "Pregame festivities")
                .Permit(Trigger.TeamsSelected, State.CoinToss);

            //when we enter the coin toss state from the TeamsSelected trigger then DoCoinToss!
            _machine.Configure(State.CoinToss)
                .OnEntry(DoCoinToss, "Teams Chosen")
                .Permit(Trigger.CoinTossed, State.PrePlay);

            _machine.Configure(State.PrePlay)
                .OnEntry(DoPrePlay, "Determine play, pre-play penalty and snap the ball")
                .PermitIf(Trigger.Snap, State.FieldGoal, () => _game.CurrentPlay.PlayType == PlayType.FieldGoal)
                .PermitIf(Trigger.Snap, State.RunPlay, () => _game.CurrentPlay.PlayType == PlayType.Run)
                .PermitIf(Trigger.Snap, State.Kickoff, () => _game.CurrentPlay.PlayType == PlayType.Kickoff)
                .PermitIf(Trigger.Snap, State.Punt, () => _game.CurrentPlay.PlayType == PlayType.Punt)
                .PermitIf(Trigger.Snap, State.PassPlay, () => _game.CurrentPlay.PlayType == PlayType.Pass);

            //every play state should end in a fumble check state

            //field goal, punt and pass go to interim states of FGBlock, PuntBlocked and Interception
            _machine.Configure(State.FieldGoal)
                .OnEntry(DoFieldGoalBlockCheck, "Check if there was a block")
                .Permit(Trigger.FieldGoalBlocked, State.FieldGoalBlock)
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.FieldGoalBlock)
                .OnEntry(DoFieldGoalBlockResult, "Check the result of the FG block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.Punt)
                .OnEntry(DoPuntBlockCheck, "Check if there was a block")
                .Permit(Trigger.PuntBlocked, State.PuntBlock)
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.PuntBlock)
                .OnEntry(DoPuntBlockResult, "Check the result of the punt block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.PassPlay)
                .OnEntry(DoInterceptionCheck, "Check if there was an Interception")
                .Permit(Trigger.Intercepted, State.InterceptionReturn)
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.InterceptionReturn)
                .OnEntry(DoInterceptionResult, "Check the result of the interception")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.RunPlay)
                .OnEntry(DoRunPlay, "We're Running")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.Kickoff)
                .OnEntry(DoKickoff, "Kicking off the ball")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            //after block checks, interception checks and interception checks - lets pull the play results all together
            //every "Result" action should end in the POST PLAY state
            _machine.Configure(State.FumbleReturn)
                .OnEntry(DoFumbleCheck, "was there a fumble on the play")
                .PermitIf(Trigger.PlayResult, State.FieldGoalResult, () => _game.CurrentPlay.PlayType == PlayType.FieldGoal)
                .PermitIf(Trigger.PlayResult, State.RunPlayResult, () => _game.CurrentPlay.PlayType == PlayType.Run)
                .PermitIf(Trigger.PlayResult, State.KickoffResult, () => _game.CurrentPlay.PlayType == PlayType.Kickoff)
                .PermitIf(Trigger.PlayResult, State.PuntResult, () => _game.CurrentPlay.PlayType == PlayType.Punt)
                .PermitIf(Trigger.PlayResult, State.PassPlayResult, () => _game.CurrentPlay.PlayType == PlayType.Pass);

            _machine.Configure(State.FieldGoalResult)
                .OnEntry(DoFieldGoalResult, "the kick is up...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.RunPlayResult)
                .OnEntry(DoRunPlayResult, "the runner is brought down")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.KickoffResult)
                .OnEntry(DoKickoffResult, "the kick is away...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.PuntResult)
                .OnEntry(DoPuntResult, "the punt is high...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.PassPlayResult)
                .OnEntry(DoPassPlayResult, "the receiver is brought down...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.PostPlay)
                .OnEntry(DoPostPlay, "play is over")
                .Permit(Trigger.HalfExpired, State.Halftime)
                .Permit(Trigger.GameExpired, State.PostGame)
                .Permit(Trigger.NextPlay, State.PrePlay);

            _machine.Configure(State.Halftime)
                .OnEntry(DoHalftime, "The band takes the field at halftime")
                .PermitIf(Trigger.HalftimeOver, State.PrePlay);

            _machine.OnTransitioned(t =>
                Console.WriteLine(
                    $"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            string graph = UmlDotGraph.Format(_machine.GetInfo());

            //fire the teams Selected trigger, which should change the state to CoinToss and launch the DoCoinToss method
            _machine.Fire(Trigger.StartGameFlow);
        }

        private void DoPreGame()
        {
            var preGame = new PreGame();
            preGame.Execute(_game);
            _machine.Fire(Trigger.TeamsSelected);
        }

        private void DoHalftime()
        {
            throw new NotImplementedException();
        }

        private void DoPassPlayResult()
        {
            throw new NotImplementedException();
        }

        private void DoPuntResult()
        {
            throw new NotImplementedException();
        }

        private void DoKickoffResult()
        {
            throw new NotImplementedException();
        }

        private void DoRunPlayResult()
        {
            throw new NotImplementedException();
        }

        private void DoFieldGoalResult()
        {
            throw new NotImplementedException();
        }

        private void DoInterceptionCheck()
        {
            throw new NotImplementedException();
        }

        private void DoPostPlay()
        {
            throw new NotImplementedException();
        }

        private void DoFieldGoalBlockResult()
        {
            throw new NotImplementedException();
        }

        private void DoInterceptionResult()
        {
            throw new NotImplementedException();
        }

        private void DoPuntBlockResult()
        {
            throw new NotImplementedException();
        }

        private void DoPuntBlockCheck()
        {
            throw new NotImplementedException();
        }

        private void DoRunPlay()
        {
            throw new NotImplementedException();
        }

        private void DoFieldGoalBlockCheck()
        {
            throw new NotImplementedException();
        }

        private void DoFumbleCheck()
        {
            throw new NotImplementedException();
        }

        private void DoKickoff()
        {
            //gotta do the kickoff in here
            var kickoff = new Kickoff();
            kickoff.Execute(_game);
            _machine.Fire(Trigger.Fumble);
        }

        private void DoPrePlay()
        {
            //in here we will need to do a pre-snap penalty check, which could move the line of scrimmage, ejection of a player etc...
            //we will also determine which kind of play happens
            var prePlay = new PrePlay();
            prePlay.Execute(_game);

            var penaltyCheck = new PenaltyCheck();
            penaltyCheck.Execute(_game, PenaltyOccured.Before);

            var snap = new Snap();
            snap.Execute(_game);
            _machine.Fire(Trigger.Snap);
        }

        private void DoCoinToss()
        {
            var coinToss = new CoinToss();
            coinToss.Execute(_game);
            _machine.Fire(Trigger.CoinTossed);
        }
    }
}
