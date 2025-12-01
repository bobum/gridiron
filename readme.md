# Gridiron Football Simulation

## Synopsis

A comprehensive NFL style football game simulation engine written in C# .NET 8. Originally created to see if modeling NFL football games with a state machine was possible - it actually works! Now featuring statistical models, comprehensive play types, penalty systems, injury tracking, and database persistence.

## Features

### Core Gameplay
- ?? **State Machine Architecture** - 19 game states with proper transitions using Stateless library
- ?? **5 Complete Play Types** - Run, Pass, Kickoff, Punt, Field Goal
- ?? **Realistic Statistics** - 20+ skill checks with probability-based outcomes
- ?? **50+ NFL Penalties** - Accurate penalty system with acceptance logic
- ?? **Injury System** - Position-specific injury risk and tracking
- ?? **Game Clock Management** - Full 4-quarter timing with halftime and overtime support

### Technical Features
- ?? **Deterministic Testing** - Seeded RNG for reproducible game simulations
- ?? **Database Persistence** - Entity Framework Core with Azure SQL support
- ?? **Comprehensive Logging** - Play-by-play logging and replay capabilities
- ?? **839 Passing Tests** - Extensive test coverage (100% pass rate)
- ?? **C4 Architecture Diagrams** - PlantUML diagram generation

## Project Structure

```
gridiron/
??? DomainObjects/             - Domain models (Players, Teams, Games, Plays)
??? StateLibrary/              - Game simulation engine and state machine
??? DataAccessLayer/           - Entity Framework Core persistence layer
??? Gridiron.WebApi/           - REST API for game simulation and team management
??? gridiron-web/              - React frontend application (Vite + TypeScript)
??? GridironConsole/           - Console application for running simulations
??? GameManagement/            - Player/team builder services
??? UnitTestProject1/          - 839 comprehensive backend tests
??? Gridiron.IntegrationTests/ - API integration tests
??? Diagram/                   - C4 architecture diagram generation
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Azure SQL Database (optional - for persistence)
- GitHub Personal Access Token with `read:packages` scope (for NuGet package access)

### NuGet Package Setup

This project uses the `Gridiron.Engine` NuGet package hosted on GitHub Packages. To restore packages locally:

1. **Create a Personal Access Token (PAT)**
   - Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate a new token with `read:packages` scope
   - Set expiration (recommended: 1 year)

2. **Set Environment Variable**
   ```bash
   # Windows (PowerShell)
   $env:NUGET_AUTH_TOKEN = "your-github-pat-here"

   # Windows (CMD)
   set NUGET_AUTH_TOKEN=your-github-pat-here

   # Linux/Mac
   export NUGET_AUTH_TOKEN=your-github-pat-here
   ```

3. **Restore Packages**
   ```bash
   dotnet restore
   ```

See [`NUGET_AUTHENTICATION.md`](NUGET_AUTHENTICATION.md) for detailed setup instructions.

### Run a Simulation

```bash
# Clone the repository
git clone https://github.com/merciless-creations/gridiron
cd gridiron

# Build the solution
dotnet build

# Run tests to verify everything works
dotnet test

# Run a simulation
cd GridironConsole
dotnet run
```

### Run Frontend Tests

```bash
# Navigate to frontend directory
cd gridiron-web

# Run component and integration tests
npm test

# Run E2E tests (requires API running on localhost:5000)
npm run test:e2e

# Run tests with UI
npm run test:ui
npm run test:e2e:ui
```

## Testing & CI/CD

### Automated Testing Pipeline

All tests run automatically on every pull request via GitHub Actions:

**Component & Integration Tests** (~2-3 minutes)
- 15 frontend tests with Vitest + React Testing Library
- Mock Service Worker (MSW) for API mocking
- No backend dependencies required
- Fast feedback on UI changes

**End-to-End Tests** (~5-8 minutes)
- SQL Server 2022 container spun up automatically
- Database migrations applied
- Test data seeded (2 teams, 106 players)
- 10 Playwright tests against real API
- Full integration testing in GitHub Actions

**Backend Tests**
- 839 MSTest unit tests (100% pass rate)
- Integration tests for API endpoints
- Soft delete cascade operation tests

See [`gridiron-web/TESTING.md`](gridiron-web/TESTING.md) for detailed testing documentation.

## Technology Stack

### Backend
- **Language**: C# 12
- **Framework**: .NET 8
- **State Machine**: Stateless library
- **Database**: Entity Framework Core 8 + Azure SQL
- **Testing**: MSTest (839 tests), Integration Tests
- **Logging**: Microsoft.Extensions.Logging
- **Architecture**: C4 Model with Structurizr

### Frontend
- **Framework**: React 18 + TypeScript
- **Build Tool**: Vite
- **UI**: TailwindCSS
- **Data Fetching**: TanStack Query (React Query)
- **HTTP Client**: Axios
- **Testing**: Vitest + React Testing Library + MSW + Playwright
- **E2E Testing**: Playwright with real API integration

## Project Stats

- **Lines of Code**: 40,800+ C# lines
- **Test Coverage**: 22,200+ lines of test code
- **Files**: 229 C# files
- **Test Pass Rate**: 100% (839/839)
- **Last Updated**: November 2025

## Documentation

### Architecture & Design
- [`GRIDIRON_COMPREHENSIVE_ARCHITECTURE.md`](GRIDIRON_COMPREHENSIVE_ARCHITECTURE.md) - Complete architecture overview
- [`CODEBASE_ANALYSIS.md`](CODEBASE_ANALYSIS.md) - Detailed architectural analysis
- [`ARCHITECTURE_PRINCIPLES.md`](ARCHITECTURE_PRINCIPLES.md) - Core design principles
- [`FIELD_POSITION_SYSTEM.md`](FIELD_POSITION_SYSTEM.md) - Field position coordinate system

### Database & Deployment
- [`DATABASE_SETUP.md`](DATABASE_SETUP.md) - Database configuration and setup guide
- [`DATABASE-MANAGEMENT.md`](DATABASE-MANAGEMENT.md) - Database management procedures
- [`DATABASE_DEPLOYMENT.md`](DATABASE_DEPLOYMENT.md) - Production deployment guide
- [`AZURE_MIGRATION_CHECKLIST.md`](AZURE_MIGRATION_CHECKLIST.md) - Azure migration steps

### Testing
- [`API_TESTING_GUIDE.md`](API_TESTING_GUIDE.md) - Backend API testing guide
- [`gridiron-web/TESTING.md`](gridiron-web/TESTING.md) - Frontend testing guide (Vitest, MSW, Playwright)

### Implementation Guides
- [`GAMEMANAGEMENT_IMPLEMENTATION_GUIDE.md`](GAMEMANAGEMENT_IMPLEMENTATION_GUIDE.md) - Game management services

## Contributing

This is a personal project, but feel free to fork and experiment!

## License

Personal project - all rights reserved.

## What's Next?

- Fine-tune statistical models for more realistic gameplay
- Add more teams to the database
- Build web API for remote simulations
- Implement season/franchise mode
- Add player progression system

---

*"Just to see if I could - I wrote this state engine to model NFL football games. Crazy thing - it actually works!"*

