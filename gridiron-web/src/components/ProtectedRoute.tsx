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
 * In E2E test mode, authentication is bypassed
 */
export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  // Skip authentication in E2E test mode
  const isTestMode = import.meta.env.VITE_E2E_TEST_MODE === 'true';

  if (isTestMode) {
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
