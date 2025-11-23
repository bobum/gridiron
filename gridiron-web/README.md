# Gridiron Football Manager - Web Frontend

React + TypeScript frontend for the Gridiron Football Management game.

## Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **React Router v6** - Client-side routing
- **TanStack Query (React Query)** - Server state management and API caching
- **Axios** - HTTP client
- **Tailwind CSS** - Utility-first CSS framework

## Prerequisites

- Node.js 20.x or higher (managed via NVM recommended)
- npm 10.x or higher
- Gridiron API running on `http://localhost:5000`

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Start the Development Server

```bash
npm run dev
```

The frontend will be available at **http://localhost:3000**

### 3. Start the Backend API

In a separate terminal, from the root `gridiron/` directory:

```bash
cd Gridiron.WebApi
dotnet run
```

The API will be available at **http://localhost:5000**

Swagger UI: **http://localhost:5000/swagger**

## Project Structure

```
gridiron-web/
├── public/                  # Static assets
├── src/
│   ├── api/                 # API client and React Query hooks
│   │   ├── client.ts        # Axios configuration
│   │   ├── teams.ts         # Teams API hooks
│   │   ├── players.ts       # Players API hooks
│   │   ├── games.ts         # Games API hooks
│   │   └── index.ts         # Barrel exports
│   ├── components/          # Reusable UI components
│   │   ├── Layout.tsx       # Main layout with navigation
│   │   ├── Navigation.tsx   # Top navigation bar
│   │   ├── Loading.tsx      # Loading spinner
│   │   ├── ErrorMessage.tsx # Error display component
│   │   └── index.ts
│   ├── pages/               # Route pages
│   │   ├── HomePage.tsx     # Dashboard with API health check
│   │   ├── TeamsPage.tsx    # Teams list view
│   │   ├── GameSimulationPage.tsx # Game simulation interface
│   │   └── index.ts
│   ├── types/               # TypeScript type definitions
│   │   ├── enums.ts         # Enums (Position, Possession, etc.)
│   │   ├── Player.ts        # Player and PlayerStats types
│   │   ├── Team.ts          # Team and TeamStats types
│   │   ├── Game.ts          # Game, Play, and simulation types
│   │   └── index.ts
│   ├── App.tsx              # Main app with routing
│   ├── main.tsx             # Entry point
│   └── index.css            # Tailwind imports and global styles
├── index.html
├── package.json
├── tailwind.config.js       # Tailwind configuration
├── tsconfig.json            # TypeScript configuration
└── vite.config.ts           # Vite configuration (includes API proxy)
```

## Available Scripts

### `npm run dev`
Start the development server with hot reload at http://localhost:3000

### `npm run build`
Build the production-ready application to the `dist/` folder

### `npm run preview`
Preview the production build locally

### `npm run lint`
Run ESLint to check code quality

## Features

### Current (MVP)

- ✅ Home dashboard with API health check
- ✅ Teams list view showing all teams
- ✅ Game simulation interface
- ✅ Real-time API connectivity status
- ✅ Responsive design (mobile, tablet, desktop)
- ✅ TypeScript type safety
- ✅ React Query caching and error handling

### Planned (Phase 2)

- 🔲 Real-time game viewer with play-by-play
- 🔲 Team detail page with 53-player roster
- 🔲 Depth chart editor
- 🔲 Player detail cards
- 🔲 Statistics dashboard
- 🔲 League standings
- 🔲 Season management
- 🔲 Draft system
- 🔲 User authentication

## API Configuration

The frontend connects to the backend API via Vite's proxy configuration.

### Development

API requests to `/api/*` are proxied to `http://localhost:5000` automatically.

Example: `GET /api/teams` → `http://localhost:5000/api/teams`

### Production

Set the `VITE_API_URL` environment variable:

```bash
# .env.production
VITE_API_URL=https://your-api-domain.com/api
```

## Tailwind CSS

### Custom Theme Colors

```css
gridiron-primary   → #1e3a8a (Deep Blue)
gridiron-secondary → #059669 (Green)
gridiron-accent    → #dc2626 (Red)
```

### Custom Component Classes

```css
.btn-primary    → Primary action button
.btn-secondary  → Secondary action button
.card           → Card container with shadow
```

## TypeScript

All API responses are strongly typed using TypeScript interfaces that mirror the C# DomainObjects models.

Example:
```typescript
import { Team, Player, Game } from './types';

const team: Team = {
  id: 1,
  name: 'Falcons',
  city: 'Atlanta',
  wins: 10,
  losses: 6,
  // ... fully typed
};
```

## Troubleshooting

### API Connection Failed

**Error:** "Failed to load teams. Make sure the API is running..."

**Solution:**
1. Verify the API is running: `http://localhost:5000/swagger`
2. Check CORS is enabled in `Gridiron.WebApi/Program.cs` (line 66-74)
3. Ensure Vite proxy is configured in `vite.config.ts`

### Port Already in Use

**Error:** "Port 3000 is already in use"

**Solution:**
Change the port in `vite.config.ts`:
```typescript
server: {
  port: 3001,
  // ...
}
```

### Node/npm Not Found

**Solution:**
Install Node.js via NVM:
```bash
nvm install lts
nvm use lts
node --version
npm --version
```

## Contributing

When adding new API endpoints:

1. Add TypeScript types in `src/types/`
2. Create API functions and React Query hooks in `src/api/`
3. Build UI components in `src/components/`
4. Create pages in `src/pages/`
5. Add routes in `App.tsx`

## License

Part of the Gridiron Football Management Game project.
