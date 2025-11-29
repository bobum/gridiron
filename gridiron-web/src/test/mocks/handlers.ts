import { http, HttpResponse } from 'msw'
import type { Team } from '../../types/Team'
import type { Game, SimulateGameResponse } from '../../types/Game'
import type { League, LeagueDetail, CreateLeagueRequest } from '../../types/League'
import type { User } from '../../types/User'

// Mock data - Teams
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

// Mock data - User
export const mockUser: User = {
  id: 1,
  email: 'testuser@example.com',
  displayName: 'Test User',
  isGlobalAdmin: false,
  createdAt: '2024-01-15T10:00:00Z',
  lastLoginAt: '2024-11-29T08:30:00Z',
  leagueRoles: [
    {
      id: 1,
      leagueId: 1,
      leagueName: 'Test League',
      role: 'Commissioner',
      teamId: null,
      teamName: null,
      assignedAt: '2024-01-15T10:00:00Z',
    },
  ],
}

// Mock data - Leagues
export const mockLeagues: League[] = [
  {
    id: 1,
    name: 'Test League',
    season: 2024,
    isActive: true,
    totalTeams: 8,
    totalConferences: 2,
  },
  {
    id: 2,
    name: 'Another League',
    season: 2024,
    isActive: true,
    totalTeams: 16,
    totalConferences: 2,
  },
]

export const mockLeagueDetail: LeagueDetail = {
  id: 1,
  name: 'Test League',
  season: 2024,
  isActive: true,
  totalTeams: 8,
  totalConferences: 2,
  conferences: [
    {
      id: 1,
      name: 'Conference A',
      divisions: [
        {
          id: 1,
          name: 'Division 1',
          teams: [mockTeams[0], mockTeams[1]],
        },
        {
          id: 2,
          name: 'Division 2',
          teams: [],
        },
      ],
    },
    {
      id: 2,
      name: 'Conference B',
      divisions: [
        {
          id: 3,
          name: 'Division 3',
          teams: [],
        },
        {
          id: 4,
          name: 'Division 4',
          teams: [],
        },
      ],
    },
  ],
}

// Mutable state for tests that modify data
let leaguesState = [...mockLeagues]
let nextLeagueId = 3

// Helper to reset mutable state between tests
export const resetMockState = () => {
  leaguesState = [...mockLeagues]
  nextLeagueId = 3
}

// API handlers
export const handlers = [
  // ============ TEAMS ============
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

  // ============ GAMES ============
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

  // ============ USERS ============
  // GET /api/users/me
  http.get('/api/users/me', () => {
    return HttpResponse.json(mockUser)
  }),

  // GET /api/users/league/:leagueId
  http.get('/api/users/league/:leagueId', () => {
    return HttpResponse.json([mockUser])
  }),

  // POST /api/users/:userId/league-roles
  http.post('/api/users/:userId/league-roles', () => {
    return HttpResponse.json(mockUser)
  }),

  // DELETE /api/users/:userId/league-roles/:roleId
  http.delete('/api/users/:userId/league-roles/:roleId', () => {
    return HttpResponse.json(mockUser)
  }),

  // ============ LEAGUES ============
  // GET /api/leagues-management
  http.get('/api/leagues-management', () => {
    return HttpResponse.json(leaguesState)
  }),

  // GET /api/leagues-management/:id
  http.get('/api/leagues-management/:id', ({ params }) => {
    const league = leaguesState.find(l => l.id === Number(params.id))
    if (!league) {
      return new HttpResponse(null, { status: 404 })
    }
    // Return detailed version for single league
    return HttpResponse.json({
      ...mockLeagueDetail,
      id: league.id,
      name: league.name,
      season: league.season,
      isActive: league.isActive,
    })
  }),

  // POST /api/leagues-management
  http.post('/api/leagues-management', async ({ request }) => {
    const body = await request.json() as CreateLeagueRequest
    const totalTeams = body.numberOfConferences * body.divisionsPerConference * body.teamsPerDivision
    const newLeague: League = {
      id: nextLeagueId++,
      name: body.name,
      season: 2024,
      isActive: true,
      totalTeams,
      totalConferences: body.numberOfConferences,
    }
    leaguesState.push(newLeague)
    return HttpResponse.json({
      ...mockLeagueDetail,
      id: newLeague.id,
      name: newLeague.name,
      totalTeams: newLeague.totalTeams,
      totalConferences: newLeague.totalConferences,
    })
  }),

  // PUT /api/leagues-management/:id
  http.put('/api/leagues-management/:id', async ({ params, request }) => {
    const id = Number(params.id)
    const body = await request.json() as { name?: string; season?: number; isActive?: boolean }
    const leagueIndex = leaguesState.findIndex(l => l.id === id)
    if (leagueIndex === -1) {
      return new HttpResponse(null, { status: 404 })
    }
    leaguesState[leagueIndex] = {
      ...leaguesState[leagueIndex],
      ...body,
    }
    return HttpResponse.json({
      ...mockLeagueDetail,
      ...leaguesState[leagueIndex],
    })
  }),

  // DELETE /api/leagues-management/:id
  http.delete('/api/leagues-management/:id', ({ params }) => {
    const id = Number(params.id)
    const leagueIndex = leaguesState.findIndex(l => l.id === id)
    if (leagueIndex === -1) {
      return new HttpResponse(null, { status: 404 })
    }
    leaguesState.splice(leagueIndex, 1)
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/leagues-management/:id/populate-rosters
  http.post('/api/leagues-management/:id/populate-rosters', ({ params }) => {
    const id = Number(params.id)
    const league = leaguesState.find(l => l.id === id)
    if (!league) {
      return new HttpResponse(null, { status: 404 })
    }
    return HttpResponse.json({
      id: league.id,
      name: league.name,
      totalTeams: league.totalTeams,
    })
  }),
]
