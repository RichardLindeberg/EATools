import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Spinner } from './Spinner'

describe('Spinner', () => {
  it('renders without crashing', () => {
    const { container } = render(<Spinner />)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('has proper ARIA attributes', () => {
    render(<Spinner />)
    const spinner = screen.getByRole('status')
    expect(spinner).toHaveAttribute('aria-label', 'Loading...')
  })

  it('displays custom aria-label when provided', () => {
    render(<Spinner label="Loading data..." />)
    const spinner = screen.getByRole('status')
    expect(spinner).toHaveAttribute('aria-label', 'Loading data...')
  })

  it('applies size classes correctly', () => {
    const { container, rerender } = render(<Spinner size="sm" />)
    expect(container.querySelector('.spinner--sm')).toBeInTheDocument()

    rerender(<Spinner size="md" />)
    expect(container.querySelector('.spinner--md')).toBeInTheDocument()

    rerender(<Spinner size="lg" />)
    expect(container.querySelector('.spinner--lg')).toBeInTheDocument()

    rerender(<Spinner size="xl" />)
    expect(container.querySelector('.spinner--xl')).toBeInTheDocument()
  })

  it('applies default size when not specified', () => {
    const { container } = render(<Spinner />)
    expect(container.querySelector('.spinner--md')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Spinner className="custom-spinner" />)
    expect(container.firstChild).toHaveClass('custom-spinner')
  })
})
