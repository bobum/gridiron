import type { Team } from './Team';

/**
 * League types matching backend DTOs
 */

export interface League {
  id: number;
  name: string;
  season: number;
  isActive: boolean;
  totalTeams: number;
  totalConferences: number;
}

export interface LeagueDetail extends League {
  conferences: Conference[];
}

export interface Conference {
  id: number;
  name: string;
  divisions: Division[];
}

export interface Division {
  id: number;
  name: string;
  teams: Team[];
}

export interface CreateLeagueRequest {
  name: string;
  numberOfConferences: number;
  divisionsPerConference: number;
  teamsPerDivision: number;
}

export interface UpdateLeagueRequest {
  name?: string;
  season?: number;
  isActive?: boolean;
}

export interface PopulateRostersResponse {
  id: number;
  name: string;
  totalTeams: number;
}
