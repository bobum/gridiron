# Azure Account Migration Checklist

Use this checklist when deploying Gridiron to a different Azure account.

---

## Prerequisites

- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] EF Core tools installed (`dotnet ef --version`)
- [ ] Git repository cloned or copied to new machine
- [ ] Azure subscription access with permission to create resources

---

## Azure Resources Setup

### 1. Create Azure SQL Database

- [ ] Log into Azure Portal with new account
- [ ] Create new Resource Group (or use existing)
  - Name: `gridiron-rg` (or your preference)
  - Region: Choose closest to you

- [ ] Create Azure SQL Server
  - Server name: `gridiron-sql-[unique]` (must be globally unique)
  - Authentication: **SQL Authentication**
  - Admin login: `sqladmin` (or your preference)
  - Admin password: (strong password, save securely)
  - Location: Same as resource group
  - Allow Azure services: **Yes**

- [ ] Create SQL Database
  - Database name: `gridiron`
  - Server: Select server created above
  - Pricing tier: Basic or Standard (Basic is sufficient for development)
  - Backup: Default settings

### 2. Configure Firewall

- [ ] Add your client IP address to firewall rules
  - Go to SQL Server ? Networking ? Firewall rules
  - Click "Add client IP"
  - Or add rule manually: `My IP` / `[Your IP]` / `[Your IP]`

### 3. Verify Connection

- [ ] Test connection using Azure Data Studio or SSMS
  - Server: `gridiron-sql-[unique].database.windows.net`
  - Database: `gridiron`
  - Authentication: SQL Login
  - Username: `sqladmin`
  - Password: (password you set)

---

## Database Deployment

### 4. Build Connection String

- [ ] Copy this template and fill in your values:
```
Server=tcp:gridiron-sql-[UNIQUE].database.windows.net,1433;Initial Catalog=gridiron;User ID=sqladmin;Password=[YOUR_PASSWORD];Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 5. Run Deployment Script

**Windows (PowerShell):**
```powershell
cd C:\projects\gridiron  # Or wherever you cloned the repo

$connString = "Server=tcp:gridiron-sql-[UNIQUE].database.windows.net,1433;Initial Catalog=gridiron;User ID=sqladmin;Password=[YOUR_PASSWORD];Encrypt=True;"

.\deploy-database.ps1 -ConnectionString $connString
```

**Linux/Mac (Bash):**
```bash
cd /path/to/gridiron

CONN_STRING="Server=tcp:gridiron-sql-[UNIQUE].database.windows.net,1433;Initial Catalog=gridiron;User ID=sqladmin;Password=[YOUR_PASSWORD];Encrypt=True;"

./deploy-database.sh --connection-string "$CONN_STRING"
```

- [ ] Script completes without errors
- [ ] See "? Database schema created successfully"
- [ ] See "? Data seeding completed successfully"

---

## Verification

### 6. Check Tables Exist

- [ ] Connect to database using SQL tool
- [ ] Run query:
```sql
SELECT name FROM sys.tables ORDER BY name;
```
- [ ] Verify 4 tables exist: `Games`, `PlayByPlays`, `Players`, `Teams`

### 7. Check Seed Data

- [ ] Run query:
```sql
SELECT 
    t.City + ' ' + t.Name AS Team,
    COUNT(p.Id) AS PlayerCount
FROM Teams t
LEFT JOIN Players p ON t.Id = p.TeamId
GROUP BY t.City, t.Name;
```
- [ ] Verify output:
  - Atlanta Falcons: 53 players
  - Philadelphia Eagles: 53 players

### 8. Check Foreign Keys

- [ ] Run query:
```sql
SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName;
```
- [ ] Verify foreign keys exist:
  - `Games` ? `Teams` (HomeTeamId, AwayTeamId)
  - `Players` ? `Teams` (TeamId)
  - `PlayByPlays` ? `Games` (GameId)

---

## Application Configuration

### 9. Update Application Settings (if needed)

**If using appsettings.json (not recommended for production):**
- [ ] Edit `Gridiron.WebApi/appsettings.json`
- [ ] Update `ConnectionStrings:GridironDb` with new connection string
- [ ] **Do NOT commit** connection string to Git

**If using User Secrets (recommended):**
- [ ] Connection string already configured by deployment script
- [ ] Verify: `dotnet user-secrets list --project DataAccessLayer`
- [ ] Should see: `ConnectionStrings:GridironDb = Server=...`

**If using Environment Variables:**
- [ ] Set environment variable: `ConnectionStrings__GridironDb`
- [ ] Value: Your connection string
- [ ] Restart IDE/terminal after setting

### 10. Test Application

- [ ] Build solution: `dotnet build`
- [ ] Run console app: `dotnet run --project GridironConsole`
- [ ] Or run Web API: `dotnet run --project Gridiron.WebApi`
- [ ] Verify no database connection errors
- [ ] Try running a game simulation
- [ ] Check that game results save to database

---

## Post-Migration Validation

### 11. Run Integration Tests (if available)

- [ ] `dotnet test UnitTestProject1`
- [ ] Verify all tests pass
- [ ] Check database-specific tests

### 12. Verify Game Simulation

- [ ] Run console app
- [ ] Simulate a game between Falcons and Eagles
- [ ] Check that game saves to `Games` table
- [ ] Check that play-by-play saves to `PlayByPlays` table
- [ ] Query database:
```sql
SELECT TOP 1 
    g.Id,
    ht.Name + ' vs ' + at.Name AS Matchup,
    g.HomeScore,
    g.AwayScore,
    g.RandomSeed
