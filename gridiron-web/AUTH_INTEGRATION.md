# Azure Entra ID Authentication Integration

This document describes the Azure Entra ID (External CIAM) authentication integration in the Gridiron frontend application.

## Overview

The application now uses Azure Entra ID External CIAM for user authentication with support for:
- Email/password authentication
- Google social login
- OAuth 2.0 Authorization Code Flow with PKCE
- Automatic JWT token injection in API requests

## Architecture

### Key Components

1. **MSAL Configuration** (`src/config/authConfig.ts`)
   - Azure Entra ID connection settings
   - Authentication scopes
   - Session storage for tokens

2. **Authentication Hook** (`src/hooks/useAuth.ts`)
   - `login()` - Redirects user to Azure login
   - `logout()` - Logs user out and redirects
   - `getAccessToken()` - Retrieves access token for API calls
   - `isAuthenticated` - Current authentication status
   - `user` - Current user information

3. **Protected Routes** (`src/components/ProtectedRoute.tsx`)
   - Wraps routes that require authentication
   - Automatically redirects unauthenticated users to login

4. **API Client Integration** (`src/api/client.ts`)
   - Request interceptor automatically adds `Authorization: Bearer <token>` header
   - Silently acquires tokens for authenticated users
   - Falls back to interactive login if token acquisition fails

5. **Navigation Component** (`src/components/Navigation.tsx`)
   - Shows Login button when not authenticated
   - Shows user name and Logout button when authenticated

6. **Login Callback Page** (`src/pages/LoginCallbackPage.tsx`)
   - Handles redirect from Azure after authentication
   - Processes authentication response

## Setup Instructions

### Environment Variables

Create a `.env` file in `gridiron-web/` with the following variables:

```env
VITE_AZURE_CLIENT_ID=29348959-a014-4550-b3c3-044585c83f0a
VITE_AZURE_AUTHORITY=https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com
VITE_AZURE_REDIRECT_URI=http://localhost:3000
VITE_AZURE_POST_LOGOUT_REDIRECT_URI=http://localhost:3000
VITE_API_URL=/api
```

**Note:** The `.env` file is gitignored. Use `.env.example` as a template for other developers.

### Azure Configuration

The application is configured to use the **Goal To Go Football** app registration in Azure Entra External ID:

- **Tenant:** gtggridiron.onmicrosoft.com
- **Client ID:** 29348959-a014-4550-b3c3-044585c83f0a
- **Redirect URI:** http://localhost:3000
- **Supported Identity Providers:** Email/Password, Google

For detailed Azure setup instructions, see: [AUTHENTICATION_SETUP.md](../AUTHENTICATION_SETUP.md)

## Authentication Flow

1. **User visits protected route** → ProtectedRoute component checks authentication
2. **Not authenticated** → Redirects to Azure Entra ID login page
3. **User logs in** → Azure redirects back to app with authorization code
4. **LoginCallbackPage processes response** → MSAL exchanges code for tokens
5. **User is redirected to original route** → Now authenticated
6. **API requests include token** → Axios interceptor adds Bearer token automatically

## Making API Calls

All API calls automatically include authentication tokens:

```typescript
import { teamsApi } from './api/teams';

// Token is automatically added by Axios interceptor
const teams = await teamsApi.getAll();
```

No manual token handling required!

## Testing

The authentication integration includes comprehensive tests:

- **Navigation.test.tsx** - Tests login button rendering and navigation
- **E2E Tests** - All E2E tests run with authentication bypassed via VITE_E2E_TEST_MODE flag
- All existing tests updated to work with MSAL provider

Run component tests with:
```bash
npm test
```

Run E2E tests with:
```bash
npm run test:e2e
```

**Important:** The E2E test mode flag (VITE_E2E_TEST_MODE) is set by Playwright's webServer configuration using `cross-env` for cross-platform compatibility. Do NOT add this flag to your `.env` file, as it will override Playwright's setting and cause tests to fail.

