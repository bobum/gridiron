import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';
import { Loading } from '../components';

/**
 * LoginCallbackPage handles the redirect from Azure Entra ID
 * and processes the authentication response
 */
export const LoginCallbackPage = () => {
  const { instance } = useMsal();
  const navigate = useNavigate();

  useEffect(() => {
    const handleRedirect = async () => {
      try {
        const response = await instance.handleRedirectPromise();
        if (response) {
          // Successfully authenticated, redirect to home
          navigate('/');
        }
      } catch (error) {
        console.error('Login callback error:', error);
        // On error, redirect to home (will trigger login again if needed)
        navigate('/');
      }
    };

    handleRedirect();
  }, [instance, navigate]);

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="text-center">
        <Loading />
        <p className="mt-4 text-gray-600">Completing authentication...</p>
      </div>
    </div>
  );
};
