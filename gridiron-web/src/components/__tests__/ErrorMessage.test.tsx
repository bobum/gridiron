import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ErrorMessage } from '../ErrorMessage'

describe('ErrorMessage', () => {
  it('renders default error message when no message provided', () => {
    render(<ErrorMessage />)

    expect(screen.getByText('An error occurred')).toBeInTheDocument()
  })

  it('renders custom error message', () => {
    render(<ErrorMessage message="Custom error message" />)

    expect(screen.getByText('Custom error message')).toBeInTheDocument()
  })

  it('displays error icon', () => {
    const { container } = render(<ErrorMessage />)

    // Check for SVG icon
    const svg = container.querySelector('svg')
    expect(svg).toBeInTheDocument()
    expect(svg).toHaveClass('text-red-600')
  })

  it('has error styling classes', () => {
    const { container } = render(<ErrorMessage />)

    const errorContainer = container.querySelector('.bg-red-50')
    expect(errorContainer).toBeInTheDocument()
    expect(errorContainer).toHaveClass('border', 'border-red-200', 'rounded-lg')
  })
})
