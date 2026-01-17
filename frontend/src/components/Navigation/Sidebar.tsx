import React, { ReactNode, useState } from 'react'
import './Sidebar.css'

export interface SidebarSection {
  title?: string
  items: SidebarItem[]
}

export interface SidebarItem {
  id: string
  label: string
  icon?: ReactNode
  href?: string
  onClick?: () => void
  active?: boolean
  badge?: string | number
}

export interface SidebarProps {
  /**
   * Sidebar sections
   */
  sections: SidebarSection[]

  /**
   * Collapsed state
   */
  collapsed?: boolean

  /**
   * Toggle collapsed state
   */
  onToggle?: () => void

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Sidebar Component
 *
 * Collapsible navigation sidebar with sections and items.
 * Supports icons, badges, and active states.
 *
 * @example
 * <Sidebar 
 *   sections={[
 *     {
 *       title: "Main",
 *       items: [
 *         { id: '1', label: 'Dashboard', icon: <HomeIcon />, active: true }
 *       ]
 *     }
 *   ]}
 *   collapsed={isCollapsed}
 *   onToggle={() => setIsCollapsed(!isCollapsed)}
 * />
 */
export const Sidebar: React.FC<SidebarProps> = ({
  sections,
  collapsed = false,
  onToggle,
  className,
}) => {
  const sidebarClasses = [
    'sidebar',
    collapsed && 'sidebar--collapsed',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const handleItemClick = (item: SidebarItem) => {
    if (item.onClick) {
      item.onClick()
    }
  }

  return (
    <aside className={sidebarClasses}>
      <div className="sidebar__container">
        {onToggle && (
          <button
            className="sidebar__toggle"
            onClick={onToggle}
            aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            {collapsed ? '→' : '←'}
          </button>
        )}

        <nav className="sidebar__nav">
          {sections.map((section, sectionIndex) => (
            <div key={sectionIndex} className="sidebar__section">
              {section.title && !collapsed && (
                <div className="sidebar__section-title">{section.title}</div>
              )}

              <ul className="sidebar__items">
                {section.items.map((item) => (
                  <li key={item.id}>
                    <a
                      href={item.href || '#'}
                      className={`sidebar__item ${item.active ? 'sidebar__item--active' : ''}`}
                      onClick={(e) => {
                        if (item.onClick) {
                          e.preventDefault()
                          handleItemClick(item)
                        }
                      }}
                      title={collapsed ? item.label : undefined}
                    >
                      {item.icon && (
                        <span className="sidebar__item-icon">{item.icon}</span>
                      )}
                      {!collapsed && (
                        <>
                          <span className="sidebar__item-label">{item.label}</span>
                          {item.badge && (
                            <span className="sidebar__item-badge">{item.badge}</span>
                          )}
                        </>
                      )}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>
      </div>
    </aside>
  )
}

export default Sidebar
