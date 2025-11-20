using Structurizr;
using Structurizr.IO.PlantUML;
using System.IO;
using PlantUml.Net;

// ================================================================================
// GRIDIRON FOOTBALL SIMULATION - C4 ARCHITECTURE DIAGRAMS
// ================================================================================
// Generates comprehensive PlantUML diagrams for the complete system
// Last Updated: November 2025
// Test Coverage: 839/839 tests passing (100%)
// ================================================================================

var workspace = new Workspace("Gridiron Simulation System", 
    "Comprehensive NFL game simulation with state machine, injury system, penalties, and database persistence");
var model = workspace.Model;

// ================================================================================
// LEVEL 1: SYSTEM CONTEXT
// ================================================================================

var user = model.AddPerson("User", "Game administrator or analyst who runs football simulations");
var developer = model.AddPerson("Developer", "Software developer maintaining and extending the simulation");

var gridironSystem = model.AddSoftwareSystem("Gridiron Simulation System", 
    "Simulates NFL football games with realistic play-by-play, injuries, penalties, and statistical outcomes. " +
    "Supports database persistence and deterministic replay.");

// External systems
var azureSql = model.AddSoftwareSystem("Azure SQL Database", 
    "Stores teams, players, games, and play-by-play data");
var loggingSystem = model.AddSoftwareSystem("Logging System", 
  "Microsoft.Extensions.Logging for play-by-play and debug logs");

// Relationships
user.Uses(gridironSystem, "Runs simulations, analyzes results");
developer.Uses(gridironSystem, "Develops, tests, maintains");
gridironSystem.Uses(azureSql, "Persists and retrieves game data", "EF Core 8.0");
gridironSystem.Uses(loggingSystem, "Writes play-by-play logs and debug info");

// ================================================================================
// LEVEL 2: CONTAINERS
// ================================================================================

var domainObjects = gridironSystem.AddContainer("DomainObjects", 
    "Pure domain models with no infrastructure dependencies. " +
    "Contains Players (25+ attributes), Teams, Games, Plays, Penalties, Injuries.",
    "C# .NET 8 Class Library");

var stateLibrary = gridironSystem.AddContainer("StateLibrary", 
    "Game simulation engine with 19-state machine, 5 play types, 25+ skill checks, " +
 "penalty enforcement, and injury system.",
    "C# .NET 8 Class Library");

var dataAccessLayer = gridironSystem.AddContainer("DataAccessLayer", 
    "Entity Framework Core persistence layer with migrations, seed data, and team loaders.",
    "C# .NET 8 Class Library");

var console = gridironSystem.AddContainer("GridironConsole", 
    "Console application for running simulations and testing game scenarios.",
    "C# .NET 8 Console App");

var tests = gridironSystem.AddContainer("UnitTestProject1", 
    "Comprehensive test suite with 839 tests (100% passing). " +
    "Includes play execution, penalties, injuries, scoring, and integration tests.",
    "MSTest");

var diagramGen = gridironSystem.AddContainer("Diagram", 
    "Generates C4 architecture diagrams using Structurizr and PlantUML.",
    "C# .NET 8 Console App");

// Container relationships
user.Uses(console, "Executes simulations");
developer.Uses(tests, "Runs tests, validates changes");
developer.Uses(diagramGen, "Generates architecture documentation");

console.Uses(stateLibrary, "Orchestrates game flow");
console.Uses(domainObjects, "Creates and manages game entities");
console.Uses(dataAccessLayer, "Loads teams from database");

stateLibrary.Uses(domainObjects, "Manipulates game state");
stateLibrary.Uses(loggingSystem, "Logs play-by-play");

dataAccessLayer.Uses(domainObjects, "Maps entities to database");
dataAccessLayer.Uses(azureSql, "Executes queries", "Entity Framework Core");

tests.Uses(stateLibrary, "Tests game logic");
tests.Uses(domainObjects, "Creates test scenarios");
tests.Uses(dataAccessLayer, "Tests persistence");

// ================================================================================
// LEVEL 3: COMPONENTS - Domain Objects
// ================================================================================

// Core entities
var game = domainObjects.AddComponent("Game", 
    "Central game state: field position, down, distance, score, possession. Contains list of all plays.",
    "C# Class");
var player = domainObjects.AddComponent("Player", 
    "Player with 25+ attributes: Speed, Strength, Passing, Catching, Tackling, Discipline, Fragility, Health.",
    "C# Class");
var team = domainObjects.AddComponent("Team", 
    "Team with roster, depth charts, coaches, stats. Supports offense, defense, and special teams formations.",
    "C# Class");

// Play types
var iPlay = domainObjects.AddComponent("IPlay", 
    "Interface for all play types. Contains penalties, fumbles, injuries, yards gained.",
    "Interface");
