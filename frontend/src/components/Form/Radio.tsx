import React, { InputHTMLAttributes } from 'react'
import './Radio.css'

export interface RadioOption {
  value: string | number
  label: string
  disabled?: boolean
}

export interface RadioProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  /**
   * Radio group name
   */
  name: string

  /**
   * Radio options
   */
  options: RadioOption[]

  /**
   * Selected value
   */
  value?: string | number

  /**
   * Change handler
   */
  onChange?: (value: string | number) => void

  /**
   * Error message
   */
  error?: string

  /**
   * Group label
   */
  label?: string

  /**
   * Layout direction
   */
  direction?: 'horizontal' | 'vertical'
}

/**
 * Radio Component
 *
 * A styled radio button group with label and validation.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <Radio 
 *   name="payment" 
 *   label="Payment Method"
 *   options={[{value: 'card', label: 'Credit Card'}, {value: 'paypal', label: 'PayPal'}]}
 *   value={selected}
 *   onChange={setSelected}
 * />
 */
export const Radio: React.FC<RadioProps> = ({
  name,
  options,
  value,
  onChange,
  error,
  label,
  direction = 'vertical',
  className,
  disabled,
}) => {
  const groupId = `radio-group-${Math.random().toString(36).substr(2, 9)}`
  const hasError = !!error

  const handleChange = (optionValue: string | number) => {
    if (onChange && !disabled) {
      onChange(optionValue)
    }
  }

  const containerClasses = [
    'radio-group',
    `radio-group--${direction}`,
    hasError && 'radio-group--error',
    disabled && 'radio-group--disabled',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={containerClasses}>
      {label && (
        <div className="radio-group__label" id={groupId}>
          {label}
        </div>
      )}

      <div className="radio-group__options" role="radiogroup" aria-labelledby={label ? groupId : undefined}>
        {options.map((option) => {
          const optionId = `${name}-${option.value}`
          const isChecked = value === option.value
          const isDisabled = disabled || option.disabled

          return (
            <label key={option.value} className="radio-option" htmlFor={optionId}>
              <input
                type="radio"
                id={optionId}
                name={name}
                value={option.value}
                checked={isChecked}
                disabled={isDisabled}
                onChange={() => handleChange(option.value)}
                className="radio-option__input"
                aria-invalid={hasError}
              />
              <span className="radio-option__label">{option.label}</span>
            </label>
          )
        })}
      </div>

      {error && (
        <div className="radio-group__error" role="alert">
          {error}
        </div>
      )}
    </div>
  )
}

export default Radio
