import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Loading } from '../Loading'

describe('Loading', () => {
  it('renders loading spinner', () => {
    const { container } = render(<Loading />)

    // Check for the spinner element with animation class
    const spinner = container.querySelector('.animate-spin')
    expect(spinner).toBeInTheDocument()
  })

  it('has correct styling classes', () => {
    const { container } = render(<Loading />)

    const spinner = container.querySelector('.animate-spin')
    expect(spinner).toHaveClass('rounded-full', 'h-12', 'w-12', 'border-b-2', 'border-gridiron-primary')
  })
})