var passPlay = domainObjects.AddComponent("PassPlay", 
    "Pass play with passer, receiver, air yards, YAC, interceptions, laterals.",
    "C# Class");
var runPlay = domainObjects.AddComponent("RunPlay", 
    "Run play with ball carrier, direction, segments for multiple fumbles.",
    "C# Class");
var kickoffPlay = domainObjects.AddComponent("KickoffPlay", 
    "Kickoff with distance, touchback, return yards, onside kick logic.",
    "C# Class");
var puntPlay = domainObjects.AddComponent("PuntPlay", 
    "Punt with hang time, distance, fair catch, blocks, returns.",
    "C# Class");
var fieldGoalPlay = domainObjects.AddComponent("FieldGoalPlay", 
 "Field goal/XP attempt with distance, wind, block returns.",
    "C# Class");

// Segments
var iPlaySegment = domainObjects.AddComponent("IPlaySegment", 
    "Interface for play segments (ball carrier changes during fumbles/laterals).",
    "Interface");
var passSegment = domainObjects.AddComponent("PassSegment", 
    "Forward pass or lateral with passer, receiver, air yards, YAC.",
    "C# Class");
var runSegment = domainObjects.AddComponent("RunSegment", 
    "Run segment with ball carrier, yards, fumble info.",
    "C# Class");
var returnSegment = domainObjects.AddComponent("ReturnSegment", 
    "Return segment for kickoffs, punts, interceptions, blocked kicks.",
    "C# Class");

// Penalties and Injuries
var penalty = domainObjects.AddComponent("Penalty", 
    "50+ NFL penalties with real odds, yardage, acceptance logic, discipline impact.",
    "C# Class");
var injury = domainObjects.AddComponent("Injury", 
    "Injury with type (Ankle/Knee/Shoulder/Concussion/Hamstring), severity (Minor/Moderate/GameEnding), recovery time.",
    "C# Class");
var fumble = domainObjects.AddComponent("Fumble", 
    "Fumble with recovery, yards, out-of-bounds tracking.",
    "C# Class");

// Helpers
var teams = domainObjects.AddComponent("Teams", 
  "Helper for loading teams from JSON or database. Contains home/away teams.",
    "C# Class");
var seedableRandom = domainObjects.AddComponent("SeedableRandom", 
    "Deterministic random number generator for reproducible simulations.",
  "C# Class");
var replayLog = domainObjects.AddComponent("ReplayLog", 
    "Captures play-by-play events for replay and analysis.",
    "C# Class");
var fieldPositionHelper = domainObjects.AddComponent("FieldPositionHelper", 
    "Converts internal 0-100 field position to NFL notation (e.g., 'Kansas City 20').",
    "C# Class");

// Time management
var quarter = domainObjects.AddComponent("Quarter", 
    "Represents a quarter with clock management and expiration detection.",
    "C# Class");
var half = domainObjects.AddComponent("Half", 
"Represents a half (1st/2nd) containing two quarters.",
    "C# Class");

// Persistence entities
var playByPlay = domainObjects.AddComponent("PlayByPlay", 
    "Entity for storing game logs as JSON in database.",
    "C# Class");

// Domain relationships
game.Uses(team, "Has home and away teams");
game.Uses(iPlay, "Contains list of plays");
game.Uses(quarter, "Tracks current quarter");
game.Uses(half, "Tracks current half");
game.Uses(replayLog, "Logs play-by-play events");

team.Uses(player, "Has roster of players");
team.Uses(teams, "Created by Teams helper");

iPlay.Uses(penalty, "Can have penalties");
iPlay.Uses(fumble, "Can have fumbles");
iPlay.Uses(injury, "Can have injuries");
iPlay.Uses(iPlaySegment, "Contains segments");

passPlay.Uses(passSegment, "Contains pass segments");
passPlay.Uses(player, "Has passer and receivers");
runPlay.Uses(runSegment, "Contains run segments");
kickoffPlay.Uses(returnSegment, "Contains return segments");
puntPlay.Uses(returnSegment, "Contains return segments");
fieldGoalPlay.Uses(returnSegment, "Contains return segments if blocked");

injury.Uses(player, "Tracks injured player");
penalty.Uses(player, "Committed by player");
fumble.Uses(player, "Fumbled by, recovered by players");

// ================================================================================
// LEVEL 3: COMPONENTS - State Library (Game Engine)
// ================================================================================

// State machine
var gameFlow = stateLibrary.AddComponent("GameFlow", 
    "19-state machine orchestrator. Controls game progression, triggers state transitions, injects dependencies.",
    "C# Class");
var stateMachine = stateLibrary.AddComponent("StateMachine<State, Trigger>", 
    "Stateless library state machine with 19 states and 11 triggers.",
    "Stateless Library");

