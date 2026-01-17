import React, { ReactNode } from 'react'
import './Alert.css'

export type AlertVariant = 'info' | 'success' | 'warning' | 'danger'

export interface AlertProps {
  /**
   * Alert variant
   */
  variant?: AlertVariant

  /**
   * Alert title
   */
  title?: string

  /**
   * Alert message
   */
  children: ReactNode

  /**
   * Show close button
   */
  dismissible?: boolean

  /**
   * Close handler
   */
  onClose?: () => void

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Alert Component
 *
 * An alert message box with different variants for various use cases.
 * Accessible with proper ARIA roles.
 *
 * @example
 * <Alert variant="success" title="Success!" dismissible onClose={handleClose}>
 *   Your changes have been saved.
 * </Alert>
 */
export const Alert: React.FC<AlertProps> = ({
  variant = 'info',
  title,
  children,
  dismissible = false,
  onClose,
  className,
}) => {
  const alertClasses = ['alert', `alert--${variant}`, className]
    .filter(Boolean)
    .join(' ')

  const getIcon = () => {
    switch (variant) {
      case 'success':
        return '✓'
      case 'warning':
        return '⚠'
      case 'danger':
        return '✕'
      default:
        return 'ℹ'
    }
  }

  const getRole = () => {
    return variant === 'danger' || variant === 'warning' ? 'alert' : 'status'
  }

  return (
    <div className={alertClasses} role={getRole()}>
      <div className="alert__icon" aria-hidden="true">
        {getIcon()}
      </div>
      
      <div className="alert__content">
        {title && <div className="alert__title">{title}</div>}
        <div className="alert__message">{children}</div>
      </div>

      {dismissible && onClose && (
        <button
          type="button"
          className="alert__close"
          onClick={onClose}
          aria-label="Close alert"
        >
          ×
        </button>
      )}
    </div>
  )
}

export default Alert
