import React, { ReactNode } from 'react'
import './EmptyState.css'

export interface EmptyStateProps {
  /**
   * Icon or illustration
   */
  icon?: ReactNode

  /**
   * Title text
   */
  title: string

  /**
   * Description text
   */
  description?: string

  /**
   * Action button or content
   */
  action?: ReactNode

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * EmptyState Component
 *
 * Display empty state with icon, message, and optional action.
 * Used for empty lists, search results, or filtered views.
 *
 * @example
 * <EmptyState
 *   icon={<SearchIcon />}
 *   title="No results found"
 *   description="Try adjusting your search or filters"
 *   action={<Button onClick={clearFilters}>Clear Filters</Button>}
 * />
 */
export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  action,
  className,
}) => {
  const emptyStateClasses = ['empty-state', className].filter(Boolean).join(' ')

  return (
    <div className={emptyStateClasses}>
      {icon && <div className="empty-state__icon">{icon}</div>}
      
      <div className="empty-state__content">
        <h3 className="empty-state__title">{title}</h3>
        {description && (
          <p className="empty-state__description">{description}</p>
        )}
      </div>

      {action && <div className="empty-state__action">{action}</div>}
    </div>
  )
}

export default EmptyState
