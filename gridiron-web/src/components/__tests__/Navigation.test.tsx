import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { Navigation } from '../Navigation'

describe('Navigation', () => {
  it('renders the app title', () => {
    render(
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    )

    expect(screen.getByText('Gridiron')).toBeInTheDocument()
  })

  it('renders all navigation links', () => {
    render(
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    )

    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Teams')).toBeInTheDocument()
    expect(screen.getByText('Simulate Game')).toBeInTheDocument()
  })

  it('renders the subtitle', () => {
    render(
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    )

    expect(screen.getByText('Football Manager')).toBeInTheDocument()
  })

  it('has correct href attributes', () => {
    render(
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    )

    const homeLink = screen.getByRole('link', { name: /home/i })
    const teamsLink = screen.getByRole('link', { name: /teams/i })
    const simulateLink = screen.getByRole('link', { name: /simulate game/i })

    expect(homeLink).toHaveAttribute('href', '/')
    expect(teamsLink).toHaveAttribute('href', '/teams')
    expect(simulateLink).toHaveAttribute('href', '/simulate')
  })
})
