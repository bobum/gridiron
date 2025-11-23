import type { Team } from './Team';
import { Possession, PlayType, Downs } from './enums';

export interface Game {
  id: number;
  homeTeamId: number;
  awayTeamId: number;
  homeTeam?: Team;
  awayTeam?: Team;
  homeScore: number;
  awayScore: number;
  fieldPosition: number;
  yardsToGo: number;
  currentDown: Downs;
  randomSeed?: number;
  plays?: Play[];
}

export interface Play {
  id?: number;
  gameId?: number;
  playType: PlayType;
  possession: Possession;
  down: Downs;
  startFieldPosition: number;
  endFieldPosition: number;
  yardsGained: number;
  startTime: number;
  stopTime: number;
  elapsedTime: number;
  isTouchdown: boolean;
  isFirstDown: boolean;
  isSafety: boolean;
  possessionChange: boolean;
  interception: boolean;
  quarterExpired: boolean;
  halfExpired: boolean;
  gameExpired: boolean;
}

export interface SimulateGameRequest {
  homeTeamId: number;
  awayTeamId: number;
  randomSeed?: number;
}

export interface SimulateGameResponse {
  id: number;
  homeTeamId: number;
  awayTeamId: number;
  homeScore: number;
  awayScore: number;
  message?: string;
}
