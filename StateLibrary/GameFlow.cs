using System;
using System.Collections.Generic;
using System.Text;
using DomainObjects;
using Stateless;
using Stateless.Graph;

namespace StateLibrary
{
    public class GameFlow
    {
        enum Trigger
        {
            Snap,
            TeamsSelected,
            CoinTossed
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

        private State _state = State.PreGame;

        StateMachine<State, Trigger> _machine;
        StateMachine<State, Trigger>.TriggerWithParameters<Game> _coinTossedTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<Game> _setSnapTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<Game> _teamsChosenTrigger;

        public GameFlow(Game game)
        {
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setSnapTrigger = _machine.SetTriggerParameters<Game>(Trigger.Snap);
            _coinTossedTrigger = _machine.SetTriggerParameters<Game>(Trigger.CoinTossed);
            _teamsChosenTrigger = _machine.SetTriggerParameters<Game>(Trigger.TeamsSelected);

            _machine.Configure(State.PreGame)
                .Permit(Trigger.TeamsSelected, State.CoinToss);

            _machine.Configure(State.CoinToss)
                .OnEntryFrom(_teamsChosenTrigger, OnTeamsChosen, "Teams Chosen")
                .Permit(Trigger.CoinTossed, State.PrePlay);

            _machine.OnTransitioned(t =>
                Console.WriteLine(
                    $"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

            _machine.Fire(_teamsChosenTrigger, game);
        }

        private void OnTeamsChosen(Game game)
        {
            var test = game;
        }
    }
}
