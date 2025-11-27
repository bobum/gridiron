import axios from 'axios';
import type { PublicClientApplication } from '@azure/msal-browser';
import { apiRequest } from '../config/authConfig';

// API base URL - uses Vite proxy in development, can be overridden with env var
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 second timeout for game simulations
});

/**
 * Sets up authentication interceptor for API client
 * This should be called once after MSAL is initialized
 */
export const setupAuthInterceptor = (msalInstance: PublicClientApplication) => {
  // Request interceptor to add auth token
  apiClient.interceptors.request.use(
    async (config) => {
      const accounts = msalInstance.getAllAccounts();

      if (accounts.length > 0) {
        try {
          const response = await msalInstance.acquireTokenSilent({
            ...apiRequest,
            account: accounts[0],
          });
          config.headers.Authorization = `Bearer ${response.accessToken}`;
        } catch (error) {
          console.warn('Failed to acquire token silently:', error);
          // Continue without token - API will return 401 if auth is required
        }
      }

      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );
};

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Server responded with error status
      console.error('API Error:', error.response.status, error.response.data);
    } else if (error.request) {
      // Request made but no response
      console.error('Network Error: No response from server');
    } else {
      // Something else happened
      console.error('Error:', error.message);
    }
    return Promise.reject(error);
  }
);
