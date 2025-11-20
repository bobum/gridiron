# Gridiron Football Simulation API

A REST API built on ASP.NET Core 8.0 for running football game simulations and accessing team/player data.

## Features

- **Game Simulation**: Run complete football game simulations with realistic play-by-play
- **Team Management**: Access team information and rosters
- **Player Stats**: View detailed player information and statistics
- **Play-by-Play Data**: Get detailed play-by-play breakdowns of simulated games

## API Endpoints

### Games

- `POST /api/games/simulate` - Simulate a new game
- `GET /api/games` - Get all simulated games
- `GET /api/games/{id}` - Get a specific game
- `GET /api/games/{id}/plays` - Get play-by-play data for a game

### Teams

- `GET /api/teams` - Get all teams
- `GET /api/teams/{id}` - Get a specific team
- `GET /api/teams/{id}/roster` - Get team roster with all players

### Players

- `GET /api/players` - Get all players (optional: filter by teamId)
- `GET /api/players/{id}` - Get specific player with stats

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)

### Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GridironDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Running the API

```bash
dotnet run
```

The API will start at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

Swagger UI is available at the root URL when running in Development mode.

## Example Requests

### Simulate a Game

```bash
POST /api/games/simulate
Content-Type: application/json

{
  "homeTeamId": 1,
  "awayTeamId": 2,
  "randomSeed": 12345  // Optional - for reproducible simulations
}
```

### Get Team Roster

```bash
GET /api/teams/1/roster
```

### Get Player Details

```bash
GET /api/players/1
```

## Architecture

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic for game simulation
- **DTOs**: Data transfer objects for API responses
- **Database**: Entity Framework Core with SQL Server

## Dependencies

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- Swashbuckle (Swagger/OpenAPI)
- Gridiron Simulation Engine (DomainObjects, StateLibrary, DataAccessLayer)
