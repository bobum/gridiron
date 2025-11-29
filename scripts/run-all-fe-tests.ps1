<#
.SYNOPSIS
    Runs ALL frontend tests locally, matching what CI/CD runs.

.DESCRIPTION
    This script runs the complete frontend test suite:
    1. TypeScript build (catches type errors)
    2. Vitest integration tests (93 tests with MSW mocks)
    3. Playwright E2E tests (4 critical journey tests against real API)

    Use this before pushing to ensure CI/CD will pass.

.PARAMETER SkipBuild
    Skip the TypeScript build step (useful for quick re-runs)

.PARAMETER SkipIntegration
    Skip the Vitest integration tests

.PARAMETER SkipE2E
    Skip the Playwright E2E tests

.EXAMPLE
    .\run-all-fe-tests.ps1
    # Runs everything

.EXAMPLE
    .\run-all-fe-tests.ps1 -SkipBuild
    # Skip build, run all tests

.EXAMPLE
    .\run-all-fe-tests.ps1 -SkipIntegration -SkipE2E
    # Just run build to check for type errors
#>

param(
    [switch]$SkipBuild,
    [switch]$SkipIntegration,
    [switch]$SkipE2E
)

$ErrorActionPreference = "Stop"
$script:hasErrors = $false

function Write-Status {
    param([string]$Message, [string]$Type = "Info")
    $timestamp = Get-Date -Format "HH:mm:ss"
    switch ($Type) {
        "Success" { Write-Host "[$timestamp] [OK] $Message" -ForegroundColor Green }
        "Error"   { Write-Host "[$timestamp] [FAIL] $Message" -ForegroundColor Red }
        "Warning" { Write-Host "[$timestamp] [WARN] $Message" -ForegroundColor Yellow }
        "Header"  { Write-Host "`n[$timestamp] === $Message ===" -ForegroundColor Magenta }
        default   { Write-Host "[$timestamp] [INFO] $Message" -ForegroundColor Cyan }
    }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$frontendPath = Join-Path $scriptDir "..\gridiron-web"

Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "  Frontend Test Suite (CI/CD Match)" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Step 1: TypeScript Build
if (-not $SkipBuild) {
    Write-Status "TypeScript Build" "Header"
    Write-Status "Running 'npm run build' to check for type errors..."

    Push-Location $frontendPath
    try {
        & npm run build 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Status "TypeScript build failed! Fix type errors before pushing." "Error"
            $script:hasErrors = $true
        } else {
            Write-Status "TypeScript build passed" "Success"
        }
    }
    finally {
        Pop-Location
    }
} else {
    Write-Status "Skipping TypeScript build (-SkipBuild)" "Warning"
}

# Step 2: Vitest Integration Tests
if (-not $SkipIntegration) {
    Write-Status "Vitest Integration Tests" "Header"
    Write-Status "Running 'npm test' (93 integration tests with MSW)..."

    Push-Location $frontendPath
    try {
        & npm test -- --run 2>&1 | Tee-Object -Variable testOutput | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Status "Vitest integration tests failed!" "Error"
            $script:hasErrors = $true
            # Show last few lines of output for context
            $testOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
        } else {
            # Extract test summary
            $summary = $testOutput | Select-String -Pattern "Tests.*passed"
            if ($summary) {
                Write-Status "Vitest: $($summary.Line.Trim())" "Success"
            } else {
                Write-Status "Vitest integration tests passed" "Success"
            }
        }
    }
    finally {
        Pop-Location
    }
} else {
    Write-Status "Skipping Vitest integration tests (-SkipIntegration)" "Warning"
}

# Step 3: Playwright E2E Tests
if (-not $SkipE2E) {
    Write-Status "Playwright E2E Tests" "Header"
    Write-Status "Running E2E tests (requires API with E2E_TEST_MODE)..."

    # Use the existing run-e2e-local.ps1 script
    $e2eScript = Join-Path $scriptDir "run-e2e-local.ps1"
    if (Test-Path $e2eScript) {
        & powershell -ExecutionPolicy Bypass -File $e2eScript 2>&1 | Tee-Object -Variable e2eOutput
        if ($LASTEXITCODE -ne 0) {
            Write-Status "Playwright E2E tests failed!" "Error"
            $script:hasErrors = $true
        } else {
            Write-Status "Playwright E2E tests passed" "Success"
        }
    } else {
        Write-Status "E2E script not found at: $e2eScript" "Error"
        $script:hasErrors = $true
    }
} else {
    Write-Status "Skipping Playwright E2E tests (-SkipE2E)" "Warning"
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "  Summary" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

if ($script:hasErrors) {
    Write-Status "Some tests FAILED - do not push to CI/CD!" "Error"
    exit 1
} else {
    Write-Status "All tests PASSED - safe to push!" "Success"
    exit 0
}
