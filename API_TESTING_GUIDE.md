# API Testing Guide

This guide explains how to test the Gridiron Football Simulation API to verify it's working correctly and properly enforcing the repository pattern.

---

## Prerequisites

Before you can test the API, you need:

### 1. **Database Setup**
- Azure SQL Database must be running
- Connection string configured in user secrets or appsettings.json
- Database migrations applied (tables created)
- **At least one team with players in the database**

**Check if you have data:**
```sql
-- Connect to your Azure SQL database and run:
SELECT COUNT(*) FROM Teams;
SELECT COUNT(*) FROM Players;
```

If you get 0 for either, you'll need to seed data first.

### 2. **Connection String**
Verify your connection string is configured:

**Option A - Check User Secrets:**
```bash
cd Gridiron.WebApi
dotnet user-secrets list
```
You should see: `ConnectionStrings:GridironDb = Server=tcp:...`

**Option B - Check appsettings.json:**
Make sure the placeholders are replaced with real values.

---

## How to Run the API

### 1. **Start the API**
```bash
cd Gridiron.WebApi
dotnet run
```

**Expected Output:**
```
info: Gridiron Football Simulation API started
info: Swagger UI available at: http://localhost:5000
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

### 2. **Verify Swagger UI**
Open your browser to:
- **http://localhost:5000** (Swagger UI should load)

You should see the API documentation with:
- Games endpoints (POST /api/games/simulate, GET /api/games, etc.)
- Teams endpoints (GET /api/teams, GET /api/teams/{id}/roster)
- Players endpoints (GET /api/players, GET /api/players/{id})

---

## Testing the Repository Pattern

These tests verify that:
1. The API can connect to the database through repositories
2. Teams and players can be loaded
3. No direct DbContext access is happening (architectural compliance)

### Test 1: Get All Teams

**Purpose:** Verify basic repository pattern works (ITeamRepository → TeamRepository → DbContext)

**Using Swagger UI:**
1. Expand `GET /api/teams`
2. Click "Try it out"
3. Click "Execute"

**Using curl:**
```bash
curl http://localhost:5000/api/teams
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "Falcons",
    "city": "Atlanta",
    "budget": 200000000,
    "championships": 0,
    "wins": 0,
    "losses": 0,
    "ties": 0,
    "fanSupport": 75,
    "chemistry": 80
  },
  {
    "id": 2,
    "name": "Eagles",
    "city": "Philadelphia",
    ...
  }
]
```

**What This Tests:**
- ✅ `TeamsController` → `ITeamRepository` → `TeamRepository` → `DbContext` → Azure SQL
- ✅ Repository pattern is working
- ✅ No direct DbContext access in controller

---

### Test 2: Get Team with Roster (Most Important)

**Purpose:** Verify Include() relationships work through repositories

**Using Swagger UI:**
1. Expand `GET /api/teams/{id}/roster`
2. Click "Try it out"
3. Enter team ID: `1` (or whatever ID you got from Test 1)
4. Click "Execute"

**Using curl:**
```bash
curl http://localhost:5000/api/teams/1/roster
```

**Expected Response:**
```json
{
  "id": 1,
  "name": "Falcons",
  "city": "Atlanta",
  "budget": 200000000,
  "championships": 0,
  "wins": 0,
  "losses": 0,
  "ties": 0,
  "fanSupport": 75,
  "chemistry": 80,
  "roster": [
    {
      "id": 1,
      "firstName": "Matt",
      "lastName": "Ryan",
      "fullName": "Matt Ryan",
      "position": "QB",
      "number": 2,
      "height": "6-4",
      "weight": 220,
      "age": 28,
      "exp": 7,
      "college": "Boston College",
      "teamId": 1,
      "speed": 65,
      "strength": 70,
      "agility": 75,
      "awareness": 95,
      "morale": 85,
      "discipline": 90,
      "passing": 95,
      "catching": 0,
      "rushing": 60,
      "blocking": 0,
      "tackling": 0,
      "coverage": 0,
      "kicking": 0,
      "health": 100,
      "isInjured": false
    }
    // ... more players (should be 53 for a full roster)
  ],
  "headCoach": {
    "firstName": "Arthur",
    "lastName": "Smith",
    "fullName": "Arthur Smith"
  }
}
```

**What This Tests:**
- ✅ `TeamsController` uses `ITeamRepository.GetByIdWithPlayersAsync()`
- ✅ Repository includes players (tests the `Include()` in repository)
- ✅ Full team roster is loaded through DAL
- ✅ **This is the key test - proves the repository pattern works end-to-end**

**Important:** The `roster` array should contain players. If it's empty `[]`, the repository may not be including players correctly.

---

### Test 3: Get All Players for a Team

**Purpose:** Verify filtering/querying works through repositories

**Using Swagger UI:**
1. Expand `GET /api/players`
2. Click "Try it out"
3. Enter teamId query parameter: `1`
4. Click "Execute"

**Using curl:**
```bash
curl http://localhost:5000/api/players?teamId=1
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "firstName": "Matt",
    "lastName": "Ryan",
    "fullName": "Matt Ryan",
    "position": "QB",
    "number": 2,
    ...
  },
  {
    "id": 2,
    "firstName": "Julio",
    "lastName": "Jones",
    "position": "WR",
    ...
  }
  // ... all players on team 1
]
```

**What This Tests:**
- ✅ `PlayersController` → `IPlayerRepository.GetByTeamIdAsync()`
- ✅ Query filtering in repository (WHERE clause)
- ✅ Repository pattern handles conditional queries

---

### Test 4: Get Single Player

**Purpose:** Verify single entity retrieval through repositories

**Using Swagger UI:**
1. Expand `GET /api/players/{id}`
2. Click "Try it out"
3. Enter player ID: `1` (from previous test)
4. Click "Execute"

**Using curl:**
```bash
curl http://localhost:5000/api/players/1
```

**Expected Response:**
```json
{
  "id": 1,
  "firstName": "Matt",
  "lastName": "Ryan",
  "fullName": "Matt Ryan",
  "position": "QB",
  "number": 2,
  "height": "6-4",
  "weight": 220,
  "age": 28,
  "exp": 7,
  "college": "Boston College",
  "teamId": 1,
  "speed": 65,
  "strength": 70,
  "agility": 75,
  "awareness": 95,
  "morale": 85,
  "discipline": 90,
  "passing": 95,
  "catching": 0,
  "rushing": 60,
  "blocking": 0,
  "tackling": 0,
  "coverage": 0,
  "kicking": 0,
  "health": 100,
  "isInjured": false,
  "gameStats": {},
  "seasonStats": {},
  "careerStats": {}
}
```

**What This Tests:**
- ✅ `PlayersController` → `IPlayerRepository.GetByIdAsync()`
- ✅ Single entity retrieval through repository

---

### Test 5: Simulate a Game (Advanced)

**Purpose:** Verify the game simulation service works through repositories

**Using Swagger UI:**
1. Expand `POST /api/games/simulate`
2. Click "Try it out"
3. Enter request body:
```json
{
  "homeTeamId": 1,
  "awayTeamId": 2,
  "randomSeed": 12345
}
```
4. Click "Execute"

**Using curl:**
```bash
curl -X POST http://localhost:5000/api/games/simulate \
  -H "Content-Type: application/json" \
  -d '{
    "homeTeamId": 1,
    "awayTeamId": 2,
    "randomSeed": 12345
  }'
