# Gridiron

Gridiron is a web-based NFL franchise management simulation game. Think Out of the Park Baseball, but for football and 100% browser/mobile accessible.

## Vision

A deep, authentic NFL front office experience where players act as General Managers—drafting, trading, signing, and building rosters across multiple seasons. The simulation runs play-by-play with a state machine engine. Multiplayer leagues allow multiple human GMs competing in the same league.

## Repositories

- **gridiron-engine**: https://github.com/merciless-creations/gridiron-engine — Simulation engine, state machine (NuGet package)
- **gridiron**: https://github.com/merciless-creations/gridiron — C# Web API, data access, game management services
- **gridiron-web**: https://github.com/merciless-creations/gridiron-web — React frontend

## Project

- https://github.com/orgs/merciless-creations/projects/1

## Tech Stack

| Layer | Technology |
|-------|------------|
| Database | Azure SQL |
| Backend | C# |
| Frontend | React, Vite, TailwindCSS |
| Hosting | Azure |

## Core Gameplay Loop

1. **Manage** — Roster moves, contracts, trades, draft prep, coaching/scheme adjustments
2. **Simulate** — Advance the game clock; the engine processes play-by-play outcomes
3. **Results** — Review box scores, standings, stats, injuries, progression

No real-time gameplay. All interaction is asynchronous management followed by simulation advancement.

## Simulation Engine

The simulation core is a state machine built by Scott. It processes play-by-play football logic: down/distance, play calling, execution, injuries, clock management, scoring. Claude has written the surrounding systems under Scott's direction.

## League Structure

- NFL-based structure (conferences, divisions, schedule, playoffs, draft, free agency, salary cap)
- Variable league sizes (not locked to 32 teams)
- Multiple human GMs per league
- Fictional generated players (no real NFL data)

## Monetization

Monthly subscription model.

## Data Model Concepts

- **League** — Container for teams, schedule, settings
- **Team** — Franchise with roster, cap space, draft picks
- **Player** — Generated fictional players with attributes, contracts, progression
- **Contract** — Salary, years, guarantees, cap implications
- **Draft** — Annual draft class, pick trading, scouting
- **Game** — Scheduled matchup, simulated play-by-play, box score
- **Season** — Schedule, standings, playoffs, offseason phases

## Development Context

This project is mature. Scott directs architecture and writes critical systems (like the state machine). Claude writes implementation code under Scott's guidance. Respect existing patterns in the codebase.

## Coding Conventions

### Backend (C#)
- Follow existing patterns in gridiron-engine
- Entity Framework for data access
- RESTful API design
- Clear separation: Controllers → Services → Repositories

### Frontend (React)
- Functional components with hooks
- TailwindCSS for styling (no separate CSS files unless necessary)
- Vite for build tooling
- Component files colocated with their concerns

## Git Workflow

> ⛔ **ABSOLUTE RULE: NEVER COMMIT OR PUSH DIRECTLY TO MASTER OR MAIN** ⛔
>
> This is non-negotiable. Violations break CI/CD and require manual cleanup.

### Required Process for ALL Changes

1. **Create a feature branch** from master:
   ```bash
   git checkout master
   git pull
   git checkout -b feature/your-change-description
   ```

2. **Make changes and commit** to the feature branch:
   ```bash
   git add .
   git commit -m "Description of change"
   ```

3. **Push the feature branch** to origin:
   ```bash
   git push -u origin feature/your-change-description
   ```

4. **Create a Pull Request** for Scott to review

5. **Wait for approval** — Scott will merge after CI passes

### Branch Naming
- `feature/` — New features or enhancements
- `fix/` — Bug fixes
- `chore/` — Maintenance, refactoring, docs

This applies to ALL changes, no matter how small—even single-line fixes.

## When Uncertain

Ask Scott. He is precise in his requirements—do not assume or estimate. If a requirement is ambiguous, clarify before implementing.

## Interaction Protocol
- **WAIT FOR EXPLICIT APPROVAL**: Do not write code or implement changes until the user explicitly says "Go ahead", "Proceed", or "Green light".
- **PLAN FIRST**: When presented with a task, analyze the architecture and propose a plan in the issue or chat.
- **DOCUMENT BEFORE CODING**: Ensure the "HOW" is documented and agreed upon before writing the "WHAT".
- **ONE STEP AT A TIME**: Do not chain multiple large tasks or assumptions together without checking in.
