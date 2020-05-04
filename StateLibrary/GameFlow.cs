using System;
using DomainObjects;
using Stateless;
using Stateless.Graph;
using StateLibrary.Actions;

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
            Intercepted
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
            PostGame
        }

        //we start in the PreGame state
        private State _state = State.PreGame;

        private readonly StateMachine<State, Trigger> _machine;
        
        public GameFlow(Game game)
        {
            _game = game;

            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);


            //in the PreGame state, on TeamsSelected - transition to CoinTossState
            _machine.Configure(State.PreGame)
                .Permit(Trigger.TeamsSelected, State.CoinToss);

            //when we enter the coin toss state from the TeamsSelected trigger then DoCoinToss!
            _machine.Configure(State.CoinToss)
                .OnEntryFrom(Trigger.TeamsSelected, DoCoinToss, "Teams Chosen")
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
                .OnEntry(DoFieldGoal, "Check if there was a block")
                .PermitIf(Trigger.FieldGoalBlocked, State.FieldGoalBlock, () => _game.CurrentPlay.FieldGoalBlockOccurred)
                .PermitIf(Trigger.Fumble, State.FumbleReturn, () => !_game.CurrentPlay.FieldGoalBlockOccurred);

            _machine.Configure(State.FieldGoalBlock)
                .OnEntry(DoFieldGoalBlockResult, "Check the result of the FG block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.Punt)
                .OnEntry(DoPunt, "Check if there was a block")
                .PermitIf(Trigger.PuntBlocked, State.PuntBlock, () => _game.CurrentPlay.PuntBlockOccurred)
                .PermitIf(Trigger.Fumble, State.FumbleReturn, () => !_game.CurrentPlay.PuntBlockOccurred);

            _machine.Configure(State.PuntBlock)
                .OnEntry(DoPuntBlockResult, "Check the result of the punt block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.PassPlay)
                .OnEntry(DoPunt, "Check if there was an Interception")
                .PermitIf(Trigger.Intercepted, State.InterceptionReturn, () => _game.CurrentPlay.InterceptionOccurred)
                .PermitIf(Trigger.Fumble, State.FumbleReturn, () => !_game.CurrentPlay.InterceptionOccurred);

            _machine.Configure(State.InterceptionReturn)
                .OnEntry(DoPuntInterceptionResult, "Check the result of the interception")
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

            _machine.OnTransitioned(t =>
                Console.WriteLine(
                    $"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            string graph = UmlDotGraph.Format(_machine.GetInfo());

            //fire the teams Selected trigger, which should change the state to CoinToss and launch the DoCoinToss method
            _machine.Fire(Trigger.TeamsSelected);
        }

        private void DoFieldGoalBlockResult()
        {
            throw new NotImplementedException();
        }

        private void DoPuntInterceptionResult()
        {
            throw new NotImplementedException();
        }

        private void DoPuntBlockResult()
        {
            throw new NotImplementedException();
        }

        private void DoPunt()
        {
            throw new NotImplementedException();
        }

        private void DoRunPlay()
        {
            throw new NotImplementedException();
        }

        private void DoFieldGoal()
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
            throw new NotImplementedException();
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
