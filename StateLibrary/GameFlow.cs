using System;
using DomainObjects;
using Stateless;
using Stateless.Graph;
using StateLibrary.Actions;
using StateLibrary.PlayResults;
using StateLibrary.Plays;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;
using Fumble = StateLibrary.Actions.Fumble;
using Interception = StateLibrary.Actions.Interception;

namespace StateLibrary
{
    public class GameFlow
    {
        private readonly Game _game;
        enum Trigger
        {
            Snap,
            WarmupsCompleted,
            CoinTossed,
            Fumble,
            PlayResult,
            HalfExpired,
            HalftimeOver,
            GameExpired,
            NextPlay,
            StartGameFlow,
            QuarterOver
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
            FumbleReturn,
            FieldGoalResult,
            RunPlayResult,
            KickoffResult,
            PuntResult,
            PassPlayResult,
            PostPlay,
            Halftime,
            PostGame,
            InitializeGame,
            QuarterExpired
        }

        private readonly StateMachine<State, Trigger>.TriggerWithParameters<bool> _nextPlayTrigger;

        //we start in the InitializeGame state
        private State _state = State.InitializeGame;

        private readonly StateMachine<State, Trigger> _machine;
        
        public GameFlow(Game game)
        {
            _game = game;

            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);
            _nextPlayTrigger = _machine.SetTriggerParameters<bool>(Trigger.NextPlay);

            _machine.Configure(State.InitializeGame)
                .Permit(Trigger.StartGameFlow, State.PreGame);

            //in the PreGame state, on WarmupsCompleted - transition to CoinTossState
            _machine.Configure(State.PreGame)
                .OnEntry(DoPreGame, "Pregame festivities")
                .Permit(Trigger.WarmupsCompleted, State.CoinToss);

            //when we enter the coin toss state from the WarmupsCompleted trigger then DoCoinToss!
            _machine.Configure(State.CoinToss)
                .OnEntry(DoCoinToss, "Teams Chosen")
                .Permit(Trigger.CoinTossed, State.PrePlay);

            //PrePlay state = huddle.  It is where the offensive and defensive plays are determined
            //we check for motion and substitution penalties etc
            //we then snap the ball and make sure the snap was good
            _machine.Configure(State.PrePlay)
                .OnEntry(DoPrePlay, "Determine play, pre-play penalty and snap the ball")
                .PermitIf(Trigger.Snap, State.FieldGoal, () => _game.CurrentPlay.PlayType == PlayType.FieldGoal)
                .PermitIf(Trigger.Snap, State.RunPlay, () => _game.CurrentPlay.PlayType == PlayType.Run)
                .PermitIf(Trigger.Snap, State.Kickoff, () => _game.CurrentPlay.PlayType == PlayType.Kickoff)
                .PermitIf(Trigger.Snap, State.Punt, () => _game.CurrentPlay.PlayType == PlayType.Punt)
                .PermitIf(Trigger.Snap, State.PassPlay, () => _game.CurrentPlay.PlayType == PlayType.Pass);

            //every play state should end in a fumble check state

            //field goal, punt and pass go to interim states of FGBlock, PuntBlocked and Interception
            //their flow is like this:
            //DoPlay - this initiates the play and is akin to saying "the ball is in the air..."
            //ActionCheck - this asks "was there a block/interception?"
            //ActionResult - if there was a block/interception what happened?
            //FumbleCheck - after any action, we always could fumble
            //FumbleResult - what happened?
            //PlayResult - tie it all together and close out the play
            _machine.Configure(State.FieldGoal)
                .OnEntry(DoFieldGoalPlay, "Check if there was a block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.Punt)
                .OnEntry(DoPuntPlay, "Check if there was a block")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.PassPlay)
                .OnEntry(DoPassPlay, "Check if there was an Interception")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.RunPlay)
                .OnEntry(DoRunPlay, "We're Running")
                .Permit(Trigger.Fumble, State.FumbleReturn);

