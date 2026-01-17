import React, { ReactNode } from 'react'
import './Breadcrumbs.css'

export interface BreadcrumbItem {
  label: string
  href?: string
  icon?: ReactNode
  onClick?: () => void
}

export interface BreadcrumbsProps {
  /**
   * Breadcrumb items
   */
  items: BreadcrumbItem[]

  /**
   * Separator between items
   */
  separator?: ReactNode

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Breadcrumbs Component
 *
 * Hierarchical navigation showing the current page's location.
 * Accessible with proper ARIA attributes.
 *
 * @example
 * <Breadcrumbs 
 *   items={[
 *     { label: 'Home', href: '/' },
 *     { label: 'Servers', href: '/servers' },
 *     { label: 'Server Details' }
 *   ]}
 * />
 */
export const Breadcrumbs: React.FC<BreadcrumbsProps> = ({
  items,
  separator = '/',
  className,
}) => {
  const breadcrumbsClasses = ['breadcrumbs', className].filter(Boolean).join(' ')

  const handleClick = (item: BreadcrumbItem, e: React.MouseEvent) => {
    if (item.onClick) {
      e.preventDefault()
      item.onClick()
    }
  }

  return (
    <nav className={breadcrumbsClasses} aria-label="Breadcrumb">
      <ol className="breadcrumbs__list">
        {items.map((item, index) => {
          const isLast = index === items.length - 1

          return (
            <li key={index} className="breadcrumbs__item">
              {item.icon && (
                <span className="breadcrumbs__icon" aria-hidden="true">
                  {item.icon}
                </span>
              )}

              {!isLast && item.href ? (
                <a
                  href={item.href}
                  className="breadcrumbs__link"
                  onClick={(e) => handleClick(item, e)}
                >
                  {item.label}
                </a>
              ) : (
                <span
                  className={`breadcrumbs__text ${isLast ? 'breadcrumbs__text--current' : ''}`}
                  aria-current={isLast ? 'page' : undefined}
                >
                  {item.label}
                </span>
              )}

              {!isLast && (
                <span className="breadcrumbs__separator" aria-hidden="true">
                  {separator}
                </span>
              )}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}

export default Breadcrumbs
