# ============================================================
# Gridiron Database Deployment Script
# ============================================================
# This script will:
# 1. Apply EF Core migrations to create database schema
# 2. Run seed data to populate teams and players
# ============================================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseUserSecrets,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSeedData
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

Set-Location $DataAccessLayerPath

# ============================================================
# Step 1: Configure Connection String
# ============================================================
Write-Section "Step 1: Configuring Connection String"

if ($UseUserSecrets) {
    Write-ColorOutput "Using User Secrets for connection string..." $InfoColor
    # User secrets are already configured, nothing to do
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
    Write-ColorOutput "WARNING: No connection string provided!" $WarningColor
    Write-Host ""
    Write-Host "Please provide connection string using one of these methods:"
    Write-Host "  1. Run with -ConnectionString parameter:"
    Write-Host "     .\deploy-database.ps1 -ConnectionString `"Server=...`""
    Write-Host ""
    Write-Host "  2. Or manually set it first, then run with -UseUserSecrets:"
    Write-Host "     cd DataAccessLayer"
    Write-Host "     dotnet user-secrets set `"ConnectionStrings:GridironDb`" `"Server=...`""
    Write-Host "     cd .."
    Write-Host "     .\deploy-database.ps1 -UseUserSecrets"
    Write-Host ""
    exit 1
}

# ============================================================
# Step 2: Apply EF Core Migrations
# ============================================================
Write-Section "Step 2: Applying Database Migrations"

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

# ============================================================
# Step 3: Seed Data (Optional)
# ============================================================
if (-not $SkipSeedData) {
    Write-Section "Step 3: Seeding Initial Data"
    
    Write-ColorOutput "Running: dotnet run --project . -- seed" $InfoColor
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
    Write-ColorOutput "Skipping data seeding (--SkipSeedData flag provided)" $WarningColor
}

# ============================================================
# Summary
# ============================================================
Write-Section "Deployment Complete!"

Write-ColorOutput "? Database schema created" $SuccessColor
if (-not $SkipSeedData) {
    Write-ColorOutput "? Initial data seeded (Falcons and Eagles)" $SuccessColor
}
Write-ColorOutput "? Database is ready to use" $SuccessColor

Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Test the database connection in Visual Studio"
Write-Host "  2. Run your application: dotnet run --project GridironConsole"
Write-Host "  3. Verify teams and players in database using SQL Server extension"
Write-Host ""

# Return to original directory
Set-Location $PSScriptRoot
