# ============================================================
# Gridiron Database Reset Script
# ============================================================
# This script will:
# 1. DROP the entire database (destructive!)
# 2. Recreate all tables by applying EF Core migrations
# 3. Seed all initial data (FirstNames, LastNames, Colleges, Teams, Players)
# ============================================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString,

    [Parameter(Mandatory=$false)]
    [switch]$UseUserSecrets,

    [Parameter(Mandatory=$false)]
    [switch]$SkipSeedData,

    [Parameter(Mandatory=$false)]
    [switch]$Force  # Skip confirmation prompt
)

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColorOutput($Message, $Color) {
    Write-Host $Message -ForegroundColor $Color
}

function Write-Section($Title) {
    Write-Host ""
    Write-ColorOutput "============================================================" $InfoColor
    Write-ColorOutput "  $Title" $InfoColor
    Write-ColorOutput "============================================================" $InfoColor
    Write-Host ""
}

# Navigate to DataAccessLayer
$DataAccessLayerPath = Join-Path $PSScriptRoot "DataAccessLayer"

if (-not (Test-Path $DataAccessLayerPath)) {
    Write-ColorOutput "ERROR: DataAccessLayer directory not found at $DataAccessLayerPath" $ErrorColor
    exit 1
}

# ============================================================
# DANGER WARNING
# ============================================================
Write-Host ""
Write-ColorOutput "============================================================" $WarningColor
Write-ColorOutput "                    WARNING - DESTRUCTIVE OPERATION         " $WarningColor
Write-ColorOutput "============================================================" $WarningColor
Write-Host ""
Write-ColorOutput "This script will COMPLETELY DROP your database!" $WarningColor
Write-ColorOutput "ALL data will be permanently deleted!" $WarningColor
Write-Host ""
Write-Host "Use this script to:"
Write-Host "  - Get a fresh database with clean schema"
Write-Host "  - Reset to initial seed data"
Write-Host "  - Fix migration/schema issues"
Write-Host ""

if (-not $Force) {
    Write-Host "Are you ABSOLUTELY SURE you want to drop the database? (type 'yes' to confirm): " -NoNewline
    $confirmation = Read-Host

    if ($confirmation -ne "yes") {
        Write-ColorOutput "Database reset cancelled." $InfoColor
        exit 0
    }
}

Set-Location $DataAccessLayerPath

# ============================================================
# Step 1: Configure Connection String
# ============================================================
Write-Section "Step 1: Configuring Connection String"

if ($UseUserSecrets) {
    Write-ColorOutput "Using User Secrets for connection string..." $InfoColor
    Write-ColorOutput "? User Secrets ID: gridiron-football-sim-2024" $SuccessColor
}
elseif ($ConnectionString) {
    Write-ColorOutput "Setting connection string in User Secrets..." $InfoColor
    dotnet user-secrets set "ConnectionStrings:GridironDb" $ConnectionString
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "ERROR: Failed to set connection string in User Secrets" $ErrorColor
        exit 1
    }
    Write-ColorOutput "? Connection string configured" $SuccessColor
}
else {
    Write-ColorOutput "ERROR: No connection string provided!" $ErrorColor
    Write-Host ""
    Write-Host "Please provide connection string using one of these methods:"
    Write-Host "  1. Run with -ConnectionString parameter:"
    Write-Host "     .\reset-database.ps1 -ConnectionString `"Server=...`""
    Write-Host ""
    Write-Host "  2. Or manually set it first, then run with -UseUserSecrets:"
    Write-Host "     cd DataAccessLayer"
    Write-Host "     dotnet user-secrets set `"ConnectionStrings:GridironDb`" `"Server=...`""
    Write-Host "     cd .."
    Write-Host "     .\reset-database.ps1 -UseUserSecrets"
    Write-Host ""
    exit 1
}

# ============================================================
# Step 2: Drop Database
# ============================================================
Write-Section "Step 2: Dropping Existing Database"

Write-ColorOutput "Running: dotnet ef database drop --force" $WarningColor
dotnet ef database drop --force

if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "WARNING: Database drop failed (database may not exist)" $WarningColor
    Write-Host "Continuing with migration..."
}
else {
    Write-ColorOutput "? Database dropped successfully" $SuccessColor
}

# ============================================================
# Step 3: Apply All Migrations (Recreate Schema)
# ============================================================
Write-Section "Step 3: Creating Database Schema"

Write-ColorOutput "Running: dotnet ef database update" $InfoColor
dotnet ef database update

if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "ERROR: Migration failed!" $ErrorColor
    Write-Host ""
    Write-Host "Common issues:"
    Write-Host "  - Connection string is incorrect"
    Write-Host "  - Azure SQL firewall blocks your IP address"
    Write-Host "  - SQL authentication is not enabled"
    Write-Host "  - Database credentials are wrong"
    Write-Host ""
    exit 1
}

Write-ColorOutput "? Database schema created successfully" $SuccessColor

# List applied migrations
Write-Host ""
Write-ColorOutput "Applied migrations:" $InfoColor
dotnet ef migrations list

# ============================================================
# Step 4: Seed Initial Data
# ============================================================
if (-not $SkipSeedData) {
    Write-Section "Step 4: Seeding Initial Data"

    Write-ColorOutput "This will seed:" $InfoColor
    Write-Host "  - FirstNames, LastNames, Colleges (player generation data)"
    Write-Host "  - Teams (Atlanta Falcons, Philadelphia Eagles)"
    Write-Host "  - Players (~53 per team)"
    Write-Host ""

    Write-ColorOutput "Running: dotnet run --project ." $InfoColor
    dotnet run --project .

    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "ERROR: Data seeding failed!" $ErrorColor
        Write-Host ""
        Write-Host "Database schema was created successfully, but seeding failed."
        Write-Host "You can manually run seeding later by executing:"
        Write-Host "  cd DataAccessLayer"
        Write-Host "  dotnet run"
        Write-Host ""
        exit 1
    }

    Write-ColorOutput "? Data seeding completed successfully" $SuccessColor
}
else {
    Write-ColorOutput "Skipping data seeding (-SkipSeedData flag provided)" $WarningColor
}

# ============================================================
# Summary
# ============================================================
Write-Section "Database Reset Complete!"

Write-ColorOutput "? Old database dropped" $SuccessColor
Write-ColorOutput "? Fresh schema created" $SuccessColor
Write-ColorOutput "? All migrations applied" $SuccessColor
if (-not $SkipSeedData) {
    Write-ColorOutput "? Initial data seeded" $SuccessColor
}
Write-ColorOutput "? Database is ready to use" $SuccessColor

Write-Host ""
Write-ColorOutput "Database Contents:" $InfoColor
Write-Host "  Tables Created:"
Write-Host "      - Teams, Players"
Write-Host "      - FirstNames, LastNames, Colleges"
Write-Host "      - Games, Conferences, Divisions, Leagues"
if (-not $SkipSeedData) {
    Write-Host ""
    Write-Host "  Seed Data Loaded:"
    Write-Host "      - ~150 first names"
    Write-Host "      - ~100 last names"
    Write-Host "      - ~130 colleges"
    Write-Host "      - 2 teams (Falcons, Eagles)"
    Write-Host "      - ~106 players total"
}

Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Run your Web API: dotnet run --project Gridiron.WebApi"
Write-Host "  2. Test endpoints: swagger at https://localhost:7000/swagger"
Write-Host "  3. Populate more teams: POST /api/leagues-management with league structure"
Write-Host "  4. Generate rosters: POST /api/teams-management/{id}/populate-roster"
Write-Host ""

# Return to original directory
Set-Location $PSScriptRoot
