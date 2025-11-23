import { useQuery } from '@tanstack/react-query';
import { apiClient } from './client';
import { Player } from '../types';

// API functions
export const playersApi = {
  getAll: async (teamId?: number): Promise<Player[]> => {
    const params = teamId ? { teamId } : {};
    const response = await apiClient.get<Player[]>('/players', { params });
    return response.data;
  },

  getById: async (id: number): Promise<Player> => {
    const response = await apiClient.get<Player>(`/players/${id}`);
    return response.data;
  },
};

// React Query hooks
export const usePlayers = (teamId?: number) => {
  return useQuery({
    queryKey: teamId ? ['players', 'team', teamId] : ['players'],
    queryFn: () => playersApi.getAll(teamId),
  });
};

export const usePlayer = (id: number) => {
  return useQuery({
    queryKey: ['players', id],
    queryFn: () => playersApi.getById(id),
    enabled: !!id,
  });
};
