import { describe, it, expect } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { TeamsPage } from '../TeamsPage'

// Create a new QueryClient for each test
const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

describe('TeamsPage', () => {
  it('shows loading state initially', () => {
    const queryClient = createTestQueryClient()

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    // Should show loading spinner with animation class
    expect(container.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('displays teams after loading', async () => {
    const queryClient = createTestQueryClient()

    render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    // Wait for teams to load (MSW will mock the API response)
    await waitFor(() => {
      expect(screen.getByText('Atlanta Falcons')).toBeInTheDocument()
      expect(screen.getByText('Philadelphia Eagles')).toBeInTheDocument()
    })
  })

  it('displays team records', async () => {
    const queryClient = createTestQueryClient()

    render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    await waitFor(() => {
      expect(screen.getByText('10-6-0')).toBeInTheDocument()
      expect(screen.getByText('12-4-0')).toBeInTheDocument()
    })
  })

  it('displays team count in header', async () => {
    const queryClient = createTestQueryClient()

    render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    await waitFor(() => {
      expect(screen.getByText('2 teams in the league')).toBeInTheDocument()
    })
  })

  it('displays page title', async () => {
    const queryClient = createTestQueryClient()

    render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    // Wait for page to load and title to appear
    expect(await screen.findByText('Teams')).toBeInTheDocument()
  })
})
