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
var play = app.AddComponent("Play", "Represents a single play in the game.");
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
game.Uses(play, "Contains plays");
game.Uses(team, "Has teams");
game.Uses(coach, "Has coaches");
game.Uses(logger, "Logs game events");
game.Uses(replayLog, "Stores play-by-play data");
testSuite.Uses(testGame, "Runs test scenarios");
testGame.Uses(gameFlow, "Tests game flow");
testGame.Uses(seedableRandom, "Controls randomness in tests");

// 4. State Machine: States, Triggers, Events
var stateEnum = app.AddComponent("State", "Enumeration of game states (PreGame, CoinToss, PrePlay, etc.)");
var triggerEnum = app.AddComponent("Trigger", "Enumeration of triggers (Snap, CoinTossed, Fumble, PlayResult, etc.)");
stateMachine.Uses(stateEnum, "Uses states");
stateMachine.Uses(triggerEnum, "Uses triggers");

// 5. Views
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

// 6. Export all views to PlantUML
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

Console.WriteLine("PlantUML files generated:");
Console.WriteLine("  SystemLandscape.puml");
Console.WriteLine("  SystemContext.puml");
Console.WriteLine("  Container.puml");
Console.WriteLine("  Component.puml");
Console.WriteLine("Run 'plantuml <filename>.puml' to generate PNG diagrams.");