```

**Expected Response:**
```json
{
  "id": 1,
  "homeTeamId": 1,
  "awayTeamId": 2,
  "homeTeamName": "Atlanta Falcons",
  "awayTeamName": "Philadelphia Eagles",
  "homeScore": 24,
  "awayScore": 17,
  "randomSeed": 12345,
  "isComplete": true,
  "totalPlays": 143
}
```

**What This Tests:**
- ✅ `GameSimulationService` uses `ITeamRepository.GetByIdWithPlayersAsync()`
- ✅ Game simulation engine runs
- ✅ `GameSimulationService` uses `IGameRepository.AddAsync()` to save results
- ✅ Full end-to-end flow through repository pattern

**Note:** This may take 10-30 seconds depending on simulation complexity.

---

## Verify Repository Pattern is Enforced

### Check the Logs

When you run the API, watch for database queries in the console. You should see:
- EF Core query logs (if logging level is set to Debug)
- No errors about "DbContext not registered" or "No service for type GridironDbContext"
- Successful HTTP status codes (200, 404, etc.)

### Check the Code

Open these files and verify there's NO direct DbContext access:

**✅ These should use repositories:**
- `Gridiron.WebApi/Controllers/TeamsController.cs` → Should have `ITeamRepository`, NOT `GridironDbContext`
- `Gridiron.WebApi/Controllers/PlayersController.cs` → Should have `IPlayerRepository`, NOT `GridironDbContext`
- `Gridiron.WebApi/Controllers/GamesController.cs` → Should have `IGameSimulationService`, NOT `GridironDbContext`
- `Gridiron.WebApi/Services/GameSimulationService.cs` → Should have `ITeamRepository` and `IGameRepository`, NOT `GridironDbContext`

**✅ Only this should access DbContext:**
- `DataAccessLayer/Repositories/*.cs` files

### Red Flags in Code Review

If you see ANY of these in `Gridiron.WebApi/**`:
- ❌ `using Microsoft.EntityFrameworkCore;`
- ❌ `private readonly GridironDbContext _context;`
- ❌ `.Include()`, `.FirstOrDefaultAsync()`, `.ToListAsync()` directly in controllers
- ❌ `_context.Teams`, `_context.Players`, `_context.Games`

**These are violations of the architectural principle.**

---

## Common Issues

### Issue: "Connection string 'GridironDb' not found"
**Fix:** Configure user secrets:
```bash
cd Gridiron.WebApi
dotnet user-secrets set "ConnectionStrings:GridironDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Issue: "Login failed for user"
**Fix:**
- Verify connection string credentials are correct
- Check Azure SQL firewall rules include your IP address
- Confirm SQL Authentication is enabled (not just Azure AD)

### Issue: Empty arrays returned `[]`
**Fix:** Database has no data. You need to seed teams and players.

**Quick check:**
```sql
SELECT * FROM Teams;
SELECT * FROM Players;
```

If tables are empty, you'll need a database seeder.

### Issue: 500 Internal Server Error
**Fix:**
- Check console output for the actual error
- Look for database connection issues
- Verify all migrations have been applied
- Check repository is registered in DI (`Program.cs`)

### Issue: "No service for type ITeamRepository"
**Fix:**
Verify `Program.cs` has:
```csharp
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
```

### Issue: Roster returns empty array but team exists
**Fix:**
- Check `TeamRepository.GetByIdWithPlayersAsync()` includes `.Include(t => t.Players)`
- Verify players exist in database for that team: `SELECT * FROM Players WHERE TeamId = 1`

---

## Success Criteria

You'll know the API and repository pattern are working correctly if:

1. ✅ `GET /api/teams` returns teams (proves basic repository works)
2. ✅ `GET /api/teams/1/roster` returns team WITH players in roster array (proves Include() works)
3. ✅ `GET /api/players?teamId=1` returns players for that team (proves filtering works)
4. ✅ `GET /api/players/1` returns single player (proves single entity retrieval works)
5. ✅ `POST /api/games/simulate` runs and returns game result (proves full simulation works)
6. ✅ No errors in console logs
7. ✅ No `GridironDbContext` references in `Gridiron.WebApi/**` files
8. ✅ All controllers use `I*Repository` interfaces
9. ✅ Swagger UI loads and all endpoints are documented

---

## Performance Testing

### Check Response Times

All endpoints should respond in:
- GET requests: < 500ms
- POST /api/games/simulate: 10-30 seconds (full game simulation)

If responses are slow:
- Check database connection latency to Azure SQL
- Consider adding indexes on foreign keys
- Review EF Core query logs for N+1 query issues

### Load Testing (Optional)

Use a tool like Apache Bench to test concurrent requests:

```bash
# Test 100 requests with 10 concurrent
ab -n 100 -c 10 http://localhost:5000/api/teams
```

---

## Next Steps After Successful Testing

Once all tests pass:

1. **Add Authentication** - Secure the API endpoints
2. **Add Rate Limiting** - Prevent API abuse
3. **Add Caching** - Cache team/player data
4. **Add Pagination** - For large result sets
5. **Add SignalR** - For real-time game simulation updates
6. **Deploy to Azure** - App Service or Container Apps

---

## Documentation References

- [ARCHITECTURE_PRINCIPLES.md](ARCHITECTURE_PRINCIPLES.md) - The repository pattern and database access rules
- [DATABASE_SETUP.md](DATABASE_SETUP.md) - Database configuration and migrations
- [Gridiron.WebApi/README.md](Gridiron.WebApi/README.md) - API project documentation

---

## Questions?

If tests fail or you need help:

1. Check console logs for detailed error messages
2. Verify database connection with SQL query tool
3. Review [ARCHITECTURE_PRINCIPLES.md](ARCHITECTURE_PRINCIPLES.md) for proper patterns
4. Check that all repositories are registered in `Program.cs`
5. Ensure migrations have been applied to the database