// States
var stateEnum = stateLibrary.AddComponent("State (enum)", 
    "19 states: InitializeGame, PreGame, CoinToss, PrePlay, RunPlay, PassPlay, Kickoff, Punt, FieldGoal, " +
    "FumbleReturn, *Result states (5), PostPlay, QuarterExpired, Halftime, PostGame",
    "Enum");
var triggerEnum = stateLibrary.AddComponent("Trigger (enum)", 
    "11 triggers: StartGameFlow, WarmupsCompleted, CoinTossed, Snap, Fumble, PlayResult, " +
    "NextPlay, QuarterOver, HalfExpired, HalftimeOver, GameExpired",
    "Enum");

// Actions (OnEntry methods)
var preGameAction = stateLibrary.AddComponent("PreGame (Action)", 
    "Pregame festivities and setup.",
"C# Class");
var coinTossAction = stateLibrary.AddComponent("CoinToss (Action)", 
    "Executes coin toss, determines possession with deferral logic.",
    "C# Class");
var prePlayAction = stateLibrary.AddComponent("PrePlay (Action)", 
    "Determines play type, checks pre-snap penalties, executes snap.",
    "C# Class");
var snapAction = stateLibrary.AddComponent("Snap (Action)", 
    "Snap skills check (bad snap possibility).",
    "C# Class");
var postPlayAction = stateLibrary.AddComponent("PostPlay (Action)", 
    "Checks during/after penalties, scores, injuries, quarter expiration.",
    "C# Class");

// Play execution
var runExecution = stateLibrary.AddComponent("Run (Play)", 
    "Executes run play: blocking, direction, yards, breakaway, fumbles, injuries.",
    "C# Class");
var passExecution = stateLibrary.AddComponent("Pass (Play)", 
    "Executes pass play: protection, completion, interception, YAC, sacks, injuries.",
    "C# Class");
var kickoffExecution = stateLibrary.AddComponent("Kickoff (Play)", 
    "Executes kickoff: distance, touchback, return, onside kick, injuries.",
    "C# Class");
var puntExecution = stateLibrary.AddComponent("Punt (Play)", 
    "Executes punt: distance, hang time, block, fair catch, return, injuries.",
    "C# Class");
var fieldGoalExecution = stateLibrary.AddComponent("FieldGoal (Play)", 
    "Executes field goal: distance calculation, block check, make/miss.",
    "C# Class");

// Play results
var runResult = stateLibrary.AddComponent("RunResult", 
    "Processes run play: updates field position, checks first down, TD, safety.",
    "C# Class");
var passResult = stateLibrary.AddComponent("PassResult", 
    "Processes pass play: updates field position, checks first down, TD, safety, interceptions.",
    "C# Class");
var kickoffResult = stateLibrary.AddComponent("KickoffResult", 
    "Processes kickoff: updates possession, field position.",
    "C# Class");
var puntResult = stateLibrary.AddComponent("PuntResult", 
    "Processes punt: updates possession, field position, checks safety.",
    "C# Class");
var fieldGoalResult = stateLibrary.AddComponent("FieldGoalResult", 
    "Processes field goal: updates score, possession.",
    "C# Class");

// Skills checks (25+)
var passCompletionCheck = stateLibrary.AddComponent("PassCompletionSkillsCheck", 
    "Determines if pass is completed based on QB passing, receiver catching, coverage.",
    "C# Class");
var passProtectionCheck = stateLibrary.AddComponent("PassProtectionSkillsCheck", 
    "Determines if QB is sacked based on O-line blocking vs D-line pressure.",
    "C# Class");
var interceptionCheck = stateLibrary.AddComponent("InterceptionOccurredSkillsCheck", 
    "Determines if pass is intercepted (only checked on incomplete passes).",
    "C# Class");
var blockingSuccessCheck = stateLibrary.AddComponent("BlockingSuccessSkillsCheck", 
    "Determines run blocking effectiveness (O-line vs D-line).",
    "C# Class");
var tackleBreakCheck = stateLibrary.AddComponent("TackleBreakSkillsCheck", 
    "Determines if ball carrier breaks tackle for extra yards.",
    "C# Class");
var bigRunCheck = stateLibrary.AddComponent("BigRunSkillsCheck", 
    "Determines if run becomes breakaway (15-44 extra yards).",
    "C# Class");
var fumbleOccurredCheck = stateLibrary.AddComponent("FumbleOccurredSkillsCheck", 
    "Determines if fumble occurs based on ball carrier, tacklers, gang tackles.",
    "C# Class");
var fumbleRecoveryCheck = stateLibrary.AddComponent("FumbleRecoverySkillsCheckResult", 
    "Determines who recovers fumble (possession change logic).",
    "C# Class");
var yardsAfterCatchCheck = stateLibrary.AddComponent("YardsAfterCatchSkillsCheck", 
    "Calculates yards after catch based on receiver speed, agility, defense.",
    "C# Class");
