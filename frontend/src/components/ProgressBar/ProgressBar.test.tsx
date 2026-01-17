import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ProgressBar } from './ProgressBar'

describe('ProgressBar', () => {
  it('renders without crashing', () => {
    const { container } = render(<ProgressBar value={50} />)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('displays progress value', () => {
    render(<ProgressBar value={75} />)
    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).toHaveAttribute('aria-valuenow', '75')
  })

  it('shows percentage label when showLabel is true', () => {
    render(<ProgressBar value={60} showLabel />)
    expect(screen.getByText('60%')).toBeInTheDocument()
  })

  it('hides label by default', () => {
    render(<ProgressBar value={60} />)
    expect(screen.queryByText('60%')).not.toBeInTheDocument()
  })

  it('displays custom label when provided', () => {
    render(<ProgressBar value={50} label="Loading files..." />)
    expect(screen.getByText('Loading files...')).toBeInTheDocument()
  })

  it('applies variant classes correctly', () => {
    const { container, rerender } = render(<ProgressBar value={50} variant="success" />)
    expect(container.querySelector('.progress--success')).toBeInTheDocument()

    rerender(<ProgressBar value={50} variant="warning" />)
    expect(container.querySelector('.progress--warning')).toBeInTheDocument()

    rerender(<ProgressBar value={50} variant="danger" />)
    expect(container.querySelector('.progress--danger')).toBeInTheDocument()
  })

  it('applies size classes correctly', () => {
    const { container, rerender } = render(<ProgressBar value={50} size="sm" />)
    expect(container.querySelector('.progress--sm')).toBeInTheDocument()

    rerender(<ProgressBar value={50} size="md" />)
    expect(container.querySelector('.progress--md')).toBeInTheDocument()

    rerender(<ProgressBar value={50} size="lg" />)
    expect(container.querySelector('.progress--lg')).toBeInTheDocument()
  })

  it('handles indeterminate mode', () => {
    const { container } = render(<ProgressBar value={0} indeterminate />)
    expect(container.querySelector('.progress--indeterminate')).toBeInTheDocument()
  })

  it('clamps display width between 0 and 100', () => {
    // Value > 100 should display as 100% width
    const { container, rerender } = render(<ProgressBar value={150} />)
    let bar = container.querySelector('.progress__bar')
    expect(bar).toHaveStyle({ width: '100%' })

    rerender(<ProgressBar value={-10} />)
    bar = container.querySelector('.progress__bar')
    expect(bar).toHaveStyle({ width: '0%' })
  })

  it('has proper ARIA attributes', () => {
    render(<ProgressBar value={45} />)
    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).toHaveAttribute('aria-valuemin', '0')
    expect(progressBar).toHaveAttribute('aria-valuemax', '100')
    expect(progressBar).toHaveAttribute('aria-valuenow', '45')
  })

  it('applies custom className', () => {
    const { container } = render(<ProgressBar value={50} className="custom-progress" />)
    expect(container.firstChild).toHaveClass('custom-progress')
  })

  it('sets correct bar width based on value', () => {
    const { container } = render(<ProgressBar value={75} />)
    const bar = container.querySelector('.progress__bar')
    expect(bar).toHaveStyle({ width: '75%' })
  })

  it('does not show aria-valuenow in indeterminate mode', () => {
    render(<ProgressBar indeterminate />)
    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).not.toHaveAttribute('aria-valuenow')
  })
})
