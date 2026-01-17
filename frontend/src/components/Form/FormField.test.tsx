import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { FormField } from './FormField'
import { TextInput } from './TextInput'

describe('FormField', () => {
  it('renders without crashing', () => {
    render(
      <FormField label="Test Field">
        <input type="text" />
      </FormField>
    )
    expect(screen.getByText('Test Field')).toBeInTheDocument()
  })

  it('displays label text', () => {
    render(
      <FormField label="Email Address">
        <TextInput />
      </FormField>
    )
    expect(screen.getByText('Email Address')).toBeInTheDocument()
  })

  it('shows required indicator when required', () => {
    render(
      <FormField label="Required Field" required>
        <TextInput />
      </FormField>
    )
    expect(screen.getByText('*')).toBeInTheDocument()
  })

  it('displays error message when error prop is provided', () => {
    render(
      <FormField label="Field" error="This field is required">
        <TextInput />
      </FormField>
    )
    expect(screen.getByText('This field is required')).toBeInTheDocument()
  })

  it('displays helper text when provided', () => {
    render(
      <FormField label="Field" helperText="Enter your full name">
        <TextInput />
      </FormField>
    )
    expect(screen.getByText('Enter your full name')).toBeInTheDocument()
  })

  it('renders children components', () => {
    render(
      <FormField label="Test">
        <input data-testid="child-input" type="text" />
      </FormField>
    )
    expect(screen.getByTestId('child-input')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(
      <FormField label="Test" className="custom-field">
        <TextInput />
      </FormField>
    )
    expect(container.firstChild).toHaveClass('custom-field')
  })

  it('shows error styling when error is present', () => {
    const { container } = render(
      <FormField label="Test" error="Error message">
        <TextInput />
      </FormField>
    )
    expect(container.querySelector('.form-field--error')).toBeInTheDocument()
  })

  it('can wrap multiple input types', () => {
    render(
      <FormField label="Multiple">
        <TextInput />
        <button>Submit</button>
      </FormField>
    )
    expect(screen.getByRole('textbox')).toBeInTheDocument()
    expect(screen.getByRole('button')).toBeInTheDocument()
  })
})
