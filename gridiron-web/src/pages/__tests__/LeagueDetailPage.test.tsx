import { describe, it, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { render } from '../../test/test-utils'
import { LeagueDetailPage } from '../LeagueDetailPage'

// Mock useParams to return league ID
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useParams: () => ({ id: '1' }),
  }
})

describe('LeagueDetailPage', () => {
  describe('Page Display', () => {
    it('shows loading state initially', () => {
      const { container } = render(<LeagueDetailPage />)
      expect(container.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('displays league name after loading', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('league-name')).toHaveTextContent('Test League')
      })
    })

    it('displays back to leagues link', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByText('← Back to Leagues')).toBeInTheDocument()
      })
    })

    it('displays season and status', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByText('Season 2024')).toBeInTheDocument()
        expect(screen.getByText('Active')).toBeInTheDocument()
      })
    })

    it('displays commissioner badge for commissioner user', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        // Multiple commissioner badges may appear (header + members list)
        expect(screen.getAllByText('👑 Commissioner').length).toBeGreaterThan(0)
      })
    })
  })

  describe('League Stats', () => {
    it('displays league statistics cards', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByText('Conferences')).toBeInTheDocument()
        expect(screen.getByText('Divisions')).toBeInTheDocument()
        expect(screen.getByText('Teams')).toBeInTheDocument()
        expect(screen.getByText('Players')).toBeInTheDocument()
      })
    })

    it('displays correct conference count', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        // mockLeagueDetail has 2 conferences
        const conferencesStat = screen.getByText('Conferences').previousElementSibling
        expect(conferencesStat).toHaveTextContent('2')
      })
    })
  })

  describe('League Structure', () => {
    it('displays league structure heading', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'League Structure' })).toBeInTheDocument()
      })
    })

    it('displays conferences', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByText('Conference A')).toBeInTheDocument()
        expect(screen.getByText('Conference B')).toBeInTheDocument()
      })
    })

    it('expands conference on click to show divisions', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('conference-1')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('conference-1'))

      await waitFor(() => {
        expect(screen.getByTestId('division-1')).toBeInTheDocument()
        expect(screen.getByText('Division 1')).toBeInTheDocument()
      })
    })

    it('collapses conference on second click', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('conference-1')).toBeInTheDocument()
      })

      // First click expands
      await user.click(screen.getByTestId('conference-1'))
      await waitFor(() => {
        expect(screen.getByTestId('division-1')).toBeInTheDocument()
      })

      // Second click collapses
      await user.click(screen.getByTestId('conference-1'))
      await waitFor(() => {
        expect(screen.queryByTestId('division-1')).not.toBeInTheDocument()
      })
    })
  })

  describe('Commissioner Controls', () => {
    it('shows edit button for commissioner', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('edit-league-button')).toBeInTheDocument()
      })
    })

    it('shows delete button for commissioner', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('delete-league-button')).toBeInTheDocument()
      })
    })

    it('shows populate rosters button for commissioner', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('populate-rosters-button')).toBeInTheDocument()
      })
    })

    it('shows add user button for commissioner', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })
    })
  })

  describe('Edit League Modal', () => {
    it('opens edit modal when clicking edit button', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('edit-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('edit-league-button'))

      expect(screen.getByTestId('edit-league-modal')).toBeInTheDocument()
      expect(screen.getByRole('heading', { name: 'Edit League' })).toBeInTheDocument()
    })

    it('displays form fields with current values', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('edit-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('edit-league-button'))

      expect(screen.getByTestId('edit-name-input')).toHaveValue('Test League')
      expect(screen.getByTestId('edit-season-input')).toHaveValue(2024)
      expect(screen.getByTestId('edit-active-checkbox')).toBeChecked()
    })

    it('closes modal when clicking cancel', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('edit-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('edit-league-button'))
      expect(screen.getByTestId('edit-league-modal')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))
      expect(screen.queryByTestId('edit-league-modal')).not.toBeInTheDocument()
    })

    it('submits edit form successfully', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('edit-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('edit-league-button'))

      // Clear and type new name
      await user.clear(screen.getByTestId('edit-name-input'))
      await user.type(screen.getByTestId('edit-name-input'), 'Updated League')

      await user.click(screen.getByTestId('submit-edit-league'))

      // Modal should close on success
      await waitFor(() => {
        expect(screen.queryByTestId('edit-league-modal')).not.toBeInTheDocument()
      })
    })
  })

  describe('Delete League Modal', () => {
    it('opens delete modal when clicking delete button', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('delete-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('delete-league-button'))

      expect(screen.getByTestId('delete-league-modal')).toBeInTheDocument()
      expect(screen.getByRole('heading', { name: 'Delete League' })).toBeInTheDocument()
    })

    it('displays warning message with league name', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('delete-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('delete-league-button'))

      expect(screen.getByText(/Are you sure you want to delete/)).toBeInTheDocument()
      // League name appears multiple times on page, just check modal content
      const modal = screen.getByTestId('delete-league-modal')
      expect(modal).toHaveTextContent('Test League')
    })

    it('closes modal when clicking cancel', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('delete-league-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('delete-league-button'))
      expect(screen.getByTestId('delete-league-modal')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))
      expect(screen.queryByTestId('delete-league-modal')).not.toBeInTheDocument()
    })
  })

  describe('Populate Rosters Modal', () => {
    it('opens populate modal when clicking populate button', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('populate-rosters-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('populate-rosters-button'))

      expect(screen.getByTestId('populate-rosters-modal')).toBeInTheDocument()
      expect(screen.getByRole('heading', { name: 'Populate Team Rosters' })).toBeInTheDocument()
    })

    it('displays seed input field', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('populate-rosters-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('populate-rosters-button'))

      expect(screen.getByTestId('populate-seed-input')).toBeInTheDocument()
      expect(screen.getByPlaceholderText('Leave empty for random')).toBeInTheDocument()
    })

    it('closes modal when clicking cancel', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('populate-rosters-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('populate-rosters-button'))
      expect(screen.getByTestId('populate-rosters-modal')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))
      expect(screen.queryByTestId('populate-rosters-modal')).not.toBeInTheDocument()
    })

    it('submits populate form successfully', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('populate-rosters-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('populate-rosters-button'))
      await user.click(screen.getByTestId('confirm-populate-rosters'))

      // Modal should close on success
      await waitFor(() => {
        expect(screen.queryByTestId('populate-rosters-modal')).not.toBeInTheDocument()
      })
    })
  })

  describe('Add User Modal', () => {
    it('opens add user modal when clicking add user button', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))

      expect(screen.getByTestId('add-user-modal')).toBeInTheDocument()
      expect(screen.getByRole('heading', { name: 'Add User to League' })).toBeInTheDocument()
    })

    it('displays user ID and role form fields', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))

      expect(screen.getByTestId('add-user-id-input')).toBeInTheDocument()
      expect(screen.getByTestId('add-user-role-select')).toBeInTheDocument()
    })

    it('shows team dropdown when General Manager role selected', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))

      // Default role is GeneralManager, so team select should be visible
      expect(screen.getByTestId('add-user-team-select')).toBeInTheDocument()
    })

    it('hides team dropdown when Commissioner role selected', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))

      // Change role to Commissioner
      await user.selectOptions(screen.getByTestId('add-user-role-select'), 'Commissioner')

      // Team select should be hidden
      expect(screen.queryByTestId('add-user-team-select')).not.toBeInTheDocument()
    })

    it('closes modal when clicking cancel', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))
      expect(screen.getByTestId('add-user-modal')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: 'Cancel' }))
      expect(screen.queryByTestId('add-user-modal')).not.toBeInTheDocument()
    })

    it('shows error for invalid user ID', async () => {
      const user = userEvent.setup()
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByTestId('add-user-button')).toBeInTheDocument()
      })

      await user.click(screen.getByTestId('add-user-button'))

      // Try to submit without user ID
      await user.click(screen.getByTestId('submit-add-user'))

      await waitFor(() => {
        expect(screen.getByTestId('action-error')).toHaveTextContent('Please enter a valid User ID')
      })
    })
  })

  describe('League Members Section', () => {
    it('displays league members heading', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'League Members' })).toBeInTheDocument()
      })
    })

    it('displays user in members list', async () => {
      render(<LeagueDetailPage />)

      await waitFor(() => {
        // mockUser is returned from the league users endpoint
        expect(screen.getByText('Test User')).toBeInTheDocument()
        expect(screen.getByText('testuser@example.com')).toBeInTheDocument()
      })
    })
  })
})
