import React, { useState, useRef, useEffect, ReactNode } from 'react'
import './Tooltip.css'

export type TooltipPlacement = 'top' | 'bottom' | 'left' | 'right'

export interface TooltipProps {
  /**
   * Tooltip content
   */
  content: ReactNode

  /**
   * Tooltip placement
   */
  placement?: TooltipPlacement

  /**
   * Delay before showing (ms)
   */
  delay?: number

  /**
   * Children to wrap with tooltip
   */
  children: ReactNode

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Tooltip Component
 *
 * Displays informational tooltip on hover or focus.
 * Accessible with proper ARIA attributes.
 *
 * @example
 * <Tooltip content="This is helpful info" placement="top">
 *   <Button>Hover me</Button>
 * </Tooltip>
 */
export const Tooltip: React.FC<TooltipProps> = ({
  content,
  placement = 'top',
  delay = 200,
  children,
  className,
}) => {
  const [isVisible, setIsVisible] = useState(false)
  const timeoutRef = useRef<NodeJS.Timeout>()
  const tooltipId = useRef(`tooltip-${Math.random().toString(36).substr(2, 9)}`)

  const handleMouseEnter = () => {
    timeoutRef.current = setTimeout(() => {
      setIsVisible(true)
    }, delay)
  }

  const handleMouseLeave = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    setIsVisible(false)
  }

  const handleFocus = () => {
    setIsVisible(true)
  }

  const handleBlur = () => {
    setIsVisible(false)
  }

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [])

  const tooltipClasses = [
    'tooltip',
    `tooltip--${placement}`,
    isVisible && 'tooltip--visible',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <div
      className="tooltip-wrapper"
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      onFocus={handleFocus}
      onBlur={handleBlur}
    >
      {React.Children.map(children, (child) => {
        if (React.isValidElement(child)) {
          return React.cloneElement(child as React.ReactElement<any>, {
            'aria-describedby': isVisible ? tooltipId.current : undefined,
          })
        }
        return child
      })}

      {isVisible && (
        <div
          id={tooltipId.current}
          role="tooltip"
          className={tooltipClasses}
        >
          <div className="tooltip__content">{content}</div>
          <div className="tooltip__arrow" aria-hidden="true" />
        </div>
      )}
    </div>
  )
}

export default Tooltip
