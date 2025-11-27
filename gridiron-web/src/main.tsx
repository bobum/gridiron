import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { PublicClientApplication } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import './index.css'
import App from './App.tsx'
import { msalConfig } from './config/authConfig'
import { setupAuthInterceptor } from './api/client'

const msalInstance = new PublicClientApplication(msalConfig)

// Setup auth interceptor for API calls
setupAuthInterceptor(msalInstance)

// Initialize MSAL and handle redirect promise before rendering
msalInstance.initialize().then(() => {
  // Handle redirect promise to prevent redirect loops
  msalInstance.handleRedirectPromise().then(() => {
    createRoot(document.getElementById('root')!).render(
      <StrictMode>
        <MsalProvider instance={msalInstance}>
          <App />
        </MsalProvider>
      </StrictMode>,
    )
  }).catch((error) => {
    console.error('Error handling redirect:', error)
  })
})
