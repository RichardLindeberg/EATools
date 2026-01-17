import React from 'react'
import './ProgressBar.css'

export type ProgressBarVariant = 'default' | 'success' | 'warning' | 'danger'
export type ProgressBarSize = 'sm' | 'md' | 'lg'

export interface ProgressBarProps {
  /**
   * Current progress value (0-100)
   */
  value: number

  /**
   * Maximum value (default: 100)
   */
  max?: number

  /**
   * Progress bar variant
   */
  variant?: ProgressBarVariant

  /**
   * Progress bar size
   */
  size?: ProgressBarSize

  /**
   * Show percentage label
   */
  showLabel?: boolean

  /**
   * Custom label text
   */
  label?: string

  /**
   * Indeterminate/loading state
   */
  indeterminate?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * ProgressBar Component
 *
 * Visual indicator of progress or loading state.
 * Supports determinate and indeterminate modes.
 *
 * @example
 * <ProgressBar value={75} showLabel />
 * <ProgressBar indeterminate variant="primary" />
 */
export const ProgressBar: React.FC<ProgressBarProps> = ({
  value,
  max = 100,
  variant = 'default',
  size = 'md',
  showLabel = false,
  label,
  indeterminate = false,
  className,
}) => {
  const percentage = Math.min(Math.max((value / max) * 100, 0), 100)

  const progressClasses = [
    'progress',
    `progress--${variant}`,
    `progress--${size}`,
    indeterminate && 'progress--indeterminate',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const displayLabel = label || (showLabel ? `${Math.round(percentage)}%` : '')

  return (
    <div className={progressClasses}>
      <div
        className="progress__track"
        role="progressbar"
        aria-valuenow={indeterminate ? undefined : value}
        aria-valuemin={0}
        aria-valuemax={max}
        aria-label={displayLabel}
      >
        <div
          className="progress__bar"
          style={indeterminate ? undefined : { width: `${percentage}%` }}
        />
      </div>
      {displayLabel && (
        <div className="progress__label">{displayLabel}</div>
      )}
    </div>
  )
}

export default ProgressBar
