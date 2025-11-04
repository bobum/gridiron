using Structurizr;
using Structurizr.IO.PlantUML;
using System.IO;

// Workspace and model
var workspace = new Workspace("Comprehensive Game Simulation Architecture", "Full C4 diagrams for the game simulation application");
var model = workspace.Model;

// 1. System Landscape
var user = model.AddPerson("User", "Interacts with the game simulation.");
var gameSystem = model.AddSoftwareSystem("Game Simulation System", "Simulates sports games, manages state, and provides results.");
user.Uses(gameSystem, "Runs and observes games");

// External systems/services (example: stats, logging)
var statsService = model.AddSoftwareSystem("Stats Service", "Provides external statistics.");
var loggingService = model.AddSoftwareSystem("Logging Service", "Stores logs and play-by-play data.");
gameSystem.Uses(statsService, "Fetches stats");
gameSystem.Uses(loggingService, "Sends logs and play-by-play data");

// 2. Containers
var app = gameSystem.AddContainer("Game Simulation App", "Executes game logic, manages state machine, and handles game flow.", "C# .NET 8");
var webApi = gameSystem.AddContainer("Web API", "Exposes game data and results via HTTP.", "ASP.NET Core");
var testSuite = gameSystem.AddContainer("Unit Test Suite", "Runs automated tests for game logic.", "MSTest");

// Relationships
user.Uses(webApi, "Views game results");
webApi.Uses(app, "Queries game state");
testSuite.Uses(app, "Tests game logic");
app.Uses(statsService, "Fetches stats");
app.Uses(loggingService, "Sends logs");

// 3. Domain Components (inside App)
var gameFlow = app.AddComponent("GameFlow", "Controls the flow and state machine of the game.");
var game = app.AddComponent("Game", "Represents the state and data of a game.");
var iPlay = app.AddComponent("IPlay", "Interface for all play types in the game.");
var runPlay = app.AddComponent("RunPlay", "Represents a running play with run segments.");
var passPlay = app.AddComponent("PassPlay", "Represents a passing play with pass segments and laterals.");
var kickoffPlay = app.AddComponent("KickoffPlay", "Represents a kickoff play with return segments.");
var puntPlay = app.AddComponent("PuntPlay", "Represents a punt play with return segments.");
var fieldGoalPlay = app.AddComponent("FieldGoalPlay", "Represents a field goal or extra point attempt.");
var iPlaySegment = app.AddComponent("IPlaySegment", "Interface for play segments (ball carrier changes).");
var runSegment = app.AddComponent("RunSegment", "Segment of a run play.");
var passSegment = app.AddComponent("PassSegment", "Segment of a pass play (forward pass or lateral).");
var returnSegment = app.AddComponent("ReturnSegment", "Segment for kickoff/punt returns.");
var team = app.AddComponent("Team", "Represents a team.");
var coach = app.AddComponent("Coach", "Represents a coach.");
var replayLog = app.AddComponent("ReplayLog", "Captures and replays game events.");
var seedableRandom = app.AddComponent("SeedableRandom", "Random number generator for deterministic tests.");
var cryptoRandom = app.AddComponent("CryptoRandom", "Cryptographically secure random generator.");
var logger = app.AddComponent("ILogger<GameFlow>", "Logging interface for game events.");
var testGame = app.AddComponent("TestGame", "Test helper for game scenarios.");

// State machine components
var stateMachine = app.AddComponent("StateMachine<State, Trigger>", "Implements the game state machine.");
var prePlayAction = app.AddComponent("PrePlay", "Action performed before each play.");

// Relationships (domain)
gameFlow.Uses(game, "Manipulates and queries game state");
gameFlow.Uses(stateMachine, "Manages game states and transitions");
gameFlow.Uses(seedableRandom, "Generates random values for game events");
gameFlow.Uses(cryptoRandom, "Generates secure random values");
gameFlow.Uses(logger, "Logs game flow events");
gameFlow.Uses(replayLog, "Captures and replays events");
gameFlow.Uses(prePlayAction, "Executes pre-play actions");
game.Uses(iPlay, "Contains list of IPlay");
game.Uses(team, "Has teams");
game.Uses(coach, "Has coaches");
game.Uses(logger, "Logs game events");
game.Uses(replayLog, "Stores play-by-play data");
iPlay.Uses(iPlaySegment, "Contains segments for complex plays");
runPlay.Uses(runSegment, "Contains run segments for multiple fumbles");
passPlay.Uses(passSegment, "Contains pass segments for laterals");
kickoffPlay.Uses(returnSegment, "Contains return segments for fumbles");
puntPlay.Uses(returnSegment, "Contains return segments for fumbles");
fieldGoalPlay.Uses(returnSegment, "Contains return segments for blocked kicks");
prePlayAction.Uses(iPlay, "Creates concrete play types");
testSuite.Uses(testGame, "Runs test scenarios");
testGame.Uses(gameFlow, "Tests game flow");
testGame.Uses(seedableRandom, "Controls randomness in tests");

