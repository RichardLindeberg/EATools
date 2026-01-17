import React, { ReactNode } from 'react'
import './Tabs.css'

export interface Tab {
  id: string
  label: string
  icon?: ReactNode
  badge?: string | number
  disabled?: boolean
}

export interface TabsProps {
  /**
   * Tab items
   */
  tabs: Tab[]

  /**
   * Active tab ID
   */
  activeTab: string

  /**
   * Tab change handler
   */
  onChange: (tabId: string) => void

  /**
   * Tabs orientation
   */
  orientation?: 'horizontal' | 'vertical'

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Tabs Component
 *
 * Accessible tab navigation with horizontal or vertical layout.
 * Supports icons, badges, and disabled states.
 *
 * @example
 * <Tabs 
 *   tabs={[
 *     { id: 'details', label: 'Details', icon: <InfoIcon /> },
 *     { id: 'settings', label: 'Settings' }
 *   ]}
 *   activeTab={currentTab}
 *   onChange={setCurrentTab}
 * />
 */
export const Tabs: React.FC<TabsProps> = ({
  tabs,
  activeTab,
  onChange,
  orientation = 'horizontal',
  className,
}) => {
  const tabsClasses = [
    'tabs',
    `tabs--${orientation}`,
    className,
  ]
    .filter(Boolean)
    .join(' ')

  const handleKeyDown = (e: React.KeyboardEvent, tabId: string, index: number) => {
    let nextIndex = index

    if (orientation === 'horizontal') {
      if (e.key === 'ArrowLeft') {
        nextIndex = index - 1
      } else if (e.key === 'ArrowRight') {
        nextIndex = index + 1
      }
    } else {
      if (e.key === 'ArrowUp') {
        nextIndex = index - 1
      } else if (e.key === 'ArrowDown') {
        nextIndex = index + 1
      }
    }

    // Wrap around
    if (nextIndex < 0) nextIndex = tabs.length - 1
    if (nextIndex >= tabs.length) nextIndex = 0

    const nextTab = tabs[nextIndex]
    if (nextTab && !nextTab.disabled) {
      onChange(nextTab.id)
      // Focus the next tab button
      const nextButton = document.querySelector(
        `button[data-tab-id="${nextTab.id}"]`
      ) as HTMLButtonElement
      nextButton?.focus()
    }
  }

  return (
    <div className={tabsClasses}>
      <div className="tabs__list" role="tablist" aria-orientation={orientation}>
        {tabs.map((tab, index) => {
          const isActive = activeTab === tab.id

          return (
            <button
              key={tab.id}
              type="button"
              role="tab"
              data-tab-id={tab.id}
              className={`tabs__tab ${isActive ? 'tabs__tab--active' : ''} ${
                tab.disabled ? 'tabs__tab--disabled' : ''
              }`}
              aria-selected={isActive}
              aria-disabled={tab.disabled}
              tabIndex={isActive ? 0 : -1}
              disabled={tab.disabled}
              onClick={() => !tab.disabled && onChange(tab.id)}
              onKeyDown={(e) => handleKeyDown(e, tab.id, index)}
            >
              {tab.icon && (
                <span className="tabs__tab-icon" aria-hidden="true">
                  {tab.icon}
                </span>
              )}
              <span className="tabs__tab-label">{tab.label}</span>
              {tab.badge && (
                <span className="tabs__tab-badge">{tab.badge}</span>
              )}
            </button>
          )
        })}
      </div>
    </div>
  )
}

export default Tabs