**Technical Details:**
- Playwright uses `cross-env` to pass the environment variable directly to Vite
- This ensures the flag is available when Vite builds the application for testing
- The `cross-env` package is a dev dependency that works on Windows, Linux, and macOS
- Command: `cross-env VITE_E2E_TEST_MODE=true npm run dev`

## Development

### Running the Application

1. Start the API backend:
```bash
cd Gridiron.WebApi
dotnet run
```

2. Start the frontend dev server:
```bash
cd gridiron-web
npm run dev
```

3. Navigate to `http://localhost:3000`
4. You'll be redirected to Azure Entra ID login
5. After logging in, you'll return to the app authenticated

### Debugging

MSAL logs are configured to show warnings and errors in the browser console. Check the console for:
- Token acquisition issues
- Authentication errors
- Redirect problems

## Next Steps: API Authorization

Currently, all authenticated users can access all data. The next phase is to implement backend authorization:

1. **Configure API to validate Azure tokens**
2. **Add user claims (userId) to tokens**
3. **Implement authorization policies**
   - Users can only access their own leagues and teams
   - Add `CreatedBy` field to League and Team entities
   - Filter queries by current user ID

4. **Add role-based access control (optional)**
   - Admin role for managing all data
   - User role for managing own data

## Files Modified/Created

### Created Files
- `gridiron-web/.env` - Environment variables (gitignored)
- `gridiron-web/.env.example` - Environment template
- `gridiron-web/src/config/authConfig.ts` - MSAL configuration
- `gridiron-web/src/hooks/useAuth.ts` - Authentication hook
- `gridiron-web/src/components/ProtectedRoute.tsx` - Route guard component
- `gridiron-web/src/pages/LoginCallbackPage.tsx` - Auth callback handler
- `gridiron-web/AUTH_INTEGRATION.md` - This file

### Modified Files
- `gridiron-web/package.json` - Added MSAL dependencies
- `gridiron-web/src/main.tsx` - Added MsalProvider wrapper
- `gridiron-web/src/App.tsx` - Added protected routes
- `gridiron-web/src/api/client.ts` - Added auth interceptor
- `gridiron-web/src/components/Navigation.tsx` - Added login/logout UI
- `gridiron-web/src/types/enums.ts` - Fixed TypeScript enum errors
- `gridiron-web/.gitignore` - Added .env files
- `.github/workflows/frontend-tests.yml` - Added build step to CI/CD

## Security Notes

- Tokens are stored in `sessionStorage` (cleared on browser close)
- Access tokens are short-lived and refreshed automatically
- PKCE is used for additional security in the auth flow
- No tokens or secrets are committed to git
- All API requests require authentication

### E2E Test Mode Security

The `VITE_E2E_TEST_MODE` flag is safe for production because:
1. **Build-time only**: Set when Vite compiles, not at runtime
2. **Not set in production**: Production deployment never sets this variable
3. **Azure Static Web Apps**: Won't have this environment variable configured
4. **No user control**: Users cannot enable test mode after the app is built

**Important**: Never set `VITE_E2E_TEST_MODE=true` in production environment variables.
This should ONLY be set by Playwright in the test pipeline.

## Troubleshooting

### Issue: Infinite redirect loop
**Solution:** Check that `VITE_AZURE_REDIRECT_URI` matches the redirect URI registered in Azure

### Issue: Token acquisition fails
**Solution:** Ensure the scopes in `authConfig.ts` are configured correctly in Azure

### Issue: API returns 401 Unauthorized
**Solution:** This is expected until the backend is configured to accept Azure tokens. See "Next Steps: API Authorization" above.

### Issue: Login page doesn't redirect back
**Solution:** Check browser console for MSAL errors. Verify Azure app registration settings.

## Resources

- [Azure Entra External ID Documentation](https://learn.microsoft.com/en-us/azure/active-directory/external-identities/)
- [MSAL.js Documentation](https://github.com/AzureAD/microsoft-authentication-library-for-js)
- [React Authentication Tutorial](https://learn.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-react)
