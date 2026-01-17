import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Select } from './Select'

describe('Select', () => {
  const mockOptions = [
    { value: 'option1', label: 'Option 1' },
    { value: 'option2', label: 'Option 2' },
    { value: 'option3', label: 'Option 3', disabled: true },
  ]

  it('renders without crashing', () => {
    render(<Select options={mockOptions} />)
    expect(screen.getByRole('combobox')).toBeInTheDocument()
  })

  it('displays label when provided', () => {
    render(<Select label="Test Label" options={mockOptions} />)
    expect(screen.getByText('Test Label')).toBeInTheDocument()
  })

  it('shows required indicator when required', () => {
    render(<Select label="Required Field" required options={mockOptions} />)
    expect(screen.getByText('*')).toBeInTheDocument()
  })

  it('displays placeholder when provided', () => {
    render(<Select placeholder="Choose an option" options={mockOptions} />)
    const selectElement = screen.getByRole('combobox') as HTMLSelectElement
    expect(selectElement.querySelector('option[disabled]')).toHaveTextContent('Choose an option')
  })

  it('displays error message when error prop is provided', () => {
    render(<Select options={mockOptions} error="This field is required" />)
    expect(screen.getByText('This field is required')).toBeInTheDocument()
  })

  it('displays helper text when provided', () => {
    render(<Select options={mockOptions} helperText="Select an option from the list" />)
    expect(screen.getByText('Select an option from the list')).toBeInTheDocument()
  })

  it('renders all options', () => {
    render(<Select options={mockOptions} />)
    const selectElement = screen.getByRole('combobox') as HTMLSelectElement
    expect(selectElement.options).toHaveLength(3)
  })

  it('handles value change', () => {
    const handleChange = vi.fn()
    render(<Select options={mockOptions} onChange={handleChange} />)
    const selectElement = screen.getByRole('combobox')
    
    fireEvent.change(selectElement, { target: { value: 'option2' } })
    expect(handleChange).toHaveBeenCalled()
  })

  it('disables select when disabled prop is true', () => {
    render(<Select options={mockOptions} disabled />)
    const selectElement = screen.getByRole('combobox')
    expect(selectElement).toBeDisabled()
  })

  it('applies custom className', () => {
    const { container } = render(<Select options={mockOptions} className="custom-class" />)
    expect(container.firstChild).toHaveClass('custom-class')
  })

  it('sets aria-invalid when error is present', () => {
    render(<Select options={mockOptions} error="Error message" />)
    const selectElement = screen.getByRole('combobox')
    expect(selectElement).toHaveAttribute('aria-invalid', 'true')
  })
})
