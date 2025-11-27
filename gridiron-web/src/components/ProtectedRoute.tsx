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
 */
export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
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
