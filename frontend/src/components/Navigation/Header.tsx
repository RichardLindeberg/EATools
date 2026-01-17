import React, { ReactNode } from 'react'
import './Header.css'

export interface HeaderProps {
  /**
   * Logo or branding element
   */
  logo?: ReactNode

  /**
   * Center content (search, navigation, etc.)
   */
  children?: ReactNode

  /**
   * Right-side content (user menu, notifications, etc.)
   */
  actions?: ReactNode

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Header Component
 *
 * Application header with logo, content area, and actions.
 * Sticky positioning for persistent navigation.
 *
 * @example
 * <Header 
 *   logo={<img src="/logo.png" alt="EATool" />}
 *   actions={<UserMenu />}
 * >
 *   <SearchBar />
 * </Header>
 */
export const Header: React.FC<HeaderProps> = ({
  logo,
  children,
  actions,
  className,
}) => {
  const headerClasses = ['header', className].filter(Boolean).join(' ')

  return (
    <header className={headerClasses}>
      <div className="header__container">
        {logo && <div className="header__logo">{logo}</div>}
        
        {children && <div className="header__content">{children}</div>}
        
        {actions && <div className="header__actions">{actions}</div>}
      </div>
    </header>
  )
}

export default Header
