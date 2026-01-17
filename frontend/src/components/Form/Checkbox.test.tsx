import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Checkbox } from './Checkbox'

describe('Checkbox', () => {
  it('renders without crashing', () => {
    render(<Checkbox label="Test Checkbox" />)
    expect(screen.getByRole('checkbox')).toBeInTheDocument()
  })

  it('displays label text', () => {
    render(<Checkbox label="Accept Terms" />)
    expect(screen.getByText('Accept Terms')).toBeInTheDocument()
  })

  it('handles checked state', () => {
    const { rerender } = render(<Checkbox label="Test" checked={false} onChange={() => {}} />)
    const checkbox = screen.getByRole('checkbox') as HTMLInputElement
    expect(checkbox.checked).toBe(false)

    rerender(<Checkbox label="Test" checked={true} onChange={() => {}} />)
    expect(checkbox.checked).toBe(true)
  })

  it('calls onChange when clicked', () => {
    const handleChange = vi.fn()
    render(<Checkbox label="Test" onChange={handleChange} />)
    const checkbox = screen.getByRole('checkbox')
    
    fireEvent.click(checkbox)
    expect(handleChange).toHaveBeenCalled()
  })

  it('displays error message when error prop is provided', () => {
    render(<Checkbox label="Test" error="This field is required" />)
    expect(screen.getByText('This field is required')).toBeInTheDocument()
  })

  it('disables checkbox when disabled prop is true', () => {
    render(<Checkbox label="Test" disabled />)
    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).toBeDisabled()
  })

  it('sets aria-invalid when error is present', () => {
    render(<Checkbox label="Test" error="Error message" />)
    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).toHaveAttribute('aria-invalid', 'true')
  })

  it('applies custom className', () => {
    const { container } = render(<Checkbox label="Test" className="custom-class" />)
    expect(container.firstChild).toHaveClass('custom-class')
  })

  it('sets indeterminate state', () => {
    const { container } = render(<Checkbox label="Test" indeterminate />)
    const checkbox = container.querySelector('input[type="checkbox"]') as HTMLInputElement
    expect(checkbox.indeterminate).toBe(true)
  })
})
