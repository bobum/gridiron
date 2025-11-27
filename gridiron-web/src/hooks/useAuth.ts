import { useMsal, useIsAuthenticated } from '@azure/msal-react';
import { InteractionStatus } from '@azure/msal-browser';
import { loginRequest, apiRequest } from '../config/authConfig';

/**
 * Custom hook for authentication operations
 * Provides login, logout, and token acquisition functions
 */
export const useAuth = () => {
  const { instance, accounts, inProgress } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  const login = async () => {
    try {
      await instance.loginRedirect(loginRequest);
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  };

  const logout = async () => {
    try {
      await instance.logoutRedirect({
        account: accounts[0],
      });
    } catch (error) {
      console.error('Logout error:', error);
      throw error;
    }
  };

  /**
   * Acquires an access token silently for API calls
   * Falls back to interactive login if silent acquisition fails
   */
  const getAccessToken = async (): Promise<string> => {
    if (!isAuthenticated || accounts.length === 0) {
      throw new Error('User is not authenticated');
    }

    try {
      const response = await instance.acquireTokenSilent({
        ...apiRequest,
        account: accounts[0],
      });
      return response.accessToken;
    } catch (error) {
      console.warn('Silent token acquisition failed, attempting interactive redirect:', error);
      // acquireTokenRedirect doesn't return a value - it redirects the page
      // After redirect completes, the token will be available
      await instance.acquireTokenRedirect(apiRequest);
      throw new Error('Redirecting for token acquisition');
    }
  };

  const user = accounts[0] || null;

  return {
    login,
    logout,
    getAccessToken,
    isAuthenticated,
    user,
    isLoading: inProgress === InteractionStatus.Login || inProgress === InteractionStatus.Logout,
  };
};