var fieldGoalMakeCheck = stateLibrary.AddComponent("FieldGoalMakeOccurredSkillsCheck", 
    "Determines if field goal is made based on distance, kicker skill.",
    "C# Class");
var fieldGoalBlockCheck = stateLibrary.AddComponent("FieldGoalBlockOccurredSkillsCheck", 
    "Determines if field goal is blocked.",
    "C# Class");
var puntDistanceCheck = stateLibrary.AddComponent("PuntDistanceSkillsCheckResult", 
    "Calculates punt distance based on punter skill.",
    "C# Class");
var puntBlockCheck = stateLibrary.AddComponent("PuntBlockOccurredSkillsCheck", 
    "Determines if punt is blocked.",
    "C# Class");
var fairCatchCheck = stateLibrary.AddComponent("FairCatchOccurredSkillsCheck", 
    "Determines if returner calls fair catch.",
    "C# Class");
var muffedCatchCheck = stateLibrary.AddComponent("MuffedCatchOccurredSkillsCheck", 
    "Determines if returner muffs catch (fumble).",
    "C# Class");

// Penalty system
var preSnapPenaltyCheck = stateLibrary.AddComponent("PreSnapPenaltyOccurredSkillsCheck", 
    "Checks for pre-snap penalties (False Start, Offsides, Delay of Game, etc.).",
    "C# Class");
var blockingPenaltyCheck = stateLibrary.AddComponent("BlockingPenaltyOccurredSkillsCheck", 
    "Checks for blocking penalties (Holding, Illegal Block, Clipping, etc.).",
    "C# Class");
var coveragePenaltyCheck = stateLibrary.AddComponent("CoveragePenaltyOccurredSkillsCheck", 
    "Checks for coverage penalties (Pass Interference, Illegal Contact, Holding, etc.).",
    "C# Class");
var tacklePenaltyCheck = stateLibrary.AddComponent("TacklePenaltyOccurredSkillsCheck", 
    "Checks for tackle penalties (Facemask, Horse Collar, Unnecessary Roughness, etc.).",
    "C# Class");
var postPlayPenaltyCheck = stateLibrary.AddComponent("PostPlayPenaltyOccurredSkillsCheck", 
    "Checks for post-play penalties (Taunting, Unsportsmanlike Conduct, etc.).",
    "C# Class");
var penaltyEffect = stateLibrary.AddComponent("PenaltyEffectSkillsCheckResult", 
    "Determines specific penalty, player who committed it, yardage, acceptance.",
    "C# Class");
var penaltyEnforcement = stateLibrary.AddComponent("PenaltyEnforcement (Service)", 
    "Enforces penalties: calculates net yards, determines acceptance, handles offsetting, updates down/distance.",
    "C# Class");

// Injury system
var injuryOccurredCheck = stateLibrary.AddComponent("InjuryOccurredSkillsCheck", 
    "Determines if injury occurs based on fragility, play type, gang tackles, big plays, position.",
    "C# Class");
var injuryEffect = stateLibrary.AddComponent("InjuryEffectSkillsCheckResult", 
    "Determines injury type (Ankle/Knee/Shoulder/Concussion/Hamstring), severity (Minor/Moderate/GameEnding), recovery time.",
    "C# Class");

// Configuration
var gameProbabilities = stateLibrary.AddComponent("GameProbabilities", 
    "Constants for game probabilities (completion %, fumble rates, etc.).",
    "C# Class");
var injuryProbabilities = stateLibrary.AddComponent("InjuryProbabilities", 
    "Constants for injury probabilities by play type, position, multipliers.",
    "C# Class");

// Services
var statsAccumulator = stateLibrary.AddComponent("StatsAccumulator (Service)", 
    "Accumulates player stats (passing, rushing, receiving, tackles, etc.).",
    "C# Class");

// Calculators
var lineBattleCalc = stateLibrary.AddComponent("LineBattleCalculator", 
    "Calculates O-line vs D-line effectiveness for blocking/protection.",
    "C# Class");
var teamPowerCalc = stateLibrary.AddComponent("TeamPowerCalculator", 
    "Calculates overall team power for various situations.",
    "C# Class");

// State machine relationships
gameFlow.Uses(stateMachine, "Configures and fires triggers");
gameFlow.Uses(game, "Manipulates game state");
gameFlow.Uses(seedableRandom, "Injects RNG for deterministic testing");
stateMachine.Uses(stateEnum, "Uses states");
stateMachine.Uses(triggerEnum, "Uses triggers");

