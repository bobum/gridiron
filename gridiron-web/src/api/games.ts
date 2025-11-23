import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type { Game, Play, SimulateGameRequest, SimulateGameResponse } from '../types/Game';

// API functions
export const gamesApi = {
  getAll: async (): Promise<Game[]> => {
    const response = await apiClient.get<Game[]>('/games');
    return response.data;
  },

  getById: async (id: number): Promise<Game> => {
    const response = await apiClient.get<Game>(`/games/${id}`);
    return response.data;
  },

  getPlays: async (id: number): Promise<Play[]> => {
    const response = await apiClient.get<Play[]>(`/games/${id}/plays`);
    return response.data;
  },

  simulate: async (request: SimulateGameRequest): Promise<SimulateGameResponse> => {
    const response = await apiClient.post<SimulateGameResponse>('/games/simulate', request);
    return response.data;
  },
};

// React Query hooks
export const useGames = () => {
  return useQuery({
    queryKey: ['games'],
    queryFn: gamesApi.getAll,
  });
};

export const useGame = (id: number) => {
  return useQuery({
    queryKey: ['games', id],
    queryFn: () => gamesApi.getById(id),
    enabled: !!id,
  });
};

export const useGamePlays = (id: number) => {
  return useQuery({
    queryKey: ['games', id, 'plays'],
    queryFn: () => gamesApi.getPlays(id),
    enabled: !!id,
  });
};

export const useSimulateGame = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: gamesApi.simulate,
    onSuccess: () => {
      // Invalidate games list to refresh after simulation
      queryClient.invalidateQueries({ queryKey: ['games'] });
    },
  });
};