// 4. State Machine: States, Triggers, Events
var stateEnum = app.AddComponent("State", "Enumeration of game states (PreGame, CoinToss, PrePlay, etc.)");
var triggerEnum = app.AddComponent("Trigger", "Enumeration of triggers (Snap, CoinTossed, Fumble, PlayResult, etc.)");
stateMachine.Uses(stateEnum, "Uses states");
stateMachine.Uses(triggerEnum, "Uses triggers");

// 5. Detailed State Machine Flow Components
// States
var initializeGameState = app.AddComponent("State.InitializeGame", "Initial state before game flow starts");
var preGameState = app.AddComponent("State.PreGame", "Pregame festivities and warmups");
var coinTossState = app.AddComponent("State.CoinToss", "Coin toss to determine possession");
var prePlayState = app.AddComponent("State.PrePlay", "Huddle: determine play, check pre-snap penalties, snap ball");
var fieldGoalState = app.AddComponent("State.FieldGoal", "Field goal attempt execution");
var runPlayState = app.AddComponent("State.RunPlay", "Run play execution");
var kickoffState = app.AddComponent("State.Kickoff", "Kickoff play execution");
var puntState = app.AddComponent("State.Punt", "Punt play execution");
var passPlayState = app.AddComponent("State.PassPlay", "Pass play execution with interception check");
var fumbleReturnState = app.AddComponent("State.FumbleReturn", "Fumble check and recovery");
var fieldGoalResultState = app.AddComponent("State.FieldGoalResult", "Field goal result processing");
var runPlayResultState = app.AddComponent("State.RunPlayResult", "Run play result processing");
var kickoffResultState = app.AddComponent("State.KickoffResult", "Kickoff result processing");
var puntResultState = app.AddComponent("State.PuntResult", "Punt result processing");
var passPlayResultState = app.AddComponent("State.PassPlayResult", "Pass play result processing");
var postPlayState = app.AddComponent("State.PostPlay", "Post-play: check penalties, scores, quarter expiration");
var quarterExpiredState = app.AddComponent("State.QuarterExpired", "Quarter expired: teams change endzones");
var halftimeState = app.AddComponent("State.Halftime", "Halftime break");
var postGameState = app.AddComponent("State.PostGame", "Game over: final processing");

// Triggers
var startGameFlowTrigger = app.AddComponent("Trigger.StartGameFlow", "Initiates game flow from initialization");
var warmupsCompletedTrigger = app.AddComponent("Trigger.WarmupsCompleted", "Pregame warmups finished");
var coinTossedTrigger = app.AddComponent("Trigger.CoinTossed", "Coin toss completed");
var snapTrigger = app.AddComponent("Trigger.Snap", "Ball is snapped (conditional on play type)");
var fumbleTrigger = app.AddComponent("Trigger.Fumble", "Checks for fumble occurrence");
var playResultTrigger = app.AddComponent("Trigger.PlayResult", "Play completed, move to result processing");
var nextPlayTrigger = app.AddComponent("Trigger.NextPlay", "Move to next play (dynamic: checks quarter expiration)");
var quarterOverTrigger = app.AddComponent("Trigger.QuarterOver", "Quarter ends, continue to next quarter");
var halfExpiredTrigger = app.AddComponent("Trigger.HalfExpired", "First half ends");
var halftimeOverTrigger = app.AddComponent("Trigger.HalftimeOver", "Halftime ends");
var gameExpiredTrigger = app.AddComponent("Trigger.GameExpired", "Game ends");

