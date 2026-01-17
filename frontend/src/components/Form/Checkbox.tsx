import React, { InputHTMLAttributes } from 'react'
import './Checkbox.css'

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  /**
   * Checkbox label
   */
  label: string

  /**
   * Error message
   */
  error?: string

  /**
   * Display as indeterminate state
   */
  indeterminate?: boolean
}

/**
 * Checkbox Component
 *
 * A styled checkbox input with label and validation.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <Checkbox label="I agree to terms" checked={agreed} onChange={handleChange} />
 */
export const Checkbox = React.forwardRef<HTMLInputElement, CheckboxProps>(
  ({ label, error, indeterminate, className, id, disabled, ...props }, ref) => {
    const checkboxId = id || `checkbox-${Math.random().toString(36).substr(2, 9)}`
    const hasError = !!error

    const checkboxRef = React.useRef<HTMLInputElement>(null)
    React.useImperativeHandle(ref, () => checkboxRef.current!)

    React.useEffect(() => {
      if (checkboxRef.current) {
        checkboxRef.current.indeterminate = !!indeterminate
      }
    }, [indeterminate])

    const containerClasses = [
      'checkbox',
      hasError && 'checkbox--error',
      disabled && 'checkbox--disabled',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <div className={containerClasses}>
        <div className="checkbox__wrapper">
          <input
            ref={checkboxRef}
            type="checkbox"
            id={checkboxId}
            className="checkbox__input"
            disabled={disabled}
            aria-invalid={hasError}
            aria-describedby={error ? `${checkboxId}-error` : undefined}
            {...props}
          />
          <label htmlFor={checkboxId} className="checkbox__label">
            {label}
          </label>
        </div>

        {error && (
          <div id={`${checkboxId}-error`} className="checkbox__error">
            {error}
          </div>
        )}
      </div>
    )
  }
)

Checkbox.displayName = 'Checkbox'

export default Checkbox
