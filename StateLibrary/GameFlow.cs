using System;
using System.Collections.Generic;
using System.Text;
using DomainObjects;
using Stateless;
using Stateless.Graph;
using StateLibrary.Actions;

namespace StateLibrary
{
    public class GameFlow
    {
        private Game _game;
        enum Trigger
        {
            Snap,
            TeamsSelected,
            CoinTossed,
            Fumble
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
            PassResult,
            PostPlay,
            PostGame
        }

        //we start in the PreGame state
        private State _state = State.PreGame;

        StateMachine<State, Trigger> _machine;

        StateMachine<State, Trigger>.TriggerWithParameters<Game> _coinTossedTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<Game> _setSnapTrigger;

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
                .OnEntry(DoPrePlay, "Determine play, preplay penalty and snap the ball")
                .PermitIf(Trigger.Snap, State.FieldGoal, () => _game.CurrentPlay.PlayType == PlayType.FieldGoal)
                .PermitIf(Trigger.Snap, State.RunPlay, () => _game.CurrentPlay.PlayType == PlayType.Run)
                .PermitIf(Trigger.Snap, State.Kickoff, () => _game.CurrentPlay.PlayType == PlayType.Kickoff)
                .PermitIf(Trigger.Snap, State.Punt, () => _game.CurrentPlay.PlayType == PlayType.Punt)
                .PermitIf(Trigger.Snap, State.PassPlay, () => _game.CurrentPlay.PlayType == PlayType.Pass);

            _machine.Configure(State.Kickoff)
                .OnEntry(DoKickoff, "Kicking off the ball");

            _machine.OnTransitioned(t =>
                Console.WriteLine(
                    $"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            //fire the teams Selected trigger, which should change the state to CoinToss and launch the DoCoinToss method
            _machine.Fire(Trigger.TeamsSelected);
        }

        private void DoKickoff()
        {
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
