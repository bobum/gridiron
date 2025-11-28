/**
 * User and role types matching backend UserDto
 */

export interface UserLeagueRole {
  id: number;
  leagueId: number;
  leagueName: string;
  role: 'Commissioner' | 'GeneralManager';
  teamId: number | null;
  teamName: string | null;
  assignedAt: string;
}

export interface User {
  id: number;
  email: string;
  displayName: string;
  isGlobalAdmin: boolean;
  createdAt: string;
  lastLoginAt: string;
  leagueRoles: UserLeagueRole[];
}

export interface AssignLeagueRoleRequest {
  leagueId: number;
  role: 'Commissioner' | 'GeneralManager';
  teamId?: number;
}