FROM Games g
INNER JOIN Teams ht ON g.HomeTeamId = ht.Id
INNER JOIN Teams at ON g.AwayTeamId = at.Id
ORDER BY g.Id DESC;
```

### 13. Test Web API (if applicable)

- [ ] Run Web API: `dotnet run --project Gridiron.WebApi`
- [ ] Navigate to Swagger UI: `http://localhost:5000/swagger`
- [ ] Test endpoints:
  - [ ] GET `/api/teams` - Returns 2 teams
  - [ ] GET `/api/players` - Returns ~106 players
  - [ ] POST `/api/games/simulate` - Simulates game
  - [ ] GET `/api/games` - Returns simulated games
  - [ ] GET `/api/games/{id}/plays` - Returns play-by-play

---

## Documentation

### 14. Update Documentation (if needed)

- [ ] Update any README with new Azure resource names
- [ ] Document any environment-specific configuration
- [ ] Update team documentation with new Azure SQL details

### 15. Backup Credentials Securely

- [ ] Store connection string in password manager
- [ ] Store SQL admin username/password securely
- [ ] Document Azure resource names for team

---

## Rollback Plan (if migration fails)

### If deployment fails:

1. **Check logs** from deployment script output
2. **Common issues**:
   - Firewall blocking connection ? Add IP to firewall rules
   - Authentication failed ? Verify SQL Auth enabled, correct credentials
   - Database exists but empty ? Rerun script with `-UseUserSecrets`
   - Seeding failed ? Manually run: `cd DataAccessLayer && dotnet run`

3. **Clean slate**:
   - Delete database in Azure Portal
   - Recreate database
   - Rerun deployment script

4. **Keep old environment running**:
   - Don't delete old Azure resources until new environment verified
   - Can switch back by changing connection string

---

## Cost Optimization (Optional)

- [ ] Set up auto-pause for SQL Database if not using continuously
- [ ] Consider Basic tier for development ($5/month)
- [ ] Scale up to Standard/Premium only if needed for performance
- [ ] Set up budget alerts in Azure Cost Management

---

## Security Hardening (Production)

- [ ] Enable Advanced Threat Protection on SQL Server
- [ ] Use Azure Key Vault for connection string storage
- [ ] Implement Managed Identity for Web API ? SQL authentication
- [ ] Enable Transparent Data Encryption (TDE)
- [ ] Configure backup retention policy
- [ ] Enable auditing and diagnostics logging
- [ ] Restrict firewall to specific IP ranges (not 0.0.0.0/0)

---

## Summary

? **Azure SQL Server created**  
? **Database created and configured**  
? **Firewall rules configured**  
? **Schema deployed via migrations**  
? **Seed data populated**  
? **Application tested and verified**  
? **Documentation updated**  

**Migration complete!** The Gridiron database is now running on the new Azure account.

---

## Estimated Time

- Azure resources setup: **10-15 minutes**
- Database deployment: **2-5 minutes**
- Verification: **5-10 minutes**
- **Total: ~20-30 minutes**

---

## Support

If you encounter issues during migration:

1. Check **DATABASE_DEPLOYMENT.md** for detailed troubleshooting
2. Check **DATABASE_QUICK_REFERENCE.md** for common commands
3. Review deployment script output for specific error messages
4. Test connection using Azure Data Studio before running deployment

---

## Maintenance

**After migration, periodically:**

- [ ] Review Azure SQL pricing tier (Basic ? Standard if performance needed)
- [ ] Monitor database size growth
- [ ] Review firewall rules (remove old IPs)
- [ ] Test database backups (Azure handles automatically, but verify restore works)
- [ ] Update EF Core and related packages (`dotnet outdated`)
