import React from 'react'
import './Skeleton.css'

export type SkeletonVariant = 'text' | 'rectangular' | 'circular'

export interface SkeletonProps {
  /**
   * Skeleton variant
   */
  variant?: SkeletonVariant

  /**
   * Width of the skeleton
   */
  width?: string | number

  /**
   * Height of the skeleton
   */
  height?: string | number

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Skeleton Component
 *
 * A loading placeholder component that displays an animated skeleton
 * while content is being loaded.
 *
 * @example
 * <Skeleton variant="text" width="200px" />
 * <Skeleton variant="circular" width="40px" height="40px" />
 * <Skeleton variant="rectangular" width="100%" height="200px" />
 */
export const Skeleton: React.FC<SkeletonProps> = ({
  variant = 'text',
  width,
  height,
  className,
}) => {
  const skeletonClasses = ['skeleton', `skeleton--${variant}`, className]
    .filter(Boolean)
    .join(' ')

  const style: React.CSSProperties = {}
  if (width) {
    style.width = typeof width === 'number' ? `${width}px` : width
  }
  if (height) {
    style.height = typeof height === 'number' ? `${height}px` : height
  } else if (variant === 'text') {
    style.height = '1em'
  }

  return <div className={skeletonClasses} style={style} aria-hidden="true" />
}

export default Skeleton
