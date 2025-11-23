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
??? DomainObjects/          - Domain models (Players, Teams, Games, Plays)
??? StateLibrary/           - Game simulation engine and state machine
??? DataAccessLayer/        - Entity Framework Core persistence layer
??? GridironConsole/        - Console application for running simulations
??? UnitTestProject1/       - 839 comprehensive tests
??? Diagram/                - C4 architecture diagram generation
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Azure SQL Database (optional - for persistence)

### Run a Simulation

```bash
# Clone the repository
git clone https://github.com/bobum/gridiron
cd gridiron

# Build the solution
dotnet build

# Run tests to verify everything works
dotnet test

# Run a simulation
cd GridironConsole
dotnet run
```

## Technology Stack

- **Language**: C# 12
- **Framework**: .NET 8
- **State Machine**: Stateless library
- **Database**: Entity Framework Core 8 + Azure SQL
- **Testing**: MSTest (839 tests)
- **Logging**: Microsoft.Extensions.Logging
- **Architecture**: C4 Model with Structurizr

## Project Stats

- **Lines of Code**: 40,800+ C# lines
- **Test Coverage**: 22,200+ lines of test code
- **Files**: 229 C# files
- **Test Pass Rate**: 100% (839/839)
- **Last Updated**: November 2025

## Documentation

- [`ARCHITECTURE_SUMMARY.txt`](ARCHITECTURE_SUMMARY.txt) - Quick reference for system architecture
- [`CODEBASE_ANALYSIS.md`](CODEBASE_ANALYSIS.md) - Detailed architectural analysis
- [`DATABASE_SETUP.md`](DATABASE_SETUP.md) - Database configuration and setup guide
- [`FIELD_POSITION_SYSTEM.md`](FIELD_POSITION_SYSTEM.md) - Field position coordinate system

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
