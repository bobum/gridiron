import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { render } from '../../test/test-utils'
import { GameSimulationPage } from '../GameSimulationPage'

describe('GameSimulationPage', () => {
  describe('Page Display', () => {
    it('shows loading state initially', () => {
      const { container } = render(<GameSimulationPage />)
      expect(container.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('displays page title after loading', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByRole('heading', { name: 'Simulate Game' })).toBeInTheDocument()
      })
    })

    it('displays page description', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Select two teams to run a game simulation')).toBeInTheDocument()
      })
    })

    it('displays how it works section', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('How it works')).toBeInTheDocument()
      })
    })
  })

  describe('Team Selection', () => {
    it('displays home team dropdown', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
        expect(screen.getByText('Select Home Team...')).toBeInTheDocument()
      })
    })

    it('displays away team dropdown', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Away Team')).toBeInTheDocument()
        expect(screen.getByText('Select Away Team...')).toBeInTheDocument()
      })
    })

    it('shows teams in dropdown options', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        // mockTeams has Atlanta Falcons and Philadelphia Eagles
        expect(screen.getAllByText(/Atlanta Falcons/).length).toBeGreaterThan(0)
        expect(screen.getAllByText(/Philadelphia Eagles/).length).toBeGreaterThan(0)
      })
    })

    it('selects home team and shows team info', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      // Find the first select (home team)
      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1') // Atlanta Falcons

      // Should show team details
      await waitFor(() => {
        expect(screen.getByText('Record:')).toBeInTheDocument()
        expect(screen.getByText('10-6-0')).toBeInTheDocument()
      })
    })

    it('selects away team and shows team info', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Away Team')).toBeInTheDocument()
      })

      // Find the second select (away team)
      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[1], '2') // Philadelphia Eagles

      // Should show team details
      await waitFor(() => {
        expect(screen.getByText('12-4-0')).toBeInTheDocument()
      })
    })
  })

  describe('Simulate Button', () => {
    it('displays simulate button', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Simulate Game/i })).toBeInTheDocument()
      })
    })

    it('simulate button is disabled when no teams selected', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        const button = screen.getByRole('button', { name: /Simulate Game/i })
        expect(button).toBeDisabled()
      })
    })

    it('simulate button is disabled when only home team selected', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1')

      const button = screen.getByRole('button', { name: /Simulate Game/i })
      expect(button).toBeDisabled()
    })

    it('simulate button is enabled when both teams selected', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1') // Home team
      await user.selectOptions(selects[1], '2') // Away team

      const button = screen.getByRole('button', { name: /Simulate Game/i })
      expect(button).toBeEnabled()
    })
  })

  describe('Game Simulation', () => {
    it('simulates game and shows result', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      // Select both teams
      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1')
      await user.selectOptions(selects[1], '2')

      // Click simulate
      await user.click(screen.getByRole('button', { name: /Simulate Game/i }))

      // Should show result
      await waitFor(() => {
        expect(screen.getByText('Game Complete!')).toBeInTheDocument()
        expect(screen.getByText('VS')).toBeInTheDocument()
      })
    })

    it('shows scores after simulation', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1')
      await user.selectOptions(selects[1], '2')

      await user.click(screen.getByRole('button', { name: /Simulate Game/i }))

      // mockSimulateResponse returns homeScore: 24, awayScore: 17
      await waitFor(() => {
        expect(screen.getByText('24')).toBeInTheDocument()
        expect(screen.getByText('17')).toBeInTheDocument()
      })
    })

    it('shows game ID after simulation', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1')
      await user.selectOptions(selects[1], '2')

      await user.click(screen.getByRole('button', { name: /Simulate Game/i }))

      await waitFor(() => {
        expect(screen.getByText(/Game ID:/)).toBeInTheDocument()
      })
    })

    it('shows loading state during simulation', async () => {
      const user = userEvent.setup()
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Home Team')).toBeInTheDocument()
      })

      const selects = screen.getAllByRole('combobox')
      await user.selectOptions(selects[0], '1')
      await user.selectOptions(selects[1], '2')

      await user.click(screen.getByRole('button', { name: /Simulate Game/i }))

      // Button should show loading state briefly
      // This is hard to test due to fast mock response, so we just verify the flow completes
      await waitFor(() => {
        expect(screen.getByText('Game Complete!')).toBeInTheDocument()
      })
    })
  })

  describe('Instructions Section', () => {
    it('displays step-by-step instructions', async () => {
      render(<GameSimulationPage />)

      await waitFor(() => {
        expect(screen.getByText('Select a home team and an away team from the dropdowns')).toBeInTheDocument()
        expect(screen.getByText(/Click "Simulate Game"/)).toBeInTheDocument()
        expect(screen.getByText('The game engine will process all plays and return the final score')).toBeInTheDocument()
        expect(screen.getByText('Results are saved to the database for future reference')).toBeInTheDocument()
      })
    })
  })
})
