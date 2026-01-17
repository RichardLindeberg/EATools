import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Alert } from './Alert'

describe('Alert', () => {
  it('renders without crashing', () => {
    render(<Alert>Alert message</Alert>)
    expect(screen.getByText('Alert message')).toBeInTheDocument()
  })

  it('applies default variant class', () => {
    const { container } = render(<Alert>Default</Alert>)
    expect(container.firstChild).toHaveClass('alert--info')
  })

  it('applies variant classes correctly', () => {
    const { container, rerender } = render(<Alert variant="success">Success</Alert>)
    expect(container.firstChild).toHaveClass('alert--success')

    rerender(<Alert variant="warning">Warning</Alert>)
    expect(container.firstChild).toHaveClass('alert--warning')

    rerender(<Alert variant="danger">Danger</Alert>)
    expect(container.firstChild).toHaveClass('alert--danger')

    rerender(<Alert variant="info">Info</Alert>)
    expect(container.firstChild).toHaveClass('alert--info')
  })

  it('displays title when provided', () => {
    render(<Alert title="Important Notice">Message</Alert>)
    expect(screen.getByText('Important Notice')).toBeInTheDocument()
  })

  it('shows dismiss button when dismissible', () => {
    render(<Alert dismissible onClose={() => {}}>Dismissible alert</Alert>)
    expect(screen.getByLabelText('Close alert')).toBeInTheDocument()
  })

  it('calls onClose when dismiss button is clicked', () => {
    const handleClose = vi.fn()
    render(
      <Alert dismissible onClose={handleClose}>
        Dismissible alert
      </Alert>
    )
    
    const dismissButton = screen.getByLabelText('Close alert')
    fireEvent.click(dismissButton)
    expect(handleClose).toHaveBeenCalled()
  })

  it('does not show dismiss button when not dismissible', () => {
    render(<Alert>Non-dismissible alert</Alert>)
    expect(screen.queryByLabelText('Dismiss alert')).not.toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Alert className="custom-alert">Alert</Alert>)
    expect(container.firstChild).toHaveClass('custom-alert')
  })

  it('has proper ARIA role', () => {
    render(<Alert>Alert message</Alert>)
    const alert = screen.getByRole('status')
    expect(alert).toBeInTheDocument()
  })

  it('renders children content', () => {
    render(
      <Alert>
        <p>Paragraph 1</p>
        <p>Paragraph 2</p>
      </Alert>
    )
    expect(screen.getByText('Paragraph 1')).toBeInTheDocument()
    expect(screen.getByText('Paragraph 2')).toBeInTheDocument()
  })
})
