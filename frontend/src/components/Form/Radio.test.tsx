import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Radio } from './Radio'

describe('Radio', () => {
  const mockOptions = [
    { value: 'option1', label: 'Option 1' },
    { value: 'option2', label: 'Option 2' },
    { value: 'option3', label: 'Option 3', disabled: true },
  ]

  it('renders without crashing', () => {
    render(<Radio name="test" options={mockOptions} />)
    expect(screen.getByText('Option 1')).toBeInTheDocument()
  })

  it('displays label when provided', () => {
    render(<Radio label="Choose One" name="test" options={mockOptions} />)
    expect(screen.getByText('Choose One')).toBeInTheDocument()
  })

  it('renders all options', () => {
    render(<Radio name="test" options={mockOptions} />)
    expect(screen.getByText('Option 1')).toBeInTheDocument()
    expect(screen.getByText('Option 2')).toBeInTheDocument()
    expect(screen.getByText('Option 3')).toBeInTheDocument()
  })

  it('handles value selection', () => {
    const handleChange = vi.fn()
    render(<Radio name="test" options={mockOptions} onChange={handleChange} />)
    
    const radio2 = screen.getByLabelText('Option 2')
    fireEvent.click(radio2)
    expect(handleChange).toHaveBeenCalled()
  })

  it('shows checked state for selected value', () => {
    render(<Radio name="test" options={mockOptions} value="option2" onChange={() => {}} />)
    const radio2 = screen.getByLabelText('Option 2') as HTMLInputElement
    expect(radio2.checked).toBe(true)
  })

  it('disables individual options when disabled prop is set', () => {
    render(<Radio name="test" options={mockOptions} />)
    const radio3 = screen.getByLabelText('Option 3')
    expect(radio3).toBeDisabled()
  })

  it('displays error message when error prop is provided', () => {
    render(<Radio name="test" options={mockOptions} error="Please select an option" />)
    expect(screen.getByText('Please select an option')).toBeInTheDocument()
  })

  it('applies horizontal direction class', () => {
    const { container } = render(<Radio name="test" options={mockOptions} direction="horizontal" />)
    expect(container.querySelector('.radio-group--horizontal')).toBeInTheDocument()
  })

  it('applies vertical layout class by default', () => {
    const { container } = render(<Radio name="test" options={mockOptions} />)
    expect(container.querySelector('.radio-group--vertical')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Radio name="test" options={mockOptions} className="custom-radio" />)
    expect(container.firstChild).toHaveClass('custom-radio')
  })

  it('groups radios with same name', () => {
    render(<Radio name="group1" options={mockOptions} />)
    const radios = screen.getAllByRole('radio')
    radios.forEach(radio => {
      expect(radio).toHaveAttribute('name', 'group1')
    })
  })
})
