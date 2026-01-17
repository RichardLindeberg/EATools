import React, { ReactNode } from 'react'
import './Badge.css'

export type BadgeVariant = 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'info'
export type BadgeSize = 'sm' | 'md' | 'lg'

export interface BadgeProps {
  /**
   * Badge content
   */
  children: ReactNode

  /**
   * Badge variant
   */
  variant?: BadgeVariant

  /**
   * Badge size
   */
  size?: BadgeSize

  /**
   * Display as a dot indicator
   */
  dot?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Badge Component
 *
 * Small status indicator or label with various styles.
 * Used for tags, counts, and status indicators.
 *
 * @example
 * <Badge variant="success">Active</Badge>
 * <Badge variant="danger" size="sm" dot />
 */
export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  size = 'md',
  dot = false,
  className,
}) => {
  const badgeClasses = [
    'badge',
    `badge--${variant}`,
    `badge--${size}`,
    dot && 'badge--dot',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return <span className={badgeClasses}>{children}</span>
}

export default Badge
