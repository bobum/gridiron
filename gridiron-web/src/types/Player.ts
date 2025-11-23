import { Position } from './enums';

export interface Player {
  id: number;
  teamId: number | null;
  position: Position;
  number: number;
  firstName: string;
  lastName: string;
  age: number;
  exp: number; // years of experience
  height: string;
  college: string;

  // General attributes (0-100)
  speed: number;
  strength: number;
  agility: number;
  awareness: number;
  fragility: number;
  morale: number;
  discipline: number;

  // Position-specific skills (0-100)
  passing: number;    // QB
  catching: number;   // WR/TE/RB
  rushing: number;    // RB/QB
  blocking: number;   // OL/TE
  tackling: number;   // Defense
  coverage: number;   // DB/LB
  kicking: number;    // K/P

  // Contract
  contractYears: number;
  salary: number;

  // Development
  potential: number;
  progression: number;

  // Status
  health: number;
  isRetired: boolean;
  isInjured: boolean;
}

export interface PlayerStats {
  // Passing
  passAttempts?: number;
  passCompletions?: number;
  passYards?: number;
  passTouchdowns?: number;
  interceptions?: number;
  sacks?: number;

  // Rushing
  rushAttempts?: number;
  rushYards?: number;
  rushTouchdowns?: number;
  fumbles?: number;

  // Receiving
  receptions?: number;
  receivingYards?: number;
  receivingTouchdowns?: number;

  // Defense
  tackles?: number;
  tacklesForLoss?: number;
  defensiveInterceptions?: number;

  // Kicking
  fieldGoalAttempts?: number;
  fieldGoalsMade?: number;
  extraPointAttempts?: number;
  extraPointsMade?: number;
}