// State transitions
preGameAction.Uses(coinTossAction, "Transitions to");
coinTossAction.Uses(prePlayAction, "Transitions to");
prePlayAction.Uses(snapAction, "Checks for bad snap");
prePlayAction.Uses(preSnapPenaltyCheck, "Checks pre-snap penalties");
snapAction.Uses(runExecution, "Snap → Run (conditional)");
snapAction.Uses(passExecution, "Snap → Pass (conditional)");
snapAction.Uses(kickoffExecution, "Snap → Kickoff (conditional)");
snapAction.Uses(puntExecution, "Snap → Punt (conditional)");
snapAction.Uses(fieldGoalExecution, "Snap → Field Goal (conditional)");

// Play execution uses skills checks
runExecution.Uses(blockingSuccessCheck, "Checks blocking");
runExecution.Uses(tackleBreakCheck, "Checks tackle breaks");
runExecution.Uses(bigRunCheck, "Checks breakaway");
runExecution.Uses(fumbleOccurredCheck, "Checks fumbles");
runExecution.Uses(blockingPenaltyCheck, "Checks holding");
runExecution.Uses(tacklePenaltyCheck, "Checks facemask");
runExecution.Uses(injuryOccurredCheck, "Checks injuries");
runExecution.Uses(injuryEffect, "Determines injury details");

passExecution.Uses(passProtectionCheck, "Checks sacks");
passExecution.Uses(passCompletionCheck, "Checks completion");
passExecution.Uses(interceptionCheck, "Checks interceptions");
passExecution.Uses(yardsAfterCatchCheck, "Calculates YAC");
passExecution.Uses(fumbleOccurredCheck, "Checks fumbles");
passExecution.Uses(coveragePenaltyCheck, "Checks PI");
passExecution.Uses(tacklePenaltyCheck, "Checks facemask");
passExecution.Uses(injuryOccurredCheck, "Checks injuries");

kickoffExecution.Uses(fairCatchCheck, "Checks fair catch");
kickoffExecution.Uses(fumbleOccurredCheck, "Checks fumbles");
kickoffExecution.Uses(injuryOccurredCheck, "Checks injuries");

puntExecution.Uses(puntDistanceCheck, "Calculates distance");
puntExecution.Uses(puntBlockCheck, "Checks blocks");
puntExecution.Uses(fairCatchCheck, "Checks fair catch");
puntExecution.Uses(muffedCatchCheck, "Checks muffs");
puntExecution.Uses(fumbleOccurredCheck, "Checks fumbles");
puntExecution.Uses(injuryOccurredCheck, "Checks injuries");

fieldGoalExecution.Uses(fieldGoalMakeCheck, "Checks make/miss");
fieldGoalExecution.Uses(fieldGoalBlockCheck, "Checks blocks");


// Results use penalty enforcement
runResult.Uses(penaltyEnforcement, "Enforces penalties");
runResult.Uses(statsAccumulator, "Accumulates stats");
passResult.Uses(penaltyEnforcement, "Enforces penalties");
passResult.Uses(statsAccumulator, "Accumulates stats");
puntResult.Uses(penaltyEnforcement, "Enforces penalties");

postPlayAction.Uses(postPlayPenaltyCheck, "Checks post-play penalties");
postPlayAction.Uses(penaltyEnforcement, "Enforces penalties");

// Injury system details
injuryOccurredCheck.Uses(player, "Checks fragility");
injuryOccurredCheck.Uses(injuryProbabilities, "Uses probability tables");
injuryEffect.Uses(injury, "Creates injury");
injuryEffect.Uses(player, "Determines position-specific type");

// ================================================================================
// LEVEL 3: COMPONENTS - Data Access Layer
// ================================================================================

var dbContext = dataAccessLayer.AddComponent("GridironDbContext", 
    "Entity Framework Core DbContext with entity configurations, relationships, and ignored properties.",
    "C# Class");
var dbContextFactory = dataAccessLayer.AddComponent("GridironDbContextFactory", 
    "Design-time factory for EF Core migrations and tools.",
  "C# Class");
var teamsLoader = dataAccessLayer.AddComponent("TeamsLoader", 
    "Loads teams and players from database using LINQ queries.",
    "C# Class");
var teamSeeder = dataAccessLayer.AddComponent("TeamSeeder", 
    "Seeds initial team data (Falcons, Eagles) into database.",
    "C# Class");
var migrations = dataAccessLayer.AddComponent("Migrations", 
    "EF Core migrations for database schema management.",
    "C# Classes");

// Database relationships
dbContext.Uses(game, "Maps Game entity");
dbContext.Uses(team, "Maps Team entity");
dbContext.Uses(player, "Maps Player entity");
dbContext.Uses(playByPlay, "Maps PlayByPlay entity");
dbContext.Uses(azureSql, "Executes SQL queries");

dbContextFactory.Uses(dbContext, "Creates instances");
teamsLoader.Uses(dbContext, "Queries teams and players");
teamsLoader.Uses(teams, "Returns Teams helper");
teamSeeder.Uses(dbContext, "Inserts seed data");
migrations.Uses(dbContext, "Updates schema");

