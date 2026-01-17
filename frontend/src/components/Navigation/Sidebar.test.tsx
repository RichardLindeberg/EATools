import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Sidebar } from './Sidebar'

describe('Sidebar', () => {
  const mockSections = [
    {
      title: 'Main',
      items: [
        { id: '1', label: 'Dashboard', href: '/' },
        { id: '2', label: 'Settings', href: '/settings', active: true },
      ],
    },
    {
      title: 'Admin',
      items: [
        { id: '3', label: 'Users', href: '/users', badge: 5 },
      ],
    },
  ]

  it('renders without crashing', () => {
    const { container } = render(<Sidebar sections={mockSections} />)
    expect(container.querySelector('.sidebar')).toBeInTheDocument()
  })

  it('renders all section titles', () => {
    render(<Sidebar sections={mockSections} />)
    expect(screen.getByText('Main')).toBeInTheDocument()
    expect(screen.getByText('Admin')).toBeInTheDocument()
  })

  it('renders all items', () => {
    render(<Sidebar sections={mockSections} />)
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
    expect(screen.getByText('Settings')).toBeInTheDocument()
    expect(screen.getByText('Users')).toBeInTheDocument()
  })

  it('applies active class to active items', () => {
    render(<Sidebar sections={mockSections} />)
    const settingsLink = screen.getByText('Settings').closest('a')
    expect(settingsLink).toHaveClass('sidebar__item--active')
  })

  it('renders badge when provided', () => {
    render(<Sidebar sections={mockSections} />)
    expect(screen.getByText('5')).toBeInTheDocument()
  })

  it('renders icons when provided', () => {
    const sectionsWithIcons = [
      {
        items: [
          { id: '1', label: 'Home', icon: <span data-testid="home-icon">🏠</span> },
        ],
      },
    ]
    render(<Sidebar sections={sectionsWithIcons} />)
    expect(screen.getByTestId('home-icon')).toBeInTheDocument()
  })

  it('calls onClick when item is clicked', () => {
    const handleClick = vi.fn()
    const sections = [
      {
        items: [
          { id: '1', label: 'Clickable', onClick: handleClick },
        ],
      },
    ]
    render(<Sidebar sections={sections} />)
    
    const item = screen.getByText('Clickable')
    fireEvent.click(item)
    
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('renders toggle button when onToggle is provided', () => {
    const handleToggle = vi.fn()
    render(<Sidebar sections={mockSections} onToggle={handleToggle} />)
    
    const toggleButton = screen.getByLabelText('Collapse sidebar')
    expect(toggleButton).toBeInTheDocument()
  })

  it('calls onToggle when toggle button is clicked', () => {
    const handleToggle = vi.fn()
    render(<Sidebar sections={mockSections} onToggle={handleToggle} />)
    
    const toggleButton = screen.getByLabelText('Collapse sidebar')
    fireEvent.click(toggleButton)
    
    expect(handleToggle).toHaveBeenCalledTimes(1)
  })

  it('applies collapsed class when collapsed', () => {
    const { container } = render(<Sidebar sections={mockSections} collapsed />)
    expect(container.querySelector('.sidebar--collapsed')).toBeInTheDocument()
  })

  it('hides section titles when collapsed', () => {
    render(<Sidebar sections={mockSections} collapsed />)
    expect(screen.queryByText('Main')).not.toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('hides labels when collapsed', () => {
    const { container } = render(<Sidebar sections={mockSections} collapsed />)
    // Labels should not be visible in collapsed mode
    const labels = container.querySelectorAll('.sidebar__item-label')
    expect(labels.length).toBe(0)
  })

  it('changes toggle button label when collapsed', () => {
    render(<Sidebar sections={mockSections} collapsed onToggle={() => {}} />)
    expect(screen.getByLabelText('Expand sidebar')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Sidebar sections={mockSections} className="custom-sidebar" />)
    expect(container.firstChild).toHaveClass('sidebar', 'custom-sidebar')
  })

  it('renders as aside element', () => {
    const { container } = render(<Sidebar sections={mockSections} />)
    expect(container.querySelector('aside')).toBeInTheDocument()
  })

  it('renders navigation with nav element', () => {
    const { container } = render(<Sidebar sections={mockSections} />)
    expect(container.querySelector('.sidebar__nav')).toBeInTheDocument()
  })
})
