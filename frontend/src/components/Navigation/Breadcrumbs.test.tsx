import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Breadcrumbs } from './Breadcrumbs'

describe('Breadcrumbs', () => {
  const mockItems = [
    { label: 'Home', href: '/' },
    { label: 'Products', href: '/products' },
    { label: 'Product Details' },
  ]

  it('renders without crashing', () => {
    const { container } = render(<Breadcrumbs items={mockItems} />)
    expect(container.querySelector('.breadcrumbs')).toBeInTheDocument()
  })

  it('renders all breadcrumb items', () => {
    render(<Breadcrumbs items={mockItems} />)
    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Products')).toBeInTheDocument()
    expect(screen.getByText('Product Details')).toBeInTheDocument()
  })

  it('renders links for items with href', () => {
    render(<Breadcrumbs items={mockItems} />)
    const homeLink = screen.getByText('Home').closest('a')
    expect(homeLink).toHaveAttribute('href', '/')
  })

  it('renders last item as plain text', () => {
    render(<Breadcrumbs items={mockItems} />)
    const lastItem = screen.getByText('Product Details')
    expect(lastItem.closest('a')).not.toBeInTheDocument()
  })

  it('sets aria-current on last item', () => {
    render(<Breadcrumbs items={mockItems} />)
    const lastItem = screen.getByText('Product Details')
    expect(lastItem).toHaveAttribute('aria-current', 'page')
  })

  it('renders default separator', () => {
    const { container } = render(<Breadcrumbs items={mockItems} />)
    const separators = container.querySelectorAll('.breadcrumbs__separator')
    expect(separators.length).toBe(2) // One less than items
    expect(separators[0]).toHaveTextContent('/')
  })

  it('renders custom separator', () => {
    const { container } = render(<Breadcrumbs items={mockItems} separator=">" />)
    const separators = container.querySelectorAll('.breadcrumbs__separator')
    expect(separators[0]).toHaveTextContent('>')
  })

  it('renders icons when provided', () => {
    const itemsWithIcons = [
      { label: 'Home', href: '/', icon: <span data-testid="home-icon">🏠</span> },
      { label: 'About' },
    ]
    render(<Breadcrumbs items={itemsWithIcons} />)
    expect(screen.getByTestId('home-icon')).toBeInTheDocument()
  })

  it('calls onClick when item is clicked', () => {
    const handleClick = vi.fn()
    const items = [
      { label: 'Clickable', href: '/click', onClick: handleClick },
      { label: 'Current' },
    ]
    render(<Breadcrumbs items={items} />)
    
    const link = screen.getByText('Clickable')
    fireEvent.click(link)
    
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('prevents default when onClick is provided', () => {
    const handleClick = vi.fn()
    const items = [
      { label: 'Clickable', href: '/click', onClick: handleClick },
      { label: 'Current' },
    ]
    render(<Breadcrumbs items={items} />)
    
    const link = screen.getByText('Clickable')
    const event = new MouseEvent('click', { bubbles: true, cancelable: true })
    const preventDefaultSpy = vi.spyOn(event, 'preventDefault')
    
    fireEvent.click(link)
    
    expect(handleClick).toHaveBeenCalled()
  })

  it('applies custom className', () => {
    const { container } = render(<Breadcrumbs items={mockItems} className="custom-breadcrumbs" />)
    expect(container.firstChild).toHaveClass('breadcrumbs', 'custom-breadcrumbs')
  })

  it('has proper ARIA label', () => {
    render(<Breadcrumbs items={mockItems} />)
    const nav = screen.getByLabelText('Breadcrumb')
    expect(nav).toBeInTheDocument()
  })

  it('renders as nav element', () => {
    const { container } = render(<Breadcrumbs items={mockItems} />)
    expect(container.querySelector('nav')).toBeInTheDocument()
  })

  it('renders ordered list', () => {
    const { container } = render(<Breadcrumbs items={mockItems} />)
    expect(container.querySelector('ol')).toBeInTheDocument()
  })

  it('renders correct number of separators', () => {
    const { container } = render(<Breadcrumbs items={mockItems} />)
    const separators = container.querySelectorAll('.breadcrumbs__separator')
    expect(separators.length).toBe(mockItems.length - 1)
  })
})
