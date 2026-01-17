import React, { HTMLAttributes, ReactNode } from 'react'
import './Card.css'

export interface CardProps extends HTMLAttributes<HTMLDivElement> {
  /**
   * Card header content
   */
  header?: ReactNode

  /**
   * Card body content
   */
  children: ReactNode

  /**
   * Card footer content
   */
  footer?: ReactNode

  /**
   * Card shadow elevation (sm, md, lg)
   */
  elevation?: 'sm' | 'md' | 'lg'

  /**
   * Hover effect
   */
  hoverable?: boolean

  /**
   * Make card clickable
   */
  clickable?: boolean

  /**
   * Click handler for clickable cards
   */
  onClick?: () => void
}

/**
 * Card Component
 *
 * A container component for grouping related content.
 * Follows WCAG 2.1 AA accessibility standards.
 *
 * @example
 * <Card header="Title">
 *   Card content here
 * </Card>
 */
export const Card = React.forwardRef<HTMLDivElement, CardProps>(
  (
    {
      header,
      children,
      footer,
      elevation = 'md',
      hoverable = false,
      clickable = false,
      onClick,
      className,
      ...props
    },
    ref
  ) => {
    const cardClasses = [
      'card',
      `card--${elevation}`,
      hoverable && 'card--hoverable',
      clickable && 'card--clickable',
      className,
    ]
      .filter(Boolean)
      .join(' ')

    return (
      <div
        ref={ref}
        className={cardClasses}
        onClick={clickable ? onClick : undefined}
        role={clickable ? 'button' : 'region'}
        tabIndex={clickable ? 0 : undefined}
        onKeyDown={
          clickable
            ? (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  onClick?.()
                }
              }
            : undefined
        }
        {...props}
      >
        {header && <div className="card__header">{header}</div>}

        <div className="card__body">{children}</div>

        {footer && <div className="card__footer">{footer}</div>}
      </div>
    )
  }
)

Card.displayName = 'Card'

export default Card
