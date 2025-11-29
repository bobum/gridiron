import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { render } from '../../test/test-utils'
import { LeaguesPage } from '../LeaguesPage'

describe('LeaguesPage', () => {
  describe('Page Display', () => {
    it('shows loading state initially', () => {
      const { container } = render(<LeaguesPage />)
      expect(container.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('displays page header', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'My Leagues' })).toBeInTheDocument()
        expect(screen.getByText('Manage your leagues or create a new one')).toBeInTheDocument()
      })
    })

    it('displays create league button', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })
    })

    it('displays leagues after loading', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByText('Test League')).toBeInTheDocument()
        expect(screen.getByText('Another League')).toBeInTheDocument()
      })
    })

    it('displays league details in cards', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        // Check season info
        expect(screen.getAllByText('2024').length).toBeGreaterThan(0)
        // Check status
        expect(screen.getAllByText('Active').length).toBeGreaterThan(0)
      })
    })
  })

  describe('Create League Modal', () => {
    it('opens create league modal when clicking create button', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))

      expect(screen.getByTestId('create-league-modal')).toBeInTheDocument()
      expect(screen.getByRole('heading', { name: 'Create New League' })).toBeInTheDocument()
    })

    it('closes modal when clicking X button', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))
      expect(screen.getByTestId('create-league-modal')).toBeInTheDocument()

      await user.click(screen.getByTestId('close-modal-button'))
      expect(screen.queryByTestId('create-league-modal')).not.toBeInTheDocument()
    })

    it('closes modal when clicking Cancel button', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))
      expect(screen.getByTestId('create-league-modal')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))
      expect(screen.queryByTestId('create-league-modal')).not.toBeInTheDocument()
    })

    it('displays league structure form elements', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))

      expect(screen.getByTestId('league-name-input')).toBeInTheDocument()
      expect(screen.getByTestId('conferences-input')).toBeInTheDocument()
      expect(screen.getByTestId('divisions-input')).toBeInTheDocument()
      expect(screen.getByTestId('teams-input')).toBeInTheDocument()
      expect(screen.getByTestId('total-teams')).toBeInTheDocument()
    })

    it('calculates total teams correctly with default values', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))

      // Default: 2 conferences x 2 divisions x 4 teams = 16
      expect(screen.getByTestId('total-teams')).toHaveTextContent('16')
    })

    it('shows error if name is empty on submit', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))
      await user.click(screen.getByTestId('submit-create-league'))

      expect(screen.getByTestId('form-error')).toHaveTextContent('League name is required')
    })

    it('creates a league successfully', async () => {
      const user = userEvent.setup()
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByTestId('create-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('create-league-button'))

      // Fill in the form - just the name, keep defaults for structure
      await user.type(screen.getByTestId('league-name-input'), 'New Test League')

      // Submit
      await user.click(screen.getByTestId('submit-create-league'))

      // Modal should close
      await waitFor(() => {
        expect(screen.queryByTestId('create-league-modal')).not.toBeInTheDocument()
      })
    })
  })

  describe('League Cards', () => {
    it('displays role badges for commissioner', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        // Mock user has Commissioner role for Test League (ID: 1)
        expect(screen.getByText(/Commissioner/)).toBeInTheDocument()
      })
    })

    it('league cards are clickable links', async () => {
      render(<LeaguesPage />)

      await waitFor(() => {
        expect(screen.getByText('Test League')).toBeInTheDocument()
      })

      // Find the link for Test League
      const leagueLink = screen.getByTestId('league-card-1')
      expect(leagueLink).toHaveAttribute('href', '/leagues/1')
    })
  })
})
