import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Badge } from './Badge'

describe('Badge', () => {
  it('renders without crashing', () => {
    render(<Badge>Test Badge</Badge>)
    expect(screen.getByText('Test Badge')).toBeInTheDocument()
  })

  it('applies default variant class', () => {
    const { container } = render(<Badge>Default</Badge>)
    expect(container.firstChild).toHaveClass('badge--default')
  })

  it('applies variant classes correctly', () => {
    const { container, rerender } = render(<Badge variant="success">Success</Badge>)
    expect(container.firstChild).toHaveClass('badge--success')

    rerender(<Badge variant="danger">Danger</Badge>)
    expect(container.firstChild).toHaveClass('badge--danger')

    rerender(<Badge variant="warning">Warning</Badge>)
    expect(container.firstChild).toHaveClass('badge--warning')
  })

  it('applies size classes correctly', () => {
    const { container, rerender } = render(<Badge size="sm">Small</Badge>)
    expect(container.firstChild).toHaveClass('badge--sm')

    rerender(<Badge size="lg">Large</Badge>)
    expect(container.firstChild).toHaveClass('badge--lg')
  })

  it('applies dot class when dot prop is true', () => {
    const { container } = render(<Badge dot>Dot</Badge>)
    expect(container.firstChild).toHaveClass('badge--dot')
  })

  it('applies custom className', () => {
    const { container } = render(<Badge className="custom-badge">Custom</Badge>)
    expect(container.firstChild).toHaveClass('custom-badge')
  })

  it('renders children content', () => {
    render(<Badge>Badge Content</Badge>)
    expect(screen.getByText('Badge Content')).toBeInTheDocument()
  })

  it('renders numeric content', () => {
    render(<Badge>99</Badge>)
    expect(screen.getByText('99')).toBeInTheDocument()
  })
})