// ================================================================================
// LEVEL 3: COMPONENTS - Tests
// ================================================================================

var playExecutionTests = tests.AddComponent("PlayExecutionTests", 
    "Tests for Run, Pass, Kickoff, Punt, Field Goal execution (100+ tests).",
  "Test Classes");
var penaltyTests = tests.AddComponent("PenaltyTests", 
    "Tests for penalty enforcement, acceptance, offsetting, discipline (50+ tests).",
    "Test Classes");
var injuryTests = tests.AddComponent("InjurySystemTests", 
    "Tests for injury occurrence, severity, position-specific types, recovery (30+ tests).",
    "Test Classes");
var scoringTests = tests.AddComponent("ScoringIntegrationTests", 
    "Tests for touchdowns, field goals, safeties, 2-pt conversions (40+ tests).",
    "Test Classes");
var downProgressionTests = tests.AddComponent("DownProgressionTests", 
    "Tests for first downs, turnovers on downs, penalties affecting downs (50+ tests).",
    "Test Classes");
var integrationTests = tests.AddComponent("IntegrationTests", 
    "Tests for full game flow, red zone, goal line, third downs (100+ tests).",
  "Test Classes");
var scenarioBuilders = tests.AddComponent("ScenarioBuilders", 
    "Fluent builders for creating test scenarios with precise RNG control.",
    "Test Helpers");
var testFluentRandom = tests.AddComponent("TestFluentSeedableRandom", 
    "Fluent interface for configuring deterministic random sequences in tests.",
    "Test Helper");
var testGame = tests.AddComponent("TestGame", 
    "Helper for creating pre-configured game instances for testing.",
    "Test Helper");
var testTeams = tests.AddComponent("TestTeams", 
    "Helper for creating test teams with realistic player attributes.",
    "Test Helper");

// Test relationships
playExecutionTests.Uses(stateLibrary, "Tests play execution");
playExecutionTests.Uses(scenarioBuilders, "Creates test scenarios");
playExecutionTests.Uses(testFluentRandom, "Controls randomness");
penaltyTests.Uses(penaltyEnforcement, "Tests enforcement logic");
injuryTests.Uses(injuryOccurredCheck, "Tests injury occurrence");
injuryTests.Uses(injuryEffect, "Tests injury details");
scoringTests.Uses(gameFlow, "Tests full game scenarios");
integrationTests.Uses(gameFlow, "Tests end-to-end flows");
scenarioBuilders.Uses(testGame, "Creates test games");
scenarioBuilders.Uses(testTeams, "Creates test teams");
scenarioBuilders.Uses(testFluentRandom, "Configures RNG");

// ================================================================================
// VIEWS
// ================================================================================

var views = workspace.Views;

// System Context View
var contextView = views.CreateSystemContextView(gridironSystem, "SystemContext", 
    "System context showing external users and systems");
contextView.AddAllSoftwareSystems();
contextView.AddAllPeople();
contextView.PaperSize = PaperSize.A4_Landscape;

// Container View
var containerView = views.CreateContainerView(gridironSystem, "Containers", 
    "Container view showing the 6 major components of the system");
containerView.AddAllContainers();
containerView.Add(user);
containerView.Add(developer);
containerView.Add(azureSql);
containerView.Add(loggingSystem);
containerView.PaperSize = PaperSize.A4_Landscape;

// Component View - Domain Objects
var domainComponentView = views.CreateComponentView(domainObjects, "DomainComponents", 
    "Domain objects: entities, value objects, and helpers");
domainComponentView.AddAllComponents();
domainComponentView.Add(stateLibrary);
domainComponentView.Add(dataAccessLayer);
domainComponentView.PaperSize = PaperSize.A3_Landscape;

// Component View - State Library
var stateLibraryComponentView = views.CreateComponentView(stateLibrary, "StateLibraryComponents", 
    "State machine, play execution, skills checks, penalties, and injury system");
stateLibraryComponentView.AddAllComponents();
stateLibraryComponentView.Add(domainObjects);
stateLibraryComponentView.Add(loggingSystem);
stateLibraryComponentView.PaperSize = PaperSize.A3_Landscape;

// Component View - Data Access Layer
var dataAccessComponentView = views.CreateComponentView(dataAccessLayer, "DataAccessComponents", 
    "Entity Framework Core persistence layer");
dataAccessComponentView.AddAllComponents();
dataAccessComponentView.Add(domainObjects);
dataAccessComponentView.Add(azureSql);
dataAccessComponentView.PaperSize = PaperSize.A4_Landscape;

// Component View - Tests
var testsComponentView = views.CreateComponentView(tests, "TestComponents", 
    "839 tests with scenario builders and deterministic testing");
