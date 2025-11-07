using System;
using DomainObjects;
using DomainObjects.Helpers;
using DomainObjects.Time;
using Microsoft.Extensions.Logging;
using Stateless;
using Stateless.Graph;
using StateLibrary.Actions;
using StateLibrary.PlayResults;
using StateLibrary.Plays;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;
using Fumble = StateLibrary.Actions.Fumble;
using Interception = StateLibrary.Actions.Interception;
using Penalty = StateLibrary.Actions.Penalty;

namespace StateLibrary
{
    public class GameFlow
    {
        private readonly Game _game;

        // Injected RNG (was: private CryptoRandom _rng = new CryptoRandom();)
        private readonly ISeedableRandom _rng;

        // Injected logger
        private readonly ILogger<GameFlow> _logger;

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

        // Constructor now takes ICryptoRandom and ILogger as dependencies
        public GameFlow(Game game, ISeedableRandom rng, ILogger<GameFlow> logger)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set the game's logger so it can be used for play-by-play logging
            _game.Logger = logger;

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
                .PermitIf(Trigger.Snap, State.PassPlay, () => _game.CurrentPlay.PlayType == PlayType.Pass)
                .Permit(Trigger.PlayResult, State.PostPlay);

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
                .Permit(Trigger.HalfExpired, State.Halftime)
                .Permit(Trigger.GameExpired, State.PostGame);

            _machine.Configure(State.Halftime)
                .OnEntry(DoHalftime, "The band takes the field at halftime")
                .Permit(Trigger.HalftimeOver, State.PrePlay);

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
            var coinToss = new CoinToss(_rng);
            coinToss.Execute(_game);

            _machine.Fire(Trigger.CoinTossed);
        }

        private void DoQuarterExpire()
        {
            //need to actually determine when the quarters get changed etc so that this switch is really done the RIGHT way...
            var quarterExpired = new QuarterExpired();
            quarterExpired.Execute(_game);

            switch (_game.CurrentQuarter.QuarterType)
            {
                case QuarterType.Third:
                    _machine.Fire(Trigger.HalfExpired);
                    break;
                case QuarterType.GameOver:
                    _machine.Fire(Trigger.GameExpired);
                    break;
                default:
                    _machine.Fire(Trigger.QuarterOver);
                    break;
            }
        }

        private void DoHalftime()
        {
            var halftime = new Halftime();
            halftime.Execute(_game);

            _machine.Fire(Trigger.HalftimeOver);
        }

        private void DoPostGame()
        {
            var postGame = new PostGame();
            postGame.Execute(_game);

            //TODO: Game has ended - what happens now that we have a complete _game object ready to go?
        }

        #endregion

        #region PLAY EVENT METHODS

        private void DoPrePlay()
        {
            //PrePlay state = huddle.  It is where the offensive and defensive plays are determined
            //we check for motion and substitution penalties etc
            //we then snap the ball and make sure the snap was good
            var prePlay = new PrePlay(_rng);
            prePlay.Execute(_game);

            PenaltyCheck(PenaltyOccuredWhen.Before);

            if (_game.CurrentPlay.Penalties.Count > 0)
            {
                //in here - at some point - if there is a presnap penalty - we need to handle it and go all the way to post play
                _machine.Fire(Trigger.PlayResult);
            }
            else
            {
                var snap = new Snap(_rng);
                snap.Execute(_game);

                _machine.Fire(Trigger.Snap);
            }
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
            var runPlay = new Run(_rng);
            runPlay.Execute(_game);

            _machine.Fire(Trigger.Fumble);
        }

        private void DoPassPlay()
        {
            //Check if there was a block & if there was, assemble the result
            var interceptionCheck = new InterceptionOccurredSkillsCheck(_rng);
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
            var blockedCheck = new PuntBlockOccurredSkillsCheck(_rng);
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
            var blockedCheck = new FieldGoalBlockOccurredSkillsCheck(_rng);
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
            var fumbleCheck = new FumbleOccurredSkillsCheck(_rng);
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
            //if we have a pre-snap penalty - no need to check for others
            if (_game.CurrentPlay.Penalties.Count == 0)
            {
                PenaltyCheck(PenaltyOccuredWhen.During);
                PenaltyCheck(PenaltyOccuredWhen.After);
            }

            //if we have a penalty/penalties then lets apply it/them
            if (_game.CurrentPlay.Penalties.Count > 0)
            {
                var penaltyResult = new Penalty();
                penaltyResult.Execute(_game);
            }

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
            var possessionChangeResult = new FumblePossessionChangeSkillsCheckResult(_rng);
            possessionChangeResult.Execute(_game);

            var fumbleResult = new Fumble(possessionChangeResult.Possession);
            fumbleResult.Execute(_game);
        }

        private void PenaltyCheck(PenaltyOccuredWhen penaltyOccuredWhen)
        {
            var penaltyOccurredSkillsCheck = new PenaltyOccurredSkillsCheck(penaltyOccuredWhen, _rng);
            penaltyOccurredSkillsCheck.Execute(_game);

            if (penaltyOccurredSkillsCheck.Occurred)
            {
                var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penaltyOccurredSkillsCheck.Penalty);
                penaltySkillsCheckResult.Execute(_game);
            }
        }

        /// <summary>
        /// If we determine at anytime there has been an interception, we use this method to determine who took possession
        /// </summary>
        private void InterceptionOccurred()
        {
            var possessionChangeResult = new InterceptionPossessionChangeSkillsCheckResult();
            possessionChangeResult.Execute(_game);

            var interceptionResult = new Interception(possessionChangeResult.Possession);
            interceptionResult.Execute(_game);
        }

        #endregion
    }
}