# PowerShell script to bulk update test files
Write-Host "Updating test files to use TestTeams helper..." -ForegroundColor Cyan

$files = @(
    'UnitTestProject1\PassPlaySkillsChecksTests.cs',
    'UnitTestProject1\PassResultTests.cs',
    'UnitTestProject1\RunResultTests.cs',
    'UnitTestProject1\PuntResultTests.cs',
    'UnitTestProject1\PuntPlayExecutionTests.cs',
    'UnitTestProject1\ScoringIntegrationTests.cs',
    'UnitTestProject1\RedZoneTests.cs',
    'UnitTestProject1\ThirdDownConversionTests.cs',
    'UnitTestProject1\DownProgressionTests.cs',
    'UnitTestProject1\PenaltyAssignmentTests.cs',
    'UnitTestProject1\PlayArchitectureTests.cs',
    'UnitTestProject1\InjurySystemTests.cs',
    'UnitTestProject1\InterceptionTests.cs',
    'UnitTestProject1\GoalLineTests.cs',
    'UnitTestProject1\SafetyFreeKickTests.cs',
    'UnitTestProject1\MultiplePenaltyTests.cs',
    'UnitTestProject1\RunPlaySkillsChecksTests.cs'
)

$pattern = 'private readonly Teams _teams = new Teams\(\);'
$replacement = 'private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();'

$updated = 0
foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -match $pattern) {
 $content = $content -replace $pattern, $replacement
     Set-Content -Path $file -Value $content -NoNewline
  Write-Host "  ? $file" -ForegroundColor Green
            $updated++
        }
    }
}

Write-Host "`nUpdated $updated files" -ForegroundColor Cyan
