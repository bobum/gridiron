<#
.SYNOPSIS
    Runs E2E tests locally with full environment setup.

.DESCRIPTION
    This script sets up the complete local environment for running E2E tests:
    1. Drops and recreates the database (fresh start like CI/CD)
    2. Applies all EF Core migrations
    3. Seeds test data
    4. Kills any existing processes on ports 5000 (API) and 3000 (frontend)
    5. Starts the API with E2E_TEST_MODE enabled (bypasses authentication)
    6. Waits for the API to be healthy
    7. Runs Playwright E2E tests with fail-fast behavior
    8. Cleans up processes when done

.PARAMETER SkipApiStart
    If specified, assumes the API is already running with E2E_TEST_MODE=true

.PARAMETER SkipDbReset
    If specified, skips database drop/recreate/seed (useful for quick re-runs)

.PARAMETER TestFilter
    Optional filter to run specific tests (e.g., "leagues" or "game-simulation")

.EXAMPLE
    .\run-e2e-local.ps1

.EXAMPLE
    .\run-e2e-local.ps1 -TestFilter "leagues"

.EXAMPLE
    .\run-e2e-local.ps1 -SkipApiStart

.EXAMPLE
    .\run-e2e-local.ps1 -SkipDbReset -TestFilter "leagues"
#>

param(
    [switch]$SkipApiStart,
    [switch]$SkipDbReset,
    [string]$TestFilter = ""
)

$ErrorActionPreference = "Stop"

# Configuration - all E2E settings in one place
$config = @{
    ApiPort = 5000
    FrontendPort = 3000
    ApiUrl = "http://localhost:5000"
    ApiHealthEndpoint = "http://localhost:5000/api/teams"
    ApiProjectPath = Join-Path $PSScriptRoot "..\Gridiron.WebApi"
    DataAccessLayerPath = Join-Path $PSScriptRoot "..\DataAccessLayer"
    FrontendPath = Join-Path $PSScriptRoot "..\gridiron-web"
    MaxApiWaitSeconds = 60
    HealthCheckIntervalSeconds = 2
}

function Write-Status {
    param([string]$Message, [string]$Type = "Info")
    $timestamp = Get-Date -Format "HH:mm:ss"
    switch ($Type) {
        "Success" { Write-Host "[$timestamp] [OK] $Message" -ForegroundColor Green }
        "Error"   { Write-Host "[$timestamp] [ERROR] $Message" -ForegroundColor Red }
        "Warning" { Write-Host "[$timestamp] [WARN] $Message" -ForegroundColor Yellow }
        default   { Write-Host "[$timestamp] [INFO] $Message" -ForegroundColor Cyan }
    }
}

function Stop-ProcessOnPort {
    param([int]$Port)

    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($connections) {
        $processIds = $connections | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($procId in $processIds) {
            $process = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Status "Stopping $($process.ProcessName) (PID: $procId) on port $Port" "Warning"
                Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
            }
        }
        Start-Sleep -Seconds 1
    }
}

function Wait-ForApi {
    $elapsed = 0
    Write-Status "Waiting for API to be healthy..."

    while ($elapsed -lt $config.MaxApiWaitSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $config.ApiHealthEndpoint -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Status "API is healthy and responding" "Success"
                return $true
            }
        }
        catch {
            # API not ready yet, continue waiting
        }

        Start-Sleep -Seconds $config.HealthCheckIntervalSeconds
        $elapsed += $config.HealthCheckIntervalSeconds
        Write-Host "." -NoNewline
    }

    Write-Host ""
    Write-Status "API failed to become healthy within $($config.MaxApiWaitSeconds) seconds" "Error"
    return $false
}

