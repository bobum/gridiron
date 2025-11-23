import { http, HttpResponse } from 'msw'
import type { Team } from '../../types/Team'
import type { Game, SimulateGameResponse } from '../../types/Game'

// Mock data
export const mockTeams: Team[] = [
  {
    id: 1,
    divisionId: 1,
    name: 'Falcons',
    city: 'Atlanta',
    budget: 200000000,
    championships: 0,
    wins: 10,
    losses: 6,
    ties: 0,
    fanSupport: 75,
    chemistry: 80,
  },
  {
    id: 2,
    divisionId: 1,
    name: 'Eagles',
    city: 'Philadelphia',
    budget: 210000000,
    championships: 1,
    wins: 12,
    losses: 4,
    ties: 0,
    fanSupport: 85,
    chemistry: 90,
  },
]

export const mockGame: Game = {
  id: 1,
  homeTeamId: 1,
  awayTeamId: 2,
  homeScore: 24,
  awayScore: 17,
  fieldPosition: 50,
  yardsToGo: 10,
  currentDown: 1,
}

export const mockSimulateResponse: SimulateGameResponse = {
  id: 1,
  homeTeamId: 1,
  awayTeamId: 2,
  homeScore: 24,
  awayScore: 17,
  message: 'Game simulated successfully',
}

// API handlers
export const handlers = [
  // GET /api/teams
  http.get('/api/teams', () => {
    return HttpResponse.json(mockTeams)
  }),

  // GET /api/teams/:id
  http.get('/api/teams/:id', ({ params }) => {
    const team = mockTeams.find(t => t.id === Number(params.id))
    if (!team) {
      return new HttpResponse(null, { status: 404 })
    }
    return HttpResponse.json(team)
  }),

  // POST /api/games/simulate
  http.post('/api/games/simulate', async ({ request }) => {
    const body = await request.json() as { homeTeamId: number; awayTeamId: number }
    return HttpResponse.json({
      ...mockSimulateResponse,
      homeTeamId: body.homeTeamId,
      awayTeamId: body.awayTeamId,
    })
  }),

  // GET /api/games
  http.get('/api/games', () => {
    return HttpResponse.json([mockGame])
  }),
]
