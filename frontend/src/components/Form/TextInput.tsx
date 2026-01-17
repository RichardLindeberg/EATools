import React, { InputHTMLAttributes, ReactNode } from 'react'
import './TextInput.css'

export interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * Label text
   */
  label?: string

  /**
   * Error message
   */
  error?: string

  /**
   * Help text
   */
  helperText?: string

  /**
   * Left icon/addon
   */
  icon?: ReactNode

  /**
   * Right icon/addon
   */
  rightIcon?: ReactNode

  /**
   * Make field required
   */
  required?: boolean

  /**
   * Full width input
   */
  fullWidth?: boolean
}

/**
 * TextInput Component
 *
 * A controlled text input component with label, validation, and helper text.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <TextInput label="Email" type="email" placeholder="Enter email" />
 * <TextInput label="Password" type="password" error="Invalid password" />
 */
export const TextInput = React.forwardRef<HTMLInputElement, TextInputProps>(
  (
    {
      label,
      error,
      helperText,
      icon,
      rightIcon,
      required,
      fullWidth,
      className,
      id,
      disabled,
      ...props
    },
    ref
  ) => {
    const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`
    const hasError = !!error

    const containerClasses = [
      'text-input',
      fullWidth && 'text-input--full-width',
      disabled && 'text-input--disabled',
      hasError && 'text-input--error',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <div className={containerClasses}>
        {label && (
          <label htmlFor={inputId} className="text-input__label">
            {label}
            {required && <span className="text-input__required">*</span>}
          </label>
        )}

        <div className="text-input__wrapper">
          {icon && <span className="text-input__icon text-input__icon--left">{icon}</span>}

          <input
            ref={ref}
            id={inputId}
            className="text-input__input"
            disabled={disabled}
            aria-invalid={hasError}
            aria-describedby={error || helperText ? `${inputId}-description` : undefined}
            {...props}
          />

          {rightIcon && (
            <span className="text-input__icon text-input__icon--right">{rightIcon}</span>
          )}
        </div>

        {(error || helperText) && (
          <div
            id={`${inputId}-description`}
            className={`text-input__description ${hasError ? 'text-input__description--error' : ''}`}
          >
            {error || helperText}
          </div>
        )}
      </div>
    )
  }
)

TextInput.displayName = 'TextInput'

export default TextInput
