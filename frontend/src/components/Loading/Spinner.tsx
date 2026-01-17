import React from 'react'
import './Spinner.css'

export type SpinnerSize = 'sm' | 'md' | 'lg' | 'xl'

export interface SpinnerProps {
  /**
   * Size of the spinner
   */
  size?: SpinnerSize

  /**
   * Custom color
   */
  color?: string

  /**
   * Additional CSS class
   */
  className?: string

  /**
   * Label for screen readers
   */
  label?: string
}

/**
 * Spinner Component
 *
 * A loading spinner with customizable size and color.
 * Accessible with proper ARIA labels.
 *
 * @example
 * <Spinner size="lg" label="Loading data..." />
 */
export const Spinner: React.FC<SpinnerProps> = ({
  size = 'md',
  color,
  className,
  label = 'Loading...',
}) => {
  const spinnerClasses = ['spinner', `spinner--${size}`, className]
    .filter(Boolean)
    .join(' ')

  return (
    <div
      className={spinnerClasses}
      role="status"
      aria-label={label}
      style={color ? { borderTopColor: color } : undefined}
    >
      <span className="spinner__label">{label}</span>
    </div>
  )
}

export default Spinner