            _machine.Configure(State.Kickoff)
                .OnEntry(DoKickoffPlay, "Kicking off the ball")
                .Permit(Trigger.Fumble, State.FumbleReturn);

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
                .OnEntry(DoPuntResult, "Punt team exits the field...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.PassPlayResult)
                .OnEntry(DoPassPlayResult, "the receiver is brought down...")
                .Permit(Trigger.PlayResult, State.PostPlay);

            _machine.Configure(State.PostPlay)
                .OnEntry(DoPostPlay, "play is over, check for penalty, score, quarter expiration")
                .PermitDynamic(_nextPlayTrigger,
                    quarterExpired => quarterExpired ? State.QuarterExpired : State.PrePlay);

            _machine.Configure(State.QuarterExpired)
                .OnEntry(DoQuarterExpire, "The teams change endzones...")
                .Permit(Trigger.QuarterOver, State.PrePlay)
                .Permit(Trigger.HalfExpired, State.Halftime);

            _machine.Configure(State.Halftime)
                .OnEntry(DoHalftime, "The band takes the field at halftime")
                .Permit(Trigger.HalftimeOver, State.PrePlay)
                .Permit(Trigger.GameExpired, State.PostGame);

            _machine.Configure(State.PostGame)
                .OnEntry(DoPostGame, "Game over folks!");

            _machine.OnTransitioned(t =>
                Console.WriteLine(
                    $"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));
        }

        public Game Execute()
        {
            //fire the teams Selected trigger, which should change the state to CoinToss and launch the DoCoinToss method
            _machine.Fire(Trigger.StartGameFlow);
            return _game;
        }

        #region GAME EVENT METHODS

        private void DoPreGame()
        {
            var preGame = new PreGame();
            preGame.Execute(_game);

            _machine.Fire(Trigger.WarmupsCompleted);
        }

        private void DoCoinToss()
        {
            var coinToss = new CoinToss();
            coinToss.Execute(_game);

            _machine.Fire(Trigger.CoinTossed);
        }

        private void DoQuarterExpire()
        {
            throw new NotImplementedException();
        }

        private void DoHalftime()
        {
            throw new NotImplementedException();
        }

        private void DoPostGame()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region PLAY EVENT METHODS

        private void DoPrePlay()
        {
            //PrePlay state = huddle.  It is where the offensive and defensive plays are determined
            //we check for motion and substitution penalties etc
            //we then snap the ball and make sure the snap was good
            var prePlay = new PrePlay();
            prePlay.Execute(_game);

            var penaltyCheck = new PenaltyCheck(PenaltyOccured.Before);
            penaltyCheck.Execute(_game);

            var snap = new Snap();
            snap.Execute(_game);

            _machine.Fire(Trigger.Snap);
        }

        private void DoKickoffPlay()
        {
            //gotta do the kickoff in here
            var kickoff = new Kickoff();
            kickoff.Execute(_game);

            _machine.Fire(Trigger.Fumble);
        }

        private void DoRunPlay()
        {
            var runPlay = new Run();
            runPlay.Execute(_game);

            _machine.Fire(Trigger.Fumble);
        }

        private void DoPassPlay()
        {
            //Check if there was a block & if there was, assemble the result
            var interceptionCheck = new InterceptionOccurredSkillsCheck();
            interceptionCheck.Execute(_game);

            if (interceptionCheck.Occurred)
            {
                //Intercepted!!!
                InterceptionOccurred();
            }
            else
            {
                //no interception, kick is up...
                var passPlay = new Pass();
                passPlay.Execute(_game);
            }

            _machine.Fire(Trigger.Fumble);
        }

        private void DoPuntPlay()
        {
            //Check if there was a block & if there was, assemble the result
            var blockedCheck = new PuntBlockOccurredSkillsCheck();
            blockedCheck.Execute(_game);

            if (blockedCheck.Occurred)
            {
                //Blocked!  Ball is loose!!
                FumbleOccurred();
            }
            else
            {
                //no block, kick is up...
                var punt = new Punt();
                punt.Execute(_game);
            }

            _machine.Fire(Trigger.Fumble);
        }

        private void DoFieldGoalPlay()
        {
            //Check if there was a block & if there was, assemble the result
            var blockedCheck = new FieldGoalBlockOccurredSkillsCheck();
            blockedCheck.Execute(_game);

            if (blockedCheck.Occurred)
            {
                //Blocked!  Ball is loose!!
                FumbleOccurred();
            }
            else
            {
                //no block, kick is up...
                var fieldGoal = new FieldGoal();
                fieldGoal.Execute(_game);
            }

            _machine.Fire(Trigger.Fumble);
        }

        private void DoFumbleCheck()
        {
            var fumbleCheck = new FumbleOccurredSkillsCheck();
            fumbleCheck.Execute(_game);

            //if true - possession skills check and fumble action
            if (fumbleCheck.Occurred)
            {
                FumbleOccurred();
            }

            _machine.Fire(Trigger.PlayResult);
        }

        private void DoPostPlay()
        {
            //check for penalties during and after the play, scores, injuries, quarter expiration
            var postPlay = new PostPlay();
            postPlay.Execute(_game);

            _machine.Fire(_nextPlayTrigger, _game.CurrentPlay.QuarterExpired);
        }

        #endregion

        #region PLAY RESULT METHODS

        private void DoPassPlayResult()
        {
            var passResult = new PassResult();
            passResult.Execute(_game);

            _machine.Fire(Trigger.PlayResult);
        }

        private void DoPuntResult()
        {
            var puntResult = new PuntResult();
            puntResult.Execute(_game);

            _machine.Fire(Trigger.PlayResult);
        }

        private void DoKickoffResult()
        {
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(_game);

            _machine.Fire(Trigger.PlayResult);
        }

        private void DoRunPlayResult()
        {
            var runResult = new RunResult();
            runResult.Execute(_game);

            _machine.Fire(Trigger.PlayResult);
        }

        private void DoFieldGoalResult()
        {
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(_game);

            _machine.Fire(Trigger.PlayResult);
        }

        #endregion

        #region NON STATE MACHINE METHODS
        
        public string GetGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }

        /// <summary>
        /// If we determine at anytime there has been a fumble, we use this method to determine who took possession
        /// </summary>
        private void FumbleOccurred()
        {
            var possessionChangeResult = new FumblePossessionChangeSkillsCheckResult();
            possessionChangeResult.Execute(_game);

            var fumbleResult = new Fumble(possessionChangeResult.Possession);
            fumbleResult.Execute(_game);
        }

        /// <summary>
        /// If we determine at anytime there has been an interception, we use this method to determine who took possession
        /// </summary>
        private void InterceptionOccurred()
        {
            var possessionChangeResult = new InterceptionPossessionChangeSkillsCheckResult();
            possessionChangeResult.Execute(_game);

            var interceptionResult = new Interception();
            interceptionResult.Execute(_game);
        }

        #endregion
    }
}
