# Gridiron Database Deployment Guide

## Overview

This guide explains how to deploy the Gridiron Football Simulation database to a new or existing Azure SQL Database instance. The deployment process is **fully reproducible** and can be executed on any Azure account.

---

## Prerequisites

Before deploying, ensure you have:

1. **.NET 8 SDK** installed
   ```bash
   dotnet --version  # Should be 8.0.x or higher
   ```

2. **EF Core Tools** installed globally
   ```bash
   dotnet tool install --global dotnet-ef
   # Or update if already installed:
   dotnet tool update --global dotnet-ef
   ```

3. **Azure SQL Database** provisioned with:
   - SQL Authentication enabled
   - Firewall rule allowing your IP address
   - Admin credentials (username/password)

---

## Deployment Steps

### Option 1: Using the Deployment Script (Recommended)

#### Windows (PowerShell)

```powershell
# Set your Azure SQL connection string
$connString = "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Run deployment
.\deploy-database.ps1 -ConnectionString $connString
```

#### Linux/Mac (Bash)

```bash
# Make script executable (first time only)
chmod +x deploy-database.sh

# Set your Azure SQL connection string
CONN_STRING="Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Run deployment
./deploy-database.sh --connection-string "$CONN_STRING"
```

**What the script does:**
1. ? Configures connection string in User Secrets
2. ? Applies EF Core migrations to create database schema
3. ? Runs seed data to populate teams and players

**Skip seeding (schema only):**
```powershell
# Windows
.\deploy-database.ps1 -ConnectionString $connString -SkipSeedData

# Linux/Mac
./deploy-database.sh --connection-string "$CONN_STRING" --skip-seed-data
```

---

### Option 2: Manual Step-by-Step

If you prefer manual control:

#### Step 1: Configure Connection String

```bash
cd DataAccessLayer

# Set connection string in User Secrets
dotnet user-secrets set "ConnectionStrings:GridironDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Step 2: Apply Migrations

```bash
# Still in DataAccessLayer directory
dotnet ef database update
```

This creates:
- `Teams` table
- `Players` table
- `Games` table
- `PlayByPlays` table
- All foreign keys, indexes, and constraints

#### Step 3: Seed Initial Data

```bash
# Still in DataAccessLayer directory
dotnet run
```

This populates:
- **Atlanta Falcons** team with full 53-man roster
- **Philadelphia Eagles** team with full 53-man roster
- All player attributes, depth charts

---

## Database Schema

### Tables Created

| Table | Description | Primary Key |
|-------|-------------|-------------|
| **Teams** | NFL teams | `Id` (int, identity) |
| **Players** | Player roster | `Id` (int, identity) |
| **Games** | Game records | `Id` (int, identity) |
| **PlayByPlays** | Play-by-play logs | `Id` (int, identity) |

### Relationships

```
Teams (1) ??? (?) Players
  ?
  ???? HomeTeam (?) Games
  ???? AwayTeam (?) Games
  
Games (1) ??? (1) PlayByPlays
```

### Key Features

- **Foreign Keys**: Proper relationships between entities
- **Cascade/Restrict**: Delete behavior configured
- **Indexes**: On TeamId, GameId for query performance
- **Nullable TeamId**: Players can be free agents (no team)
- **RandomSeed**: Games store seed for reproducible simulation

---

## Seed Data Details

The seeding process (`SeedDataRunner.cs`) populates:

### Teams
- **Atlanta Falcons** (City: Atlanta, Name: Falcons)
- **Philadelphia Eagles** (City: Philadelphia, Name: Eagles)

### Player Positions (per team)
- **Quarterbacks (QB)**: 3 players
- **Running Backs (RB)**: 4 players
- **Wide Receivers (WR)**: 6 players
- **Tight Ends (TE)**: 3 players
- **Offensive Line (T, G, C)**: 9 players
- **Defensive Line (DE, DT)**: 7 players
- **Linebackers (LB)**: 7 players
- **Defensive Backs (CB, S, FS)**: 8 players
- **Special Teams (K, P, LS)**: 3 players

**Total: ~106 players (53 per team)**

---

## Switching Azure Accounts

To redeploy on a **different Azure account**:

1. **Provision new Azure SQL Database**
   - Create new server or use existing
   - Create new database or clear existing
   - Configure firewall rules

2. **Run deployment script with new credentials**
   ```powershell
   # Windows
   .\deploy-database.ps1 -ConnectionString "Server=NEW_SERVER.database.windows.net;..."
   
   # Linux/Mac
   ./deploy-database.sh --connection-string "Server=NEW_SERVER.database.windows.net;..."
   ```

3. **Verify deployment**
   - Check tables exist: `Teams`, `Players`, `Games`, `PlayByPlays`
   - Verify seed data: Should have 2 teams, ~106 players

**That's it!** The entire database structure and initial data will be recreated.

---

## Verifying Deployment

### Using SQL Server Extension (VS Code)

1. Install **SQL Server (mssql)** extension
2. Connect to your Azure SQL Database
3. Expand **Tables** folder
4. Right-click `dbo.Teams` ? **Select Top 1000**
5. Should see 2 teams: Falcons and Eagles

### Using SQL Query

```sql
-- Check tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check seed data
SELECT 
    t.Name AS TeamName,
    COUNT(p.Id) AS PlayerCount