testsComponentView.AddAllComponents();
testsComponentView.Add(stateLibrary);
testsComponentView.Add(domainObjects);
testsComponentView.Add(dataAccessLayer);
testsComponentView.PaperSize = PaperSize.A3_Landscape;

// State Machine Flow View (Focused)
var stateMachineView = views.CreateComponentView(stateLibrary, "StateMachineFlow", 
  "Detailed state machine flow showing 19 states and transitions");
stateMachineView.Add(gameFlow);
stateMachineView.Add(stateMachine);
stateMachineView.Add(stateEnum);
stateMachineView.Add(triggerEnum);
stateMachineView.Add(preGameAction);
stateMachineView.Add(coinTossAction);
stateMachineView.Add(prePlayAction);
stateMachineView.Add(snapAction);
stateMachineView.Add(runExecution);
stateMachineView.Add(passExecution);
stateMachineView.Add(kickoffExecution);
stateMachineView.Add(puntExecution);
stateMachineView.Add(fieldGoalExecution);
stateMachineView.Add(runResult);
stateMachineView.Add(passResult);
stateMachineView.Add(kickoffResult);
stateMachineView.Add(puntResult);
stateMachineView.Add(fieldGoalResult);
stateMachineView.Add(postPlayAction);
stateMachineView.PaperSize = PaperSize.A3_Landscape;

// Skills Checks View (Focused)
var skillsChecksView = views.CreateComponentView(stateLibrary, "SkillsChecks", 
 "25+ skills checks for realistic gameplay");
skillsChecksView.Add(passCompletionCheck);
skillsChecksView.Add(passProtectionCheck);
skillsChecksView.Add(interceptionCheck);
skillsChecksView.Add(blockingSuccessCheck);
skillsChecksView.Add(tackleBreakCheck);
skillsChecksView.Add(bigRunCheck);
skillsChecksView.Add(fumbleOccurredCheck);
skillsChecksView.Add(fumbleRecoveryCheck);
skillsChecksView.Add(yardsAfterCatchCheck);
skillsChecksView.Add(fieldGoalMakeCheck);
skillsChecksView.Add(fieldGoalBlockCheck);
skillsChecksView.Add(puntDistanceCheck);
skillsChecksView.Add(puntBlockCheck);
skillsChecksView.Add(fairCatchCheck);
skillsChecksView.Add(muffedCatchCheck);
skillsChecksView.Add(runExecution);
skillsChecksView.Add(passExecution);
skillsChecksView.Add(kickoffExecution);
skillsChecksView.Add(puntExecution);
skillsChecksView.Add(fieldGoalExecution);
skillsChecksView.PaperSize = PaperSize.A3_Landscape;

// Penalty System View (Focused)
var penaltySystemView = views.CreateComponentView(stateLibrary, "PenaltySystem", 
    "50+ NFL penalties with enforcement logic");
penaltySystemView.Add(preSnapPenaltyCheck);
penaltySystemView.Add(blockingPenaltyCheck);
penaltySystemView.Add(coveragePenaltyCheck);
penaltySystemView.Add(tacklePenaltyCheck);
penaltySystemView.Add(postPlayPenaltyCheck);
penaltySystemView.Add(penaltyEffect);
penaltySystemView.Add(penaltyEnforcement);
penaltySystemView.Add(penalty);
penaltySystemView.Add(player);
penaltySystemView.Add(game);
penaltySystemView.Add(prePlayAction);
penaltySystemView.Add(runExecution);
penaltySystemView.Add(passExecution);
penaltySystemView.Add(postPlayAction);
penaltySystemView.Add(runResult);
penaltySystemView.Add(passResult);
penaltySystemView.PaperSize = PaperSize.A3_Landscape;

// Injury System View (Focused)
var injurySystemView = views.CreateComponentView(stateLibrary, "InjurySystem", 
    "Position-specific injury system with fragility and recovery");
injurySystemView.Add(injuryOccurredCheck);
injurySystemView.Add(injuryEffect);
injurySystemView.Add(injuryProbabilities);
injurySystemView.Add(injury);
injurySystemView.Add(player);
injurySystemView.Add(game);
injurySystemView.Add(runExecution);
injurySystemView.Add(passExecution);
injurySystemView.Add(kickoffExecution);
injurySystemView.Add(puntExecution);
injurySystemView.Add(fieldGoalExecution);
injurySystemView.PaperSize = PaperSize.A3_Landscape;

// ================================================================================
// EXPORT TO PLANTUML
// ================================================================================

var plantUmlWriter = new PlantUMLWriter();

// Get the solution root directory by finding the .sln file
var currentDir = Directory.GetCurrentDirectory();
var solutionDir = currentDir;