// Actions (OnEntry methods)
var doPreGameAction = app.AddComponent("Action.DoPreGame", "Executes pregame festivities");
var doCoinTossAction = app.AddComponent("Action.DoCoinToss", "Executes coin toss logic");
var doPrePlayAction = app.AddComponent("Action.DoPrePlay", "Determines play, checks pre-snap penalties, executes snap");
var doFieldGoalPlayAction = app.AddComponent("Action.DoFieldGoalPlay", "Checks for block, executes field goal kick");
var doRunPlayAction = app.AddComponent("Action.DoRunPlay", "Executes run play");
var doKickoffPlayAction = app.AddComponent("Action.DoKickoffPlay", "Executes kickoff");
var doPuntPlayAction = app.AddComponent("Action.DoPuntPlay", "Checks for block, executes punt");
var doPassPlayAction = app.AddComponent("Action.DoPassPlay", "Checks for interception, executes pass");
var doFumbleCheckAction = app.AddComponent("Action.DoFumbleCheck", "Checks if fumble occurred, determines recovery");
var doFieldGoalResultAction = app.AddComponent("Action.DoFieldGoalResult", "Processes field goal result");
var doRunPlayResultAction = app.AddComponent("Action.DoRunPlayResult", "Processes run play result");
var doKickoffResultAction = app.AddComponent("Action.DoKickoffResult", "Processes kickoff result");
var doPuntResultAction = app.AddComponent("Action.DoPuntResult", "Processes punt result");
var doPassPlayResultAction = app.AddComponent("Action.DoPassPlayResult", "Processes pass play result");
var doPostPlayAction = app.AddComponent("Action.DoPostPlay", "Checks penalties, scores, injuries, quarter expiration");
var doQuarterExpireAction = app.AddComponent("Action.DoQuarterExpire", "Handles quarter transition logic");
var doHalftimeAction = app.AddComponent("Action.DoHalftime", "Executes halftime activities");
var doPostGameAction = app.AddComponent("Action.DoPostGame", "Finalizes game data");

// Skills checks and helper actions
var penaltyCheck = app.AddComponent("PenaltyCheck", "Checks for penalties (Before/During/After)");
var fumbleOccurred = app.AddComponent("FumbleOccurred", "Determines fumble recovery possession");
var interceptionOccurred = app.AddComponent("InterceptionOccurred", "Determines interception possession change");
var snapAction = app.AddComponent("Snap", "Executes snap skills check");

// State Machine Flow Relationships: State -> Trigger -> Next State
// InitializeGame -> StartGameFlow -> PreGame
gameFlow.Uses(initializeGameState, "Starts in");
initializeGameState.Uses(startGameFlowTrigger, "Fires");
startGameFlowTrigger.Uses(preGameState, "Transitions to");
preGameState.Uses(doPreGameAction, "OnEntry");

// PreGame -> WarmupsCompleted -> CoinToss
preGameState.Uses(warmupsCompletedTrigger, "Fires");
warmupsCompletedTrigger.Uses(coinTossState, "Transitions to");
coinTossState.Uses(doCoinTossAction, "OnEntry");

// CoinToss -> CoinTossed -> PrePlay
coinTossState.Uses(coinTossedTrigger, "Fires");
coinTossedTrigger.Uses(prePlayState, "Transitions to");
prePlayState.Uses(doPrePlayAction, "OnEntry");

// PrePlay can fire Snap (conditional on play type) or PlayResult (if pre-snap penalty)
doPrePlayAction.Uses(penaltyCheck, "Checks penalties before snap");
doPrePlayAction.Uses(snapAction, "Executes snap if no penalty");

// PrePlay -> Snap -> FieldGoal/RunPlay/Kickoff/Punt/PassPlay (conditional)
prePlayState.Uses(snapTrigger, "Fires (conditional on play type)");
snapTrigger.Uses(fieldGoalState, "If PlayType = FieldGoal");
snapTrigger.Uses(runPlayState, "If PlayType = Run");
snapTrigger.Uses(kickoffState, "If PlayType = Kickoff");
snapTrigger.Uses(puntState, "If PlayType = Punt");
snapTrigger.Uses(passPlayState, "If PlayType = Pass");

// PrePlay -> PlayResult -> PostPlay (if pre-snap penalty)
prePlayState.Uses(playResultTrigger, "Fires (if pre-snap penalty)");
playResultTrigger.Uses(postPlayState, "Transitions to (skip play execution)");

// FieldGoal -> OnEntry -> Fumble -> FumbleReturn
fieldGoalState.Uses(doFieldGoalPlayAction, "OnEntry");
doFieldGoalPlayAction.Uses(fumbleOccurred, "Checks for block and fumble");
fieldGoalState.Uses(fumbleTrigger, "Fires");
fumbleTrigger.Uses(fumbleReturnState, "Transitions to");

