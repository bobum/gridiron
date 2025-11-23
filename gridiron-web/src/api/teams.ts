import { useQuery } from '@tanstack/react-query';
import { apiClient } from './client';
import type { Team, TeamWithRoster } from '../types/Team';

// API functions
export const teamsApi = {
  getAll: async (): Promise<Team[]> => {
    const response = await apiClient.get<Team[]>('/teams');
    return response.data;
  },

  getById: async (id: number): Promise<Team> => {
    const response = await apiClient.get<Team>(`/teams/${id}`);
    return response.data;
  },

  getRoster: async (id: number): Promise<TeamWithRoster> => {
    const response = await apiClient.get<TeamWithRoster>(`/teams/${id}/roster`);
    return response.data;
  },
};

// React Query hooks
export const useTeams = () => {
  return useQuery({
    queryKey: ['teams'],
    queryFn: teamsApi.getAll,
  });
};

export const useTeam = (id: number) => {
  return useQuery({
    queryKey: ['teams', id],
    queryFn: () => teamsApi.getById(id),
    enabled: !!id, // Only fetch if id is provided
  });
};

export const useTeamRoster = (id: number) => {
  return useQuery({
    queryKey: ['teams', id, 'roster'],
    queryFn: () => teamsApi.getRoster(id),
    enabled: !!id,
  });
};
