import { InteractionType } from '@azure/msal-browser';
import { MsalAuthenticationTemplate } from '@azure/msal-react';
import { loginRequest } from '../config/authConfig';
import { Loading } from './Loading';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

/**
 * ProtectedRoute component that ensures user is authenticated
 * Uses MSAL's built-in authentication template to handle login flow
 * In E2E test mode, authentication is bypassed (DEV ONLY)
 */
export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  // Skip authentication in E2E test mode - ONLY works in development
  // Production builds will always require authentication
  const isTestMode = import.meta.env.VITE_E2E_TEST_MODE === 'true' && import.meta.env.DEV;

  if (isTestMode) {
    console.log('[E2E Test Mode] Authentication bypassed for testing');
    return <>{children}</>;
  }

  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      loadingComponent={Loading}
    >
      {children}
    </MsalAuthenticationTemplate>
  );
};