// RunPlay -> OnEntry -> Fumble -> FumbleReturn
runPlayState.Uses(doRunPlayAction, "OnEntry");
runPlayState.Uses(fumbleTrigger, "Fires");

// Kickoff -> OnEntry -> Fumble -> FumbleReturn
kickoffState.Uses(doKickoffPlayAction, "OnEntry");
kickoffState.Uses(fumbleTrigger, "Fires");

// Punt -> OnEntry -> Fumble -> FumbleReturn
puntState.Uses(doPuntPlayAction, "OnEntry");
doPuntPlayAction.Uses(fumbleOccurred, "Checks for block");
puntState.Uses(fumbleTrigger, "Fires");

// PassPlay -> OnEntry -> Fumble -> FumbleReturn
passPlayState.Uses(doPassPlayAction, "OnEntry");
doPassPlayAction.Uses(interceptionOccurred, "Checks for interception");
passPlayState.Uses(fumbleTrigger, "Fires");

// FumbleReturn -> OnEntry -> PlayResult -> (FieldGoalResult/RunPlayResult/KickoffResult/PuntResult/PassPlayResult)
fumbleReturnState.Uses(doFumbleCheckAction, "OnEntry");
doFumbleCheckAction.Uses(fumbleOccurred, "Checks fumble and recovery");
fumbleReturnState.Uses(playResultTrigger, "Fires (conditional on play type)");
playResultTrigger.Uses(fieldGoalResultState, "If PlayType = FieldGoal");
playResultTrigger.Uses(runPlayResultState, "If PlayType = Run");
playResultTrigger.Uses(kickoffResultState, "If PlayType = Kickoff");
playResultTrigger.Uses(puntResultState, "If PlayType = Punt");
playResultTrigger.Uses(passPlayResultState, "If PlayType = Pass");

// All Result states -> OnEntry -> PlayResult -> PostPlay
fieldGoalResultState.Uses(doFieldGoalResultAction, "OnEntry");
fieldGoalResultState.Uses(playResultTrigger, "Fires");

runPlayResultState.Uses(doRunPlayResultAction, "OnEntry");
runPlayResultState.Uses(playResultTrigger, "Fires");

kickoffResultState.Uses(doKickoffResultAction, "OnEntry");
kickoffResultState.Uses(playResultTrigger, "Fires");

puntResultState.Uses(doPuntResultAction, "OnEntry");
puntResultState.Uses(playResultTrigger, "Fires");

passPlayResultState.Uses(doPassPlayResultAction, "OnEntry");
passPlayResultState.Uses(playResultTrigger, "Fires");

// PostPlay -> OnEntry -> NextPlay (dynamic) -> PrePlay or QuarterExpired
postPlayState.Uses(doPostPlayAction, "OnEntry");
doPostPlayAction.Uses(penaltyCheck, "Checks during/after penalties");
postPlayState.Uses(nextPlayTrigger, "Fires (dynamic: checks quarter expiration)");
nextPlayTrigger.Uses(prePlayState, "If quarter NOT expired");
nextPlayTrigger.Uses(quarterExpiredState, "If quarter expired");

// QuarterExpired -> OnEntry -> QuarterOver/HalfExpired/GameExpired
quarterExpiredState.Uses(doQuarterExpireAction, "OnEntry");
quarterExpiredState.Uses(quarterOverTrigger, "Fires (if not end of half or game)");
quarterOverTrigger.Uses(prePlayState, "Transitions to next quarter");
quarterExpiredState.Uses(halfExpiredTrigger, "Fires (if Q2 ends)");
halfExpiredTrigger.Uses(halftimeState, "Transitions to");
quarterExpiredState.Uses(gameExpiredTrigger, "Fires (if Q4/OT ends)");
gameExpiredTrigger.Uses(postGameState, "Transitions to");

// Halftime -> OnEntry -> HalftimeOver -> PrePlay
halftimeState.Uses(doHalftimeAction, "OnEntry");
halftimeState.Uses(halftimeOverTrigger, "Fires");
halftimeOverTrigger.Uses(prePlayState, "Transitions to Q3");

// PostGame -> OnEntry (terminal state)
postGameState.Uses(doPostGameAction, "OnEntry");

// 6. Views
var landscapeView = workspace.Views.CreateSystemLandscapeView("Landscape", "System Landscape diagram");
landscapeView.AddAllElements();

var contextView = workspace.Views.CreateSystemContextView(gameSystem, "Context", "System Context diagram");
contextView.AddAllSoftwareSystems();
contextView.AddAllPeople();

