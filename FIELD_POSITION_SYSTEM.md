# Field Position System

## Overview

The Gridiron simulation uses a dual representation for field position:
- **Internal (0-100)**: Used for game logic and calculations
- **NFL Notation**: Used for display and logging

## Internal Representation (0-100)

The game engine uses absolute field position from the offensive team's perspective:

```
Offense Goal Line                 Midfield                    Opponent Goal Line
        0 ─────────────────────────── 50 ─────────────────────────── 100
     (Own End Zone)                                           (Trying to Score)
```

- **0** = Offense's own goal line (defense scores a safety here)
- **50** = Midfield (50-yard line)
- **100** = Opponent's goal line (offense scores a touchdown here)

### Examples (Internal):
- `FieldPosition = 20` → Ball is 20 yards from offense's goal
- `FieldPosition = 80` → Ball is 20 yards from opponent's goal (Red Zone!)
- `FieldPosition = 50` → Midfield

## NFL Notation (Display)

When logging and displaying field position, we convert to standard NFL notation:

### Format
- **Own side (0-49)**: `"[Team Name] [YardLine]"` or `"Own [YardLine]"`
- **Midfield (50)**: `"50"` or `"50 yard line (midfield)"`
- **Opponent side (51-100)**: `"[Opponent Name] [YardLine]"` or `"Opp [YardLine]"`

### Conversion Examples

| Internal | Offensive Team | NFL Notation Display |
|----------|----------------|----------------------|
| 0 | Buffalo | Buffalo 0 (own goal line) |
| 20 | Buffalo | Buffalo 20 |
| 45 | Buffalo | Buffalo 45 |
| 50 | Buffalo | 50 (midfield) |
| 60 | Buffalo | Kansas City 40 |
| 80 | Buffalo | Kansas City 20 (RED ZONE) |
| 97 | Buffalo | Kansas City 3 |
| 100 | Buffalo | Kansas City 0 (touchdown!) |

## Helper Methods

### FieldPositionHelper Class
Located in `DomainObjects/Helpers/FieldPositionHelper.cs`

```csharp
// Convert field position to NFL notation
string nflPosition = FieldPositionHelper.FormatFieldPosition(80, offenseTeam, defenseTeam);
// Returns: "Kansas City 20"

// With "yard line" suffix
string withSuffix = FieldPositionHelper.FormatFieldPositionWithYardLine(80, offenseTeam, defenseTeam);
// Returns: "Kansas City 20 yard line"

// Check if in Red Zone (opponent's 20 or closer)
bool isRedZone = FieldPositionHelper.IsInRedZone(80); // true

// Check if Goal-to-Go (opponent's 10 or closer)
bool isGoalToGo = FieldPositionHelper.IsGoalToGo(95); // true
```

### Game Class Methods
The `Game` class provides convenient wrappers:

```csharp
// Format current field position for the team with possession
string position = game.FormatFieldPosition(Possession.Home);

// Format with "yard line" suffix
string positionWithYardLine = game.FormatFieldPositionWithYardLine(Possession.Home);

// Get offensive/defensive team
Team offense = game.GetOffensiveTeam(Possession.Home);
Team defense = game.GetDefensiveTeam(Possession.Home);
```

## Logging Examples

### Before (Confusing)
```
First down! Ball at the 97 yard line.
Runner is down. 2nd and 3 at the 68.
```

### After (NFL Standard)
```
First down! Ball at the Kansas City 3 yard line.
Runner is down. 2nd and 3 at the Kansas City 32.
```

## Special Zones

### Red Zone
- Internal: `FieldPosition >= 80`
- Display: Opponent's 20 yard line or closer
- Method: `FieldPositionHelper.IsInRedZone(fieldPosition)`

### Goal-to-Go
- Internal: `FieldPosition >= 90`
- Display: Opponent's 10 yard line or closer
- Method: `FieldPositionHelper.IsGoalToGo(fieldPosition)`

### Midfield
- Internal: `FieldPosition == 50`
- Display: "50" or "50 yard line (midfield)"

## Implementation Status

### ✅ Completed
- FieldPositionHelper class with all conversion methods
- Game class helper methods for easy access
- PassResult logging updated to use NFL notation
- RunResult logging updated to use NFL notation

### ⚠️ In Progress
- FieldGoalResult logging (partially updated)
- PuntResult logging (partially updated)
- KickoffResult logging (partially updated)
- Kickoff.cs, Punt.cs, Pass.cs, Run.cs play execution logging

### 📋 To Do
- Update remaining logging statements in special teams plays
- Update penalty enforcement logging
- Update interception/fumble return logging
- Update test assertions to expect NFL notation

## Technical Notes

### Why Keep Internal 0-100?
1. **Simplicity**: Easy to calculate yards gained (just add to field position)
2. **Boundary Detection**: Simple checks for touchdowns (>= 100) and safeties (<= 0)
3. **Consistency**: No need to flip field direction when possession changes
4. **Math**: Distance to goal is simply `100 - FieldPosition`

### Why Convert for Display?
1. **NFL Standard**: Matches how real football is described
2. **User Understanding**: Familiar to anyone who watches football
3. **Clarity**: "Kansas City 5" is clearer than "95 yard line"
4. **Context**: Immediately shows which team's territory

## Future Enhancements

Potential additions to the field position system:

1. **Play-by-Play Descriptions**
   - "Deep in their own territory" (0-20)
   - "Backed up" (0-10)
   - "In field goal range" (57+, based on kicker ability)

2. **Statistical Tracking**
   - Average field position per drive
   - Field position win probability
   - Expected points by field position

3. **Situational Awareness**
   - "Plus territory" (opponent's side, 50+)
   - "Long field" vs "Short field"
   - "Pinned deep" (< 10)
