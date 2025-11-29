import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type {
  League,
  LeagueDetail,
  CreateLeagueRequest,
  UpdateLeagueRequest,
  PopulateRostersResponse
} from '../types/League';

// API functions
export const leaguesApi = {
  getAll: async (): Promise<League[]> => {
    const response = await apiClient.get<League[]>('/leagues-management');
    return response.data;
  },

  getById: async (id: number): Promise<LeagueDetail> => {
    const response = await apiClient.get<LeagueDetail>(`/leagues-management/${id}`);
    return response.data;
  },

  create: async (request: CreateLeagueRequest): Promise<LeagueDetail> => {
    const response = await apiClient.post<LeagueDetail>('/leagues-management', request);
    return response.data;
  },

  update: async (id: number, request: UpdateLeagueRequest): Promise<LeagueDetail> => {
    const response = await apiClient.put<LeagueDetail>(`/leagues-management/${id}`, request);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/leagues-management/${id}`);
  },

  populateRosters: async (id: number, seed?: number): Promise<PopulateRostersResponse> => {
    const url = seed !== undefined
      ? `/leagues-management/${id}/populate-rosters?seed=${seed}`
      : `/leagues-management/${id}/populate-rosters`;
    const response = await apiClient.post<PopulateRostersResponse>(url);
    return response.data;
  },
};

// React Query hooks
export const useLeagues = () => {
  return useQuery({
    queryKey: ['leagues'],
    queryFn: leaguesApi.getAll,
  });
};

export const useLeague = (id: number) => {
  return useQuery({
    queryKey: ['leagues', id],
    queryFn: () => leaguesApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateLeague = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: leaguesApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leagues'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'me'] });
    },
  });
};

export const useUpdateLeague = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: number; request: UpdateLeagueRequest }) =>
      leaguesApi.update(id, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['leagues'] });
      queryClient.invalidateQueries({ queryKey: ['leagues', variables.id] });
    },
  });
};

export const useDeleteLeague = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: leaguesApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leagues'] });
      queryClient.invalidateQueries({ queryKey: ['users', 'me'] });
    },
  });
};

export const usePopulateRosters = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, seed }: { id: number; seed?: number }) =>
      leaguesApi.populateRosters(id, seed),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['leagues', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
    },
  });
};