var containerView = workspace.Views.CreateContainerView(gameSystem, "Containers", "Container diagram");
containerView.AddAllContainers();
containerView.AddAllPeople();

var componentView = workspace.Views.CreateComponentView(app, "Components", "Component diagram for Game Simulation App");
componentView.AddAllComponents();
componentView.AddAllPeople();

// 7. State Machine Flow View - Comprehensive
var stateMachineView = workspace.Views.CreateComponentView(app, "StateMachineFlow", "Comprehensive State Machine Flow");

// Add all state machine components
stateMachineView.Add(gameFlow);
stateMachineView.Add(stateMachine);

// States
stateMachineView.Add(initializeGameState);
stateMachineView.Add(preGameState);
stateMachineView.Add(coinTossState);
stateMachineView.Add(prePlayState);
stateMachineView.Add(fieldGoalState);
stateMachineView.Add(runPlayState);
stateMachineView.Add(kickoffState);
stateMachineView.Add(puntState);
stateMachineView.Add(passPlayState);
stateMachineView.Add(fumbleReturnState);
stateMachineView.Add(fieldGoalResultState);
stateMachineView.Add(runPlayResultState);
stateMachineView.Add(kickoffResultState);
stateMachineView.Add(puntResultState);
stateMachineView.Add(passPlayResultState);
stateMachineView.Add(postPlayState);
stateMachineView.Add(quarterExpiredState);
stateMachineView.Add(halftimeState);
stateMachineView.Add(postGameState);

// Triggers
stateMachineView.Add(startGameFlowTrigger);
stateMachineView.Add(warmupsCompletedTrigger);
stateMachineView.Add(coinTossedTrigger);
stateMachineView.Add(snapTrigger);
stateMachineView.Add(fumbleTrigger);
stateMachineView.Add(playResultTrigger);
stateMachineView.Add(nextPlayTrigger);
stateMachineView.Add(quarterOverTrigger);
stateMachineView.Add(halfExpiredTrigger);
stateMachineView.Add(halftimeOverTrigger);
stateMachineView.Add(gameExpiredTrigger);

// Actions
stateMachineView.Add(doPreGameAction);
stateMachineView.Add(doCoinTossAction);
stateMachineView.Add(doPrePlayAction);
stateMachineView.Add(doFieldGoalPlayAction);
stateMachineView.Add(doRunPlayAction);
stateMachineView.Add(doKickoffPlayAction);
stateMachineView.Add(doPuntPlayAction);
stateMachineView.Add(doPassPlayAction);
stateMachineView.Add(doFumbleCheckAction);
stateMachineView.Add(doFieldGoalResultAction);
stateMachineView.Add(doRunPlayResultAction);
stateMachineView.Add(doKickoffResultAction);
stateMachineView.Add(doPuntResultAction);
stateMachineView.Add(doPassPlayResultAction);
stateMachineView.Add(doPostPlayAction);
stateMachineView.Add(doQuarterExpireAction);
stateMachineView.Add(doHalftimeAction);
stateMachineView.Add(doPostGameAction);

// Helpers
stateMachineView.Add(penaltyCheck);
stateMachineView.Add(fumbleOccurred);
stateMachineView.Add(interceptionOccurred);
stateMachineView.Add(snapAction);

// 8. Export all views to PlantUML
var plantUmlWriter = new PlantUMLWriter();

void WriteView(View view, string filename)
{
    using (var writer = new StreamWriter(filename))
    {
        switch (view)
        {
            case SystemLandscapeView v: plantUmlWriter.Write(v, writer); break;
            case SystemContextView v: plantUmlWriter.Write(v, writer); break;
            case ContainerView v: plantUmlWriter.Write(v, writer); break;
            case ComponentView v: plantUmlWriter.Write(v, writer); break;
        }
    }
}

WriteView(landscapeView, "SystemLandscape.puml");
WriteView(contextView, "SystemContext.puml");
WriteView(containerView, "Container.puml");
WriteView(componentView, "Component.puml");
WriteView(stateMachineView, "StateMachineFlow.puml");

Console.WriteLine("PlantUML files generated:");
Console.WriteLine("  SystemLandscape.puml");
Console.WriteLine("  SystemContext.puml");
Console.WriteLine("  Container.puml");
Console.WriteLine("  Component.puml");
Console.WriteLine("  StateMachineFlow.puml");
Console.WriteLine("Run 'plantuml <filename>.puml' to generate PNG diagrams.");