function Reset-Database {
    Write-Status "Resetting database (drop, migrate, seed)..."

    Push-Location $config.DataAccessLayerPath
    try {
        # Step 1: Drop database (suppress output, only show errors)
        Write-Status "Dropping existing database..."
        $null = & dotnet ef database drop --force 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Status "Database may not have existed (continuing)" "Warning"
        } else {
            Write-Status "Database dropped" "Success"
        }

        # Step 2: Apply migrations (suppress info output)
        Write-Status "Applying EF Core migrations..."
        $null = & dotnet ef database update --startup-project ../Gridiron.WebApi 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Database migration failed"
        }
        Write-Status "Migrations applied" "Success"
    }
    finally {
        Pop-Location
    }

    # Step 3: Seed test data (from API project, like CI/CD does)
    Push-Location $config.ApiProjectPath
    try {
        Write-Status "Seeding test data..."
        $null = & dotnet run -- --seed --force 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Database seeding failed"
        }
        Write-Status "Test data seeded" "Success"
    }
    finally {
        Pop-Location
    }
}

function Start-ApiWithE2EMode {
    Write-Status "Starting API with E2E_TEST_MODE=true..."

    # Set environment variables in the current session - they will be inherited by child processes
    $env:E2E_TEST_MODE = "true"
    $env:ASPNETCORE_URLS = $config.ApiUrl

    Push-Location $config.ApiProjectPath
    try {
        # Start API in background - use cmd.exe wrapper to ensure env vars are inherited
        $apiProcess = Start-Process -FilePath "cmd.exe" -ArgumentList "/c", "dotnet run" -PassThru -WindowStyle Hidden
        Write-Status "API started with PID: $($apiProcess.Id)"
        return $apiProcess
    }
    finally {
        Pop-Location
    }
}

function Invoke-E2ETests {
    param([string]$Filter)

    Push-Location $config.FrontendPath
    try {
        if ($Filter) {
            Write-Status "Running E2E tests with filter: $Filter"
            & npx playwright test --config=playwright.local.config.ts -x --grep "$Filter"
        }
        else {
            Write-Status "Running all E2E tests"
            & npx playwright test --config=playwright.local.config.ts -x
        }
        return $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
}

# Main execution
Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "  Gridiron E2E Local Test Runner" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

$apiProcess = $null

try {
    # Step 1: Clean up existing processes
    Write-Status "Cleaning up existing processes..."
    Stop-ProcessOnPort -Port $config.ApiPort
    Stop-ProcessOnPort -Port $config.FrontendPort

    # Step 2: Reset database (unless skipped)
    if (-not $SkipDbReset) {
        Reset-Database
    }
    else {
        Write-Status "Skipping database reset (-SkipDbReset flag provided)" "Warning"
    }

    # Step 3: Start API (unless skipped)
    if (-not $SkipApiStart) {
        $apiProcess = Start-ApiWithE2EMode

        # Step 4: Wait for API to be healthy
        if (-not (Wait-ForApi)) {
            throw "API failed to start"
        }
    }
    else {
        Write-Status "Skipping API start (assuming it's already running with E2E_TEST_MODE=true)" "Warning"

        # Verify API is actually running
        try {
            $response = Invoke-WebRequest -Uri $config.ApiHealthEndpoint -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            Write-Status "API is running and accessible" "Success"
        }
        catch {
            Write-Status "API is not accessible at $($config.ApiHealthEndpoint). Make sure to start it with E2E_TEST_MODE=true" "Error"
            throw "API not accessible"
        }
    }

    # Step 5: Run E2E tests
    Write-Host ""
    Write-Status "Starting E2E tests..."
    Write-Host ""

    $exitCode = Invoke-E2ETests -Filter $TestFilter

    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Status "All tests passed!" "Success"
    }
    else {
        Write-Host ""
        Write-Status "Some tests failed (exit code: $exitCode)" "Error"
    }

    exit $exitCode
}
catch {
    Write-Status $_.Exception.Message "Error"
    exit 1
}
finally {
    # Step 5: Cleanup
    if ($apiProcess -and -not $apiProcess.HasExited) {
        Write-Status "Stopping API process..."
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
}
