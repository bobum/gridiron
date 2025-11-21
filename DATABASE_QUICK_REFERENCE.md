# Gridiron Database Quick Reference

## Initial Setup (First Time)

### Windows
```powershell
.\deploy-database.ps1 -ConnectionString "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;"
```

### Linux/Mac
```bash
chmod +x deploy-database.sh
./deploy-database.sh --connection-string "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;"
```

---

## Quick Commands

### Rebuild Everything (New Azure Account)
```bash
# Just run the deployment script with new connection string
.\deploy-database.ps1 -ConnectionString "NEW_CONNECTION_STRING"
```

### Schema Only (No Seed Data)
```bash
.\deploy-database.ps1 -ConnectionString "..." -SkipSeedData
```

### Re-run Seed Data Only
```bash
cd DataAccessLayer
dotnet run
cd ..
```

### Check Database Status
```sql
-- Count teams and players
SELECT 'Teams' AS Entity, COUNT(*) AS Count FROM Teams
UNION ALL
SELECT 'Players', COUNT(*) FROM Players
UNION ALL
SELECT 'Games', COUNT(*) FROM Games;
```

---

## When You Add New Features

### Added/Changed Domain Models?
```bash
# From solution root (C:\projects\gridiron)
dotnet ef migrations add YourChangeDescription --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
dotnet ef database update --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
```

**Important:** Use forward slashes (`/`) not backslashes (`\`)

### Want to Reset Database?
```bash
# Option 1: Drop database in Azure Portal, then redeploy
.\deploy-database.ps1 -ConnectionString "..."

# Option 2: Clear data, keep schema
cd DataAccessLayer
dotnet run  # Will prompt to clear existing data
cd ..
```

---

## Verify Deployment Worked

```sql
-- Should see 4 tables
SELECT name FROM sys.tables ORDER BY name;
-- Expected: Games, PlayByPlays, Players, Teams

-- Should see 2 teams with ~53 players each
SELECT 
    t.City + ' ' + t.Name AS Team,
    COUNT(p.Id) AS Players
FROM Teams t
LEFT JOIN Players p ON t.Id = p.TeamId
GROUP BY t.City, t.Name;
-- Expected:
-- Atlanta Falcons     53
-- Philadelphia Eagles 53
```

---

## Connection String Format

```
Server=tcp:YOUR_SERVER.database.windows.net,1433;
Initial Catalog=YOUR_DATABASE;
User ID=YOUR_USERNAME;
Password=YOUR_PASSWORD;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

Replace:
- `YOUR_SERVER` - Azure SQL server name (without .database.windows.net)
- `YOUR_DATABASE` - Database name (e.g., gridiron)
- `YOUR_USERNAME` - SQL admin username
- `YOUR_PASSWORD` - SQL admin password

---

## File Structure

```
/gridiron
??? deploy-database.ps1          # ? Run this (Windows)
??? deploy-database.sh           # ? Run this (Linux/Mac)
??? DATABASE_DEPLOYMENT.md       # ? Full documentation
??? DATABASE_SETUP.md            # ? Original setup guide
??? DataAccessLayer/
?   ??? Migrations/              # EF Core migrations
?   ??? SeedData/                # Team/player data
?   ??? GridironDbContext.cs     # Database configuration
?   ??? appsettings.json         # Connection string (template)
??? DomainObjects/
    ??? Team.cs                  # Domain models
    ??? Player.cs
    ??? Game.cs
```

---

## Common Issues

| Problem | Solution |
|---------|----------|
| "Connection string not found" | Pass `-ConnectionString` to script |
| "Login failed" | Check username/password, verify SQL Auth enabled |
| "Cannot connect" | Add IP to Azure SQL firewall rules |
| "Build failed" | Run `dotnet restore` in DataAccessLayer |
| "dotnet-ef not found" | Run `dotnet tool install --global dotnet-ef` |

---

## Going Forward

? **Switching Azure accounts?**  
Just run `deploy-database.ps1` with new connection string

? **Need to reset database?**  
Run `deploy-database.ps1` again (will prompt before clearing data)

? **Adding more teams?**  
Create new seeder in `DataAccessLayer/SeedData/`, add to `SeedDataRunner.cs`

? **Changed domain models?**  
Run `dotnet ef migrations add YourChange` then `dotnet ef database update`

---

**That's it!** The entire database infrastructure is now reproducible and portable across Azure accounts.