// Navigate up to find the solution directory
while (solutionDir != null && !Directory.GetFiles(solutionDir, "*.sln").Any())
{
    var parent = Directory.GetParent(solutionDir);
    if (parent == null) break;
    solutionDir = parent.FullName;
}

// If we couldn't find .sln, assume we're in a subdirectory and go up one level
if (!Directory.GetFiles(solutionDir, "*.sln").Any())
{
 solutionDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
}

var outputPath = Path.Combine(solutionDir, "diagram");

// Create diagrams directory if it doesn't exist
if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

void WriteView(View view, string filename)
{
 var fullPath = Path.Combine(outputPath, filename);
using (var writer = new StreamWriter(fullPath))
    {
        switch (view)
 {
 case SystemLandscapeView v: plantUmlWriter.Write(v, writer); break;
   case SystemContextView v: plantUmlWriter.Write(v, writer); break;
     case ContainerView v: plantUmlWriter.Write(v, writer); break;
     case ComponentView v: plantUmlWriter.Write(v, writer); break;
        }
  }
    Console.WriteLine($"  ✓ Generated {filename}");
}

Console.WriteLine("================================================================================");
Console.WriteLine("GRIDIRON FOOTBALL SIMULATION - C4 DIAGRAM GENERATION");
Console.WriteLine("================================================================================");
Console.WriteLine($"System: {workspace.Name}");
Console.WriteLine($"Output: {outputPath}");
Console.WriteLine();
Console.WriteLine("Generating PlantUML diagrams...");
Console.WriteLine();

WriteView(contextView, "01-SystemContext.puml");
WriteView(containerView, "02-Containers.puml");
WriteView(domainComponentView, "03-DomainComponents.puml");
WriteView(stateLibraryComponentView, "04-StateLibraryComponents.puml");
WriteView(dataAccessComponentView, "05-DataAccessComponents.puml");
WriteView(testsComponentView, "06-TestComponents.puml");
WriteView(stateMachineView, "07-StateMachineFlow.puml");
WriteView(skillsChecksView, "08-SkillsChecks.puml");
WriteView(penaltySystemView, "09-PenaltySystem.puml");
WriteView(injurySystemView, "10-InjurySystem.puml");

Console.WriteLine();
Console.WriteLine("================================================================================");
Console.WriteLine("✓ COMPLETE - 10 diagrams generated");
Console.WriteLine("================================================================================");
Console.WriteLine();
Console.WriteLine("Generating PNG images from PlantUML files...");
Console.WriteLine();

// Generate PNGs from PlantUML files
try
{
    var factory = new RendererFactory();
    var renderer = factory.CreateRenderer(new PlantUmlSettings
    {
        RemoteUrl = "http://www.plantuml.com/plantuml"  // Use public PlantUML server
 });

    var pumlFiles = Directory.GetFiles(outputPath, "*.puml");
    int pngCount = 0;

    foreach (var pumlFile in pumlFiles)
  {
        try
        {
            var pumlContent = File.ReadAllText(pumlFile);
        var pngBytes = renderer.Render(pumlContent, OutputFormat.Png);
            
            var pngPath = Path.ChangeExtension(pumlFile, ".png");
      File.WriteAllBytes(pngPath, pngBytes);
        
     var fileName = Path.GetFileName(pngPath);
Console.WriteLine($"  ✓ Generated {fileName}");
            pngCount++;
        }
        catch (Exception ex)
 {
            Console.WriteLine($"  ✗ Failed to generate PNG for {Path.GetFileName(pumlFile)}: {ex.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"✓ Generated {pngCount} PNG images");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ PNG generation failed: {ex.Message}");
 Console.WriteLine("Note: PNG generation requires internet connection to PlantUML server");
}

Console.WriteLine();
Console.WriteLine("================================================================================");
Console.WriteLine("Diagrams:");
Console.WriteLine("  01 - System Context (users and external systems)");
Console.WriteLine("  02 - Containers (6 major components)");
Console.WriteLine("  03 - Domain Components (entities and helpers)");
Console.WriteLine("  04 - State Library Components (game engine)");
Console.WriteLine("  05 - Data Access Components (EF Core persistence)");
Console.WriteLine("  06 - Test Components (839 tests)");
Console.WriteLine("  07 - State Machine Flow (19 states)");
Console.WriteLine("  08 - Skills Checks (25+ checks)");
Console.WriteLine("  09 - Penalty System (50+ penalties)");
Console.WriteLine("  10 - Injury System (position-specific)");
Console.WriteLine();
Console.WriteLine("Output Formats:");
Console.WriteLine("  ✓ PlantUML (.puml) - Source files for editing");
Console.WriteLine("  ✓ PNG (.png) - Image files for viewing/documentation");
Console.WriteLine();
Console.WriteLine("Last Updated: November 2025");
Console.WriteLine("Test Status: 839/839 passing (100%)");
Console.WriteLine("================================================================================");
