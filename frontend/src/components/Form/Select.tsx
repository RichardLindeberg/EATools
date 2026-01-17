import React, { SelectHTMLAttributes, ReactNode } from 'react'
import './Select.css'

export interface SelectOption {
  value: string | number
  label: string
  disabled?: boolean
}

export interface SelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'size'> {
  /**
   * Label text
   */
  label?: string

  /**
   * Select options
   */
  options: SelectOption[]

  /**
   * Error message
   */
  error?: string

  /**
   * Help text
   */
  helperText?: string

  /**
   * Placeholder text
   */
  placeholder?: string

  /**
   * Make field required
   */
  required?: boolean

  /**
   * Full width select
   */
  fullWidth?: boolean
}

/**
 * Select Component
 *
 * A styled select dropdown with label, validation, and helper text.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <Select 
 *   label="Country" 
 *   options={[{value: 'us', label: 'United States'}]}
 *   placeholder="Select country"
 * />
 */
export const Select = React.forwardRef<HTMLSelectElement, SelectProps>(
  (
    {
      label,
      options,
      error,
      helperText,
      placeholder,
      required,
      fullWidth,
      className,
      id,
      disabled,
      ...props
    },
    ref
  ) => {
    const selectId = id || `select-${Math.random().toString(36).substr(2, 9)}`
    const hasError = !!error

    const containerClasses = [
      'select',
      fullWidth && 'select--full-width',
      disabled && 'select--disabled',
      hasError && 'select--error',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <div className={containerClasses}>
        {label && (
          <label htmlFor={selectId} className="select__label">
            {label}
            {required && <span className="select__required">*</span>}
          </label>
        )}

        <div className="select__wrapper">
          <select
            ref={ref}
            id={selectId}
            className="select__input"
            disabled={disabled}
            aria-invalid={hasError}
            aria-describedby={error || helperText ? `${selectId}-description` : undefined}
            {...props}
          >
            {placeholder && (
              <option value="" disabled>
                {placeholder}
              </option>
            )}
            {options.map((option) => (
              <option
                key={option.value}
                value={option.value}
                disabled={option.disabled}
              >
                {option.label}
              </option>
            ))}
          </select>
          
          <span className="select__arrow" aria-hidden="true">
            ▼
          </span>
        </div>

        {(error || helperText) && (
          <div
            id={`${selectId}-description`}
            className={`select__description ${hasError ? 'select__description--error' : ''}`}
          >
            {error || helperText}
          </div>
        )}
      </div>
    )
  }
)

Select.displayName = 'Select'

export default Select
