# Gridiron.WebApi.Tests

Unit tests for the Gridiron Football Simulation REST API.

## Overview

This test project contains **unit tests only** - no database integration tests. All tests use mocked repositories to verify controller and service logic without touching the database.

## Key Characteristics

- ✅ **Fast** - Tests run in milliseconds (no database I/O)
- ✅ **Isolated** - Each test is independent
- ✅ **Deterministic** - Same inputs always produce same outputs
- ✅ **No Database** - All data access is mocked using Moq
- ✅ **Separate from Domain Tests** - Runs independently from the 800+ domain tests

## Test Structure

```
Gridiron.WebApi.Tests/
├── Controllers/
│   ├── TeamsControllerTests.cs       (17 tests)
│   └── PlayersControllerTests.cs     (12 tests)
├── Services/
│   └── GameSimulationServiceTests.cs (13 tests)
└── README.md
```

## What is Tested

### TeamsController (17 tests)
- `GET /api/teams` - List all teams
- `GET /api/teams/{id}` - Get specific team
- `GET /api/teams/{id}/roster` - Get team with players
- DTO mapping correctness
- 404 handling
- Repository method calls

### PlayersController (12 tests)
- `GET /api/players` - List all players
- `GET /api/players?teamId={id}` - Filter by team
- `GET /api/players/{id}` - Get specific player
- Stats mapping (game, season, career)
- DTO mapping correctness
- Null handling

### GameSimulationService (13 tests)
- Team loading through repositories
- Error handling (team not found)
- Game simulation execution
- Deterministic simulation (same seed = same result)
- Game persistence through repository
- Get game / get all games

## Running the Tests

### Run All API Tests

```bash
cd Gridiron.WebApi.Tests
dotnet test
```

### Run Tests with Detailed Output

```bash
dotnet test --verbosity normal
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~TeamsControllerTests"
dotnet test --filter "FullyQualifiedName~PlayersControllerTests"
dotnet test --filter "FullyQualifiedName~GameSimulationServiceTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~GetTeams_WhenTeamsExist_ReturnsOkWithTeams"
```

### Generate Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Running API Tests Separately from Domain Tests

### Option 1: Target Specific Project

```bash
# Run ONLY API tests (this project)
dotnet test Gridiron.WebApi.Tests/Gridiron.WebApi.Tests.csproj

# Run ONLY domain tests (the 800+ existing tests)
dotnet test UnitTestProject1/UnitTestProject1.csproj
```

### Option 2: Use Test Filters

```bash
# Run all tests in the Gridiron.WebApi.Tests namespace
dotnet test --filter "FullyQualifiedName~Gridiron.WebApi.Tests"

# Run all domain tests (exclude API tests)
dotnet test --filter "FullyQualifiedName!~Gridiron.WebApi.Tests"
```

### Option 3: Run from Specific Directory

```bash
# Run only API tests
cd Gridiron.WebApi.Tests
dotnet test

# Run only domain tests
cd UnitTestProject1
dotnet test
```

## Test Dependencies

- **xUnit** - Testing framework
- **Moq** - Mocking framework for creating fake repositories
- **FluentAssertions** - Readable assertion syntax
- **Microsoft.NET.Test.Sdk** - Test runner

## Why Separate from Domain Tests?

1. **Different Concerns** - API tests verify HTTP endpoints, domain tests verify business logic
2. **Different Speed** - API tests are fast (mocked), domain tests may include slower scenarios
3. **Independent Execution** - Can run API tests during API development without running all 800+ domain tests
4. **Clear Separation** - Easy to see API coverage vs domain coverage
5. **CI/CD Flexibility** - Can run different test suites in different pipeline stages

## Test Patterns Used

### Arrange-Act-Assert

```csharp
[Fact]
public async Task GetTeam_WhenTeamExists_ReturnsOkWithTeam()
{
    // Arrange - Set up mocks and test data
    var team = CreateTestTeam(1, "Falcons", "Atlanta");
    _mockTeamRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(team);

    // Act - Call the method being tested
    var result = await _controller.GetTeam(1);

    // Assert - Verify the result
    result.Result.Should().BeOfType<OkObjectResult>();
}
```

### Mocking Repositories

```csharp
// Create mock
var mockTeamRepo = new Mock<ITeamRepository>();

// Setup what it should return
mockTeamRepo
    .Setup(repo => repo.GetByIdAsync(1))
    .ReturnsAsync(new Team { Id = 1, Name = "Falcons" });

// Inject into controller
var controller = new TeamsController(mockTeamRepo.Object, mockLogger.Object);

// Verify it was called
mockTeamRepo.Verify(repo => repo.GetByIdAsync(1), Times.Once);
```

## Code Coverage Goals

- **Controllers:** 90%+ (thin layer, mostly routing)
- **Services:** 85%+ (business logic)
- **Overall:** 85%+

Current coverage:
- TeamsController: ~95%
- PlayersController: ~90%
- GameSimulationService: ~85%

## What These Tests DO NOT Cover

### ❌ Not Tested Here (Use Integration Tests Instead)

- Database queries (EF Core, SQL)
- Repository implementations
- Database migrations
- Connection string handling
- Real HTTP requests
- End-to-end workflows

### ✅ What IS Tested Here

- Controller routing logic
- DTO mapping
- Service business logic
- Error handling
- Repository method calls (mocked)
- HTTP status codes

## Adding New Tests

### When to Add a Test

Add a test when you:
1. Add a new API endpoint
2. Add new business logic to a service
3. Fix a bug (write a failing test first)
4. Add error handling
5. Add new DTO mappings

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Example: GetTeam_WhenTeamExists_ReturnsOkResult
}
```

### Test Organization

- Group related tests with `#region`
- One test class per controller/service
- Helper methods at the bottom
- Keep tests focused (one assertion per test ideally)

## Continuous Integration

These tests should run:
- ✅ On every pull request
- ✅ Before merging to main
- ✅ As part of CI/CD pipeline
- ✅ Before deploying to any environment

Example CI command:
```bash
dotnet test Gridiron.WebApi.Tests --logger "trx" --results-directory ./TestResults
```

## Troubleshooting

### Tests Fail with "Repository not found"

Make sure all repositories are mocked in the test constructor:
```csharp
_mockTeamRepository = new Mock<ITeamRepository>();
_controller = new TeamsController(_mockTeamRepository.Object, ...);
```

### Tests Timeout

GameSimulationService tests run actual simulations. They may take 5-10 seconds each. This is expected.

### FluentAssertions Syntax Errors

Make sure you have:
```csharp
using FluentAssertions;
```

### Moq Setup Not Working

Verify the method signature matches exactly:
```csharp
// Correct
.Setup(repo => repo.GetByIdAsync(1))

// Wrong (parameter type mismatch)
.Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))  // Should be int
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ARCHITECTURE_PRINCIPLES.md](../ARCHITECTURE_PRINCIPLES.md) - Repository pattern rules
- [API_TESTING_GUIDE.md](../API_TESTING_GUIDE.md) - Manual API testing

## Summary

These tests verify that the API layer works correctly by:
1. Mocking all database access through repositories
2. Testing controllers return correct HTTP responses
3. Testing services orchestrate business logic correctly
4. Ensuring DTOs map domain objects properly
5. Verifying error handling works as expected

All without touching the database or running the full domain test suite.
