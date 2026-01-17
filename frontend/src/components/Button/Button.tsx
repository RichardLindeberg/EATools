import React, { ButtonHTMLAttributes, ReactNode } from 'react'
import './Button.css'

export type ButtonVariant = 'primary' | 'secondary' | 'tertiary' | 'danger' | 'ghost'
export type ButtonSize = 'sm' | 'md' | 'lg'

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * Visual style variant
   * @default 'primary'
   */
  variant?: ButtonVariant

  /**
   * Button size
   * @default 'md'
   */
  size?: ButtonSize

  /**
   * Full width button
   * @default false
   */
  fullWidth?: boolean

  /**
   * Disabled state
   */
  disabled?: boolean

  /**
   * Loading state with spinner
   */
  isLoading?: boolean

  /**
   * Button content
   */
  children: ReactNode

  /**
   * Optional icon component
   */
  icon?: ReactNode

  /**
   * Icon position (left or right)
   */
  iconPosition?: 'left' | 'right'

  /**
   * Aria label for accessibility
   */
  ariaLabel?: string
}

/**
 * Button Component
 *
 * A versatile button component supporting multiple variants and sizes.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <Button variant="primary">Click me</Button>
 * <Button variant="danger" size="lg">Delete</Button>
 * <Button disabled>Disabled</Button>
 */
export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'md',
      fullWidth = false,
      disabled = false,
      isLoading = false,
      children,
      icon,
      iconPosition = 'left',
      className,
      ariaLabel,
      ...props
    },
    ref
  ) => {
    const buttonClasses = [
      'button',
      `button--${variant}`,
      `button--${size}`,
      fullWidth && 'button--full-width',
      disabled || isLoading ? 'button--disabled' : '',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <button
        ref={ref}
        className={buttonClasses}
        disabled={disabled || isLoading}
        aria-label={ariaLabel}
        aria-busy={isLoading}
        {...props}
      >
        {isLoading && <span className="button__spinner" />}

        {icon && iconPosition === 'left' && (
          <span className="button__icon button__icon--left">{icon}</span>
        )}

        <span className="button__text">{children}</span>

        {icon && iconPosition === 'right' && (
          <span className="button__icon button__icon--right">{icon}</span>
        )}
      </button>
    )
  }
)

Button.displayName = 'Button'

export default Button
