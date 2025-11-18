# Gridiron Console - Football Game Simulator

A simple console application to run and view fully randomized football game simulations with complete play-by-play output.

## How to Run

### Option 1: Using Visual Studio
1. Open `gridiron.sln` in Visual Studio
2. Right-click on **GridironConsole** project
3. Select **Set as Startup Project**
4. Press `F5` or click **Start** to run

### Option 2: Using Command Line (if .NET CLI is available)
```bash
cd GridironConsole
dotnet run
```

### Option 3: Using the Test Framework (No CLI Required)
If you don't have .NET CLI available, you can use the existing test to view a game:

1. Open `UnitTestProject1/FlowTests.cs` in Visual Studio
2. Find the test method `GameTestWithPlayByPlayCapture`
3. Right-click on the test name
4. Select **Run Test**
5. Open **Test Explorer** → **Output** to see the complete play-by-play

## What You'll See

When you run the console app, it will:

1. **Display the matchup** - Shows which teams are playing
2. **Simulate the entire game** - Runs through all 4 quarters with play-by-play commentary
3. **Show final results** - Displays:
   - Final score
   - Winner
   - Total plays executed
   - Play breakdown (pass/run/special teams)
   - Completion percentage
   - Turnover statistics

## Sample Output

```
================================================================================
                    GRIDIRON FOOTBALL SIMULATION
================================================================================

MATCHUP: Kansas City Chiefs @ Buffalo Bills
Location: Highmark Stadium

Press ENTER to start the game...

Starting game simulation...
================================================================================

[Information] Game Starting
[Information] Kickoff...
[Information] Kickoff returned for 23 yards
[Information] 1st and 10 at the 23-yard line
[Information] Pass play - COMPLETE for 8 yards
[Information] 2nd and 2 at the 31-yard line
...

================================================================================
                         FINAL SCORE
================================================================================

Kansas City Chiefs: 27
Buffalo Bills: 24

WINNER: Kansas City Chiefs

================================================================================
                         GAME STATISTICS
================================================================================

Total Plays: 142
Game Duration: 60:00

Play Breakdown:
  Pass Plays: 58 (37 completions, 63.8% completion rate)
  Run Plays: 52
  Kickoffs: 9
  Punts: 6
  Field Goals: 5

Turnovers:
  Interceptions: 2
  Fumbles: 3
```

## Customization

To modify the simulation:

- **Change teams**: Edit `GameHelper.GetNewGame()` in `DomainObjects/Helpers/GameHelper.cs`
- **Use seeded randomization**: Replace `new SeedableRandom()` with `new SeedableRandom(12345)` for reproducible games
- **Adjust logging detail**: Change `LogLevel.Information` to `LogLevel.Debug` for more detail
- **Save output to file**: Redirect console output with `dotnet run > game_output.txt`

## How It Works

The console app:
1. Creates a new `Game` object with two teams
2. Initializes a `SeedableRandom` for randomized outcomes
3. Sets up a console logger to display play-by-play
4. Creates a `GameFlow` state machine to execute the game
5. Calls `gameFlow.Execute()` to simulate all plays
6. Displays final statistics and results

The simulation uses your existing statistical models for:
- Pass completion probabilities
- Run yard calculations
- Fumble/interception chances
- Penalty occurrences based on player discipline
- Field goal accuracy

## Next Steps

This is the simplest way to view your simulation in action. Future enhancements could include:
- Interactive play calling (choose plays manually)
- Save game results to file
- Season simulation (multiple games)
- Team selection menu
- Statistics tracking across games
