import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { render } from '../../test/test-utils'
import { ProfilePage } from '../ProfilePage'

describe('ProfilePage', () => {
  describe('Page Display', () => {
    it('shows loading state initially', () => {
      const { container } = render(<ProfilePage />)
      expect(container.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('displays page title', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'User Profile' })).toBeInTheDocument()
        expect(screen.getByText('Manage your account and view your leagues')).toBeInTheDocument()
      })
    })
  })

  describe('Account Information', () => {
    it('displays account information section', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByText('Account Information')).toBeInTheDocument()
      })
    })

    it('displays user ID with copy button', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByText('Your User ID')).toBeInTheDocument()
        expect(screen.getByRole('button', { name: 'Copy' })).toBeInTheDocument()
        expect(screen.getByText('Share this ID with league commissioners to join a league')).toBeInTheDocument()
      })
    })

    it('displays user ID value from mock data', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        // Mock user has ID 1
        expect(screen.getByText('1')).toBeInTheDocument()
      })
    })

    it('displays display name', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByText('Display Name')).toBeInTheDocument()
        expect(screen.getByText('Test User')).toBeInTheDocument()
      })
    })

    it('displays email', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByText('Email')).toBeInTheDocument()
        expect(screen.getByText('testuser@example.com')).toBeInTheDocument()
      })
    })

    it('displays account activity dates', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        expect(screen.getByText('Member Since')).toBeInTheDocument()
        expect(screen.getByText('Last Login')).toBeInTheDocument()
      })
    })
  })

  describe('My Leagues Section', () => {
    it('displays my leagues section with count', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        // Mock user has 1 league (Test League)
        expect(screen.getByRole('heading', { name: /My Leagues \(1\)/ })).toBeInTheDocument()
      })
    })

    it('displays league cards for leagues user belongs to', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        // Mock user is Commissioner of "Test League"
        expect(screen.getByText('Test League')).toBeInTheDocument()
      })
    })

    it('displays role badges on league cards', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        // Mock user has Commissioner role
        expect(screen.getByText(/Commissioner/)).toBeInTheDocument()
      })
    })

    it('league cards are clickable links', async () => {
      render(<ProfilePage />)

      await waitFor(() => {
        const leagueLink = screen.getByRole('link', { name: /Test League/ })
        expect(leagueLink).toHaveAttribute('href', '/leagues/1')
      })
    })
  })
})
