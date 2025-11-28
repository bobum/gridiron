import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type { User, AssignLeagueRoleRequest } from '../types/User';

// API functions
export const usersApi = {
  getCurrentUser: async (): Promise<User> => {
    const response = await apiClient.get<User>('/users/me');
    return response.data;
  },

  getUsersByLeague: async (leagueId: number): Promise<User[]> => {
    const response = await apiClient.get<User[]>(`/users/league/${leagueId}`);
    return response.data;
  },

  assignLeagueRole: async (userId: number, request: AssignLeagueRoleRequest): Promise<User> => {
    const response = await apiClient.post<User>(`/users/${userId}/league-roles`, request);
    return response.data;
  },

  removeLeagueRole: async (userId: number, roleId: number): Promise<User> => {
    const response = await apiClient.delete<User>(`/users/${userId}/league-roles/${roleId}`);
    return response.data;
  },
};

// React Query hooks
export const useCurrentUser = () => {
  return useQuery({
    queryKey: ['users', 'me'],
    queryFn: usersApi.getCurrentUser,
  });
};

export const useLeagueUsers = (leagueId: number) => {
  return useQuery({
    queryKey: ['users', 'league', leagueId],
    queryFn: () => usersApi.getUsersByLeague(leagueId),
    enabled: !!leagueId,
  });
};

export const useAssignLeagueRole = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, request }: { userId: number; request: AssignLeagueRoleRequest }) =>
      usersApi.assignLeagueRole(userId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });
};

export const useRemoveLeagueRole = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: number; roleId: number }) =>
      usersApi.removeLeagueRole(userId, roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });
};
