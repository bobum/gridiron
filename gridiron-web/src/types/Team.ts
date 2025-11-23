import { Player } from './Player';

export interface Team {
  id: number;
  divisionId: number | null;
  name: string;
  city: string;
  budget: number;
  championships: number;
  wins: number;
  losses: number;
  ties: number;
  fanSupport: number;
  chemistry: number;
  players?: Player[]; // Optional, included when fetching roster
}

export interface TeamWithRoster extends Team {
  players: Player[];
}

export interface TeamStats {
  totalYards?: number;
  passingYards?: number;
  rushingYards?: number;
  turnovers?: number;
  penalties?: number;
  penaltyYards?: number;
  timeOfPossession?: number;
}
