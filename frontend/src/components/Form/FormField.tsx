import React, { ReactNode } from 'react'
import './FormField.css'

export interface FormFieldProps {
  /**
   * Field label
   */
  label?: string

  /**
   * Help text
   */
  helperText?: string

  /**
   * Error message
   */
  error?: string

  /**
   * Required field indicator
   */
  required?: boolean

  /**
   * Field ID for label association
   */
  htmlFor?: string

  /**
   * Form field children (input, select, etc.)
   */
  children: ReactNode

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * FormField Component
 *
 * Wrapper component for form inputs that provides consistent
 * label, error, and help text layout.
 *
 * @example
 * <FormField 
 *   label="Email" 
 *   required 
 *   error={errors.email}
 *   helperText="We'll never share your email"
 * >
 *   <TextInput {...register('email')} />
 * </FormField>
 */
export const FormField: React.FC<FormFieldProps> = ({
  label,
  helperText,
  error,
  required,
  htmlFor,
  children,
  className,
}) => {
  const hasError = !!error

  const fieldClasses = [
    'form-field',
    hasError && 'form-field--error',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={fieldClasses}>
      {label && (
        <label htmlFor={htmlFor} className="form-field__label">
          {label}
          {required && <span className="form-field__required">*</span>}
        </label>
      )}

      <div className="form-field__control">{children}</div>

      {(error || helperText) && (
        <div
          className={`form-field__message ${
            hasError ? 'form-field__message--error' : ''
          }`}
        >
          {error || helperText}
        </div>
      )}
    </div>
  )
}

export default FormField
