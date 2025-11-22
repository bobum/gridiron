# Database Management Guide

This guide explains how to manage your Gridiron database using the provided PowerShell scripts.

## Available Scripts

### 1. `reset-database.ps1` ⚠️ **DESTRUCTIVE - Use for Fresh Start**

**What it does:**
- **Drops the entire database** (all data permanently deleted!)
- Recreates all tables using EF Core migrations
- Seeds initial data (names, colleges, teams, players)

**When to use:**
- Starting fresh with a clean database
- Fixing migration/schema corruption issues
- Resetting to known good state for testing
- After pulling major database schema changes

**Usage:**

```powershell
# Interactive (prompts for confirmation)
.\reset-database.ps1 -UseUserSecrets

# With connection string
.\reset-database.ps1 -ConnectionString "Server=..."

# Skip confirmation prompt (dangerous!)
.\reset-database.ps1 -UseUserSecrets -Force

# Reset schema only, skip seed data
.\reset-database.ps1 -UseUserSecrets -SkipSeedData
```

---

### 2. `deploy-database.ps1` ✅ **SAFE - Use for Updates**

**What it does:**
- Applies new EF Core migrations (preserves existing data)
- Optionally seeds data if tables are empty

**When to use:**
- Applying new migrations after `git pull`
- Initial database setup (creates tables if they don't exist)
- Adding new tables/columns without losing data

**Usage:**

```powershell
# Standard deployment
.\deploy-database.ps1 -UseUserSecrets

# With connection string
.\deploy-database.ps1 -ConnectionString "Server=..."

# Apply migrations only, skip seeding
.\deploy-database.ps1 -UseUserSecrets -SkipSeedData
```

---

## Common Workflows

### First-Time Setup

```powershell
# Option 1: Use reset script (recommended for first time)
.\reset-database.ps1 -UseUserSecrets

# Option 2: Use deploy script (also works for first time)
.\deploy-database.ps1 -UseUserSecrets
```

### After Pulling Code Changes

```powershell
# Check for new migrations
cd DataAccessLayer
dotnet ef migrations list

# Apply new migrations (keeps existing data)
cd ..
.\deploy-database.ps1 -UseUserSecrets
```

### Complete Database Reset

```powershell
# WARNING: This deletes ALL data!
.\reset-database.ps1 -UseUserSecrets
```

### Manual Migration Commands

```powershell
cd DataAccessLayer

# List all migrations
dotnet ef migrations list

# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update <MigrationName>

# Rollback to previous migration
dotnet ef database update <PreviousMigrationName>

# Drop database
dotnet ef database drop --force

# Create new migration (after changing models)
dotnet ef migrations add <MigrationName>
```

### Manual Seeding

```powershell
cd DataAccessLayer
dotnet run
```

---

## What Gets Seeded

When you run seeding (via scripts or `dotnet run`), the following data is loaded:

### Player Generation Data
- **FirstNames**: ~150 common first names for random player generation
- **LastNames**: ~100 common last names for random player generation
- **Colleges**: ~130 NCAA colleges for player backgrounds

**Source Files:**
- `Gridiron.WebApi/SeedData/FirstNames.json`
- `Gridiron.WebApi/SeedData/LastNames.json`
- `Gridiron.WebApi/SeedData/Colleges.json`

### Team Data
- **Atlanta Falcons** (full 53-player roster)
- **Philadelphia Eagles** (full 53-player roster)

**Source Code:**
- `DataAccessLayer/SeedData/TeamSeeder.cs`
- `DataAccessLayer/SeedData/Falcons/*.cs`
- `DataAccessLayer/SeedData/Eagles/*.cs`

---

## Connection String Setup

### Option 1: User Secrets (Recommended for Development)

```powershell
cd DataAccessLayer
dotnet user-secrets set "ConnectionStrings:GridironDb" "Server=your-server;Database=GridironDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
cd ..
```

Then use scripts with `-UseUserSecrets` flag.

### Option 2: Pass Connection String Directly

```powershell
.\reset-database.ps1 -ConnectionString "Server=your-server;Database=GridironDb;..."
```

### Connection String Examples

**Local SQL Server:**
```
Server=localhost;Database=GridironDb;Integrated Security=true;TrustServerCertificate=true
```

**Azure SQL Database:**
```
Server=tcp:your-server.database.windows.net,1433;Database=GridironDb;User Id=admin;Password=YourPassword;Encrypt=true
```

**SQL Server Express:**
```
Server=localhost\SQLEXPRESS;Database=GridironDb;Integrated Security=true;TrustServerCertificate=true
```

---

## Troubleshooting

### "Invalid object name 'FirstNames'" Error

**Problem:** Migration `AddPlayerGenerationTables` hasn't been applied.

**Solution:**
```powershell
.\deploy-database.ps1 -UseUserSecrets
```

### Migration Conflicts

**Problem:** `dotnet ef database update` fails with migration conflicts.

**Solution:**
```powershell
# Nuclear option - reset everything
.\reset-database.ps1 -UseUserSecrets

# Or rollback to specific migration
cd DataAccessLayer
dotnet ef database update <GoodMigrationName>
```

### Web API Running During Build

**Problem:** Build fails with "The process cannot access the file... because it is being used by another process."

**Solution:** Stop the Web API before running scripts:
```powershell
# Find the process
Get-Process | Where-Object { $_.ProcessName -like "*Gridiron*" }

# Kill it
Stop-Process -Name "Gridiron.WebApi" -Force
```

### Seeding Fails

**Problem:** Seeding completes but data isn't in database.

**Check:**
1. Connection string points to correct database
2. Migrations applied successfully first
3. Check `SeedData` folder has JSON files:
   - `Gridiron.WebApi/SeedData/FirstNames.json`
   - `Gridiron.WebApi/SeedData/LastNames.json`
   - `Gridiron.WebApi/SeedData/Colleges.json`

---

## Current Migrations

Your database has these migrations (in order):

1. **`20251119154355_InitialCreate`** - Initial Teams, Players, Games schema
2. **`20251120222800_AddPlayerGenerationTables`** - Adds FirstNames, LastNames, Colleges
3. **`20251121160335_AddLeagueStructure`** - Adds Leagues, Conferences, Divisions

All three must be applied for the roster population feature to work.

---

## Quick Reference

| Task | Command |
|------|---------|
| Fresh start (drops DB) | `.\reset-database.ps1 -UseUserSecrets` |
| Apply new migrations | `.\deploy-database.ps1 -UseUserSecrets` |
| Check migrations | `cd DataAccessLayer && dotnet ef migrations list` |
| Drop database | `cd DataAccessLayer && dotnet ef database drop --force` |
| Seed data only | `cd DataAccessLayer && dotnet run` |
| Create migration | `cd DataAccessLayer && dotnet ef migrations add <Name>` |

---

## Best Practices

1. **Use `deploy-database.ps1` for routine updates** - It's safe and preserves data
2. **Use `reset-database.ps1` sparingly** - Only when you need a clean slate
3. **Always backup production data** before running any migration scripts
4. **Use User Secrets for local development** - Never commit connection strings to git
5. **Test migrations in dev first** before applying to production
6. **Review migration code** before applying (`DataAccessLayer/Migrations/*.cs`)

---

## Script Comparison

| Feature | `reset-database.ps1` | `deploy-database.ps1` |
|---------|---------------------|----------------------|
| Drops database | ✅ Yes | ❌ No |
| Creates tables | ✅ Yes | ✅ Yes |
| Applies migrations | ✅ Yes | ✅ Yes |
| Seeds data | ✅ Yes | ✅ Yes (optional) |
| Preserves data | ❌ No | ✅ Yes |
| Safe for production | ⚠️ No | ✅ Yes |
| Requires confirmation | ✅ Yes (unless -Force) | ❌ No |
