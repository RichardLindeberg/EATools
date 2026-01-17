import React, { ReactNode } from 'react'
import './List.css'

export interface ListItem {
  id: string | number
  primary: ReactNode
  secondary?: ReactNode
  icon?: ReactNode
  actions?: ReactNode
  disabled?: boolean
  divider?: boolean
}

export interface ListProps {
  /**
   * List items
   */
  items: ListItem[]

  /**
   * Item click handler
   */
  onItemClick?: (item: ListItem) => void

  /**
   * Show hover effect
   */
  hoverable?: boolean

  /**
   * Dense/compact layout
   */
  dense?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * List Component
 *
 * Displays a vertical list of items with optional icons, secondary text, and actions.
 * Alternative to Table for simpler list views.
 *
 * @example
 * <List 
 *   items={[
 *     { 
 *       id: 1, 
 *       primary: 'Server 1', 
 *       secondary: 'Active',
 *       icon: <ServerIcon />,
 *       actions: <Button size="sm">View</Button>
 *     }
 *   ]}
 *   onItemClick={(item) => navigate(`/servers/${item.id}`)}
 * />
 */
export const List: React.FC<ListProps> = ({
  items,
  onItemClick,
  hoverable = true,
  dense = false,
  className,
}) => {
  const listClasses = [
    'list',
    hoverable && 'list--hoverable',
    dense && 'list--dense',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const handleItemClick = (item: ListItem) => {
    if (!item.disabled && onItemClick) {
      onItemClick(item)
    }
  }

  return (
    <ul className={listClasses} role="list">
      {items.map((item, index) => {
        const itemClasses = [
          'list__item',
          item.disabled && 'list__item--disabled',
          onItemClick && !item.disabled && 'list__item--clickable',
        ]
          .filter(Boolean)
          .join(' ')

        return (
          <React.Fragment key={item.id}>
            <li
              className={itemClasses}
              onClick={() => handleItemClick(item)}
              role={onItemClick && !item.disabled ? 'button' : undefined}
              tabIndex={onItemClick && !item.disabled ? 0 : undefined}
              onKeyDown={(e) => {
                if ((e.key === 'Enter' || e.key === ' ') && onItemClick && !item.disabled) {
                  e.preventDefault()
                  handleItemClick(item)
                }
              }}
            >
              {item.icon && (
                <div className="list__item-icon">{item.icon}</div>
              )}

              <div className="list__item-content">
                <div className="list__item-primary">{item.primary}</div>
                {item.secondary && (
                  <div className="list__item-secondary">{item.secondary}</div>
                )}
              </div>

              {item.actions && (
                <div className="list__item-actions">{item.actions}</div>
              )}
            </li>

            {item.divider && index < items.length - 1 && (
              <li className="list__divider" role="separator" aria-hidden="true" />
            )}
          </React.Fragment>
        )
      })}
    </ul>
  )
}

export default List
