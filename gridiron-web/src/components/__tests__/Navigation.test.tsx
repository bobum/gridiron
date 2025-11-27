import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { PublicClientApplication } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { Navigation } from '../Navigation'

// Create a mock MSAL instance for testing
const msalConfig = {
  auth: {
    clientId: 'test-client-id',
    authority: 'https://test.ciamlogin.com',
  },
}
const msalInstance = new PublicClientApplication(msalConfig)

const renderNavigation = () => {
  return render(
    <MsalProvider instance={msalInstance}>
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    </MsalProvider>
  )
}

describe('Navigation', () => {
  it('renders the app title', () => {
    renderNavigation()
    expect(screen.getByText('Gridiron')).toBeInTheDocument()
  })

  it('renders all navigation links', () => {
    renderNavigation()
    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Teams')).toBeInTheDocument()
    expect(screen.getByText('Simulate Game')).toBeInTheDocument()
  })

  it('renders the login button when not authenticated', () => {
    renderNavigation()
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument()
  })

  it('has correct href attributes', () => {
    renderNavigation()

    const homeLink = screen.getByRole('link', { name: /home/i })
    const teamsLink = screen.getByRole('link', { name: /teams/i })
    const simulateLink = screen.getByRole('link', { name: /simulate game/i })

    expect(homeLink).toHaveAttribute('href', '/')
    expect(teamsLink).toHaveAttribute('href', '/teams')
    expect(simulateLink).toHaveAttribute('href', '/simulate')
  })
})