FROM Teams t
LEFT JOIN Players p ON t.Id = p.TeamId
GROUP BY t.Name;

-- Expected output:
-- Falcons   53
-- Eagles    53
```

### Using C# Code

```csharp
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<GridironDbContext>()
    .UseSqlServer("YOUR_CONNECTION_STRING")
    .Options;

using var db = new GridironDbContext(options);

var teamCount = await db.Teams.CountAsync();
var playerCount = await db.Players.CountAsync();

Console.WriteLine($"Teams: {teamCount}");    // Should be 2
Console.WriteLine($"Players: {playerCount}"); // Should be ~106
```

---

## Troubleshooting

### "Connection string not found"
**Problem**: Script can't find connection string

**Solutions**:
1. Use `-ConnectionString` parameter (PowerShell) or `--connection-string` (bash)
2. Or set manually then use `-UseUserSecrets` / `--use-user-secrets`
3. Check User Secrets ID: `gridiron-football-sim-2024`

### "Login failed for user"
**Problem**: SQL Authentication failed

**Solutions**:
1. Verify SQL Authentication is enabled on Azure SQL
2. Check username/password are correct
3. Confirm user has `db_datareader`, `db_datawriter`, `db_ddladmin` roles
4. Test connection with Azure Data Studio or SSMS first

### "Cannot connect to server"
**Problem**: Network/firewall blocking connection

**Solutions**:
1. Add your IP address to Azure SQL firewall rules
2. Check server name format: `yourserver.database.windows.net`
3. Verify port 1433 is not blocked by corporate firewall
4. Try from different network (home vs work)

### "Build failed" during migration
**Problem**: .NET project won't compile

**Solutions**:
1. Run `dotnet restore` in `DataAccessLayer` directory
2. Verify .NET 8 SDK installed: `dotnet --version`
3. Check for compilation errors: `dotnet build`
4. Ensure all NuGet packages restored

### "No executable found matching command 'dotnet-ef'"
**Problem**: EF Core tools not installed

**Solution**:
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Seeding fails but schema succeeds
**Problem**: Migration succeeded, but data seeding failed

**Solutions**:
1. Check connection string is valid
2. Verify foreign key constraints didn't fail
3. Run seeding manually:
   ```bash
   cd DataAccessLayer
   dotnet run
   ```
4. Review error messages for specific issues

---

## Clean Slate Deployment

To completely **rebuild database** from scratch:

### Option 1: Drop and Recreate Database (Azure Portal)
1. Delete existing database in Azure Portal
2. Create new database with same name
3. Run deployment script

### Option 2: Clear All Data (Keep Structure)
```sql
-- WARNING: This deletes ALL data!
DELETE FROM PlayByPlays;
DELETE FROM Games;
DELETE FROM Players;
DELETE FROM Teams;

-- Reset identity seeds
DBCC CHECKIDENT ('Teams', RESEED, 0);
DBCC CHECKIDENT ('Players', RESEED, 0);
DBCC CHECKIDENT ('Games', RESEED, 0);
DBCC CHECKIDENT ('PlayByPlays', RESEED, 0);
```

Then run seeding only:
```bash
cd DataAccessLayer
dotnet run
```

### Option 3: Drop and Recreate All Tables (Code)
```bash
# From solution root (C:\projects\gridiron)

# Remove existing migrations
rm -rf DataAccessLayer/Migrations/

# Recreate migration
dotnet ef migrations add InitialCreate --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj

# Apply migration (will fail if tables exist)
dotnet ef database update --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj

# Run seeding
cd DataAccessLayer
dotnet run
cd ..
```

**Important:** Use forward slashes (`/`) not backslashes (`\`)

---

## Going Forward

### Adding New Migrations

When you modify domain models (`Team`, `Player`, `Game`, `League`, etc.):

```bash
# From solution root (C:\projects\gridiron)

# Create new migration
dotnet ef migrations add DescriptiveNameForChange --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj

# Apply to database
dotnet ef database update --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
```

**Important:** Use forward slashes (`/`) not backslashes (`\`)

### Adding More Seed Data

1. Create new seeder class in `DataAccessLayer/SeedData/`
2. Follow pattern of existing seeders (e.g., `FalconsQBSeeder.cs`)
3. Call from `SeedDataRunner.cs` in `RunAsync()` method

Example:
```csharp
public static class NewTeamSeeder
{
    public static async Task SeedAsync(GridironDbContext db, int teamId)
    {
        var players = new List<Player>
        {
            // Define players here
        };
        
        db.Players.AddRange(players);
        await db.SaveChangesAsync();
    }
}
```

---

## Summary

? **Fully reproducible** database deployment  
? **Schema migrations** via EF Core  
? **Seed data** for 2 complete teams  
? **Scripts** for Windows and Linux/Mac  
? **Clean separation** of concerns (DomainObjects vs DataAccessLayer)  
? **Future-proof** - easy to add teams, migrations, data  

**Switching Azure accounts?** Just run the deployment script with new credentials. **That's it!**
