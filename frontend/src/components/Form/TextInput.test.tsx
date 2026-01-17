import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { TextInput } from './TextInput'

describe('TextInput', () => {
  it('renders without crashing', () => {
    render(<TextInput />)
    expect(screen.getByRole('textbox')).toBeInTheDocument()
  })

  it('displays label when provided', () => {
    render(<TextInput label="Email Address" />)
    expect(screen.getByText('Email Address')).toBeInTheDocument()
  })

  it('shows required indicator when required', () => {
    render(<TextInput label="Username" required />)
    expect(screen.getByText('*')).toBeInTheDocument()
  })

  it('displays placeholder text', () => {
    render(<TextInput placeholder="Enter your email" />)
    const input = screen.getByPlaceholderText('Enter your email')
    expect(input).toBeInTheDocument()
  })

  it('handles value changes', () => {
    const handleChange = vi.fn()
    render(<TextInput onChange={handleChange} />)
    const input = screen.getByRole('textbox')
    
    fireEvent.change(input, { target: { value: 'test@example.com' } })
    expect(handleChange).toHaveBeenCalled()
  })

  it('displays error message when error prop is provided', () => {
    render(<TextInput error="Invalid email format" />)
    expect(screen.getByText('Invalid email format')).toBeInTheDocument()
  })

  it('displays helper text when provided', () => {
    render(<TextInput helperText="We'll never share your email" />)
    expect(screen.getByText("We'll never share your email")).toBeInTheDocument()
  })

  it('disables input when disabled prop is true', () => {
    render(<TextInput disabled />)
    const input = screen.getByRole('textbox')
    expect(input).toBeDisabled()
  })

  it('sets aria-invalid when error is present', () => {
    render(<TextInput error="Error message" />)
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('aria-invalid', 'true')
  })

  it('applies custom className', () => {
    const { container } = render(<TextInput className="custom-input" />)
    expect(container.firstChild).toHaveClass('custom-input')
  })

  it('supports different input types', () => {
    const { container, rerender } = render(<TextInput type="email" />)
    expect(screen.getByRole('textbox')).toHaveAttribute('type', 'email')

    rerender(<TextInput type="password" />)
    const passwordInput = container.querySelector('input[type="password"]')
    expect(passwordInput).toHaveAttribute('type', 'password')
  })

  it('renders left icon when provided', () => {
    const { container } = render(
      <TextInput icon={<span data-testid="left-icon">📧</span>} />
    )
    expect(screen.getByTestId('left-icon')).toBeInTheDocument()
  })

  it('renders right icon when provided', () => {
    const { container } = render(
      <TextInput rightIcon={<span data-testid="right-icon">✓</span>} />
    )
    expect(screen.getByTestId('right-icon')).toBeInTheDocument()
  })

  it('supports controlled input', () => {
    const { rerender } = render(<TextInput value="initial" onChange={() => {}} />)
    const input = screen.getByRole('textbox') as HTMLInputElement
    expect(input.value).toBe('initial')

    rerender(<TextInput value="updated" onChange={() => {}} />)
    expect(input.value).toBe('updated')
  })
})
