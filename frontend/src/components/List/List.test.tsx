import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { List } from './List'

describe('List', () => {
  const mockItems = [
    { id: 1, primary: 'Item 1', secondary: 'Description 1' },
    { id: 2, primary: 'Item 2', secondary: 'Description 2' },
    { id: 3, primary: 'Item 3', secondary: 'Description 3', disabled: true },
  ]

  it('renders without crashing', () => {
    const { container } = render(<List items={mockItems} />)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('renders all list items', () => {
    render(<List items={mockItems} />)
    expect(screen.getByText('Item 1')).toBeInTheDocument()
    expect(screen.getByText('Item 2')).toBeInTheDocument()
    expect(screen.getByText('Item 3')).toBeInTheDocument()
  })

  it('displays primary and secondary text', () => {
    render(<List items={mockItems} />)
    expect(screen.getByText('Description 1')).toBeInTheDocument()
    expect(screen.getByText('Description 2')).toBeInTheDocument()
  })

  it('renders icons when provided', () => {
    const itemsWithIcons = [
      { id: 1, primary: 'Item', icon: <span data-testid="icon-1">🔔</span> },
    ]
    render(<List items={itemsWithIcons} />)
    expect(screen.getByTestId('icon-1')).toBeInTheDocument()
  })

  it('renders actions when provided', () => {
    const itemsWithActions = [
      { 
        id: 1, 
        primary: 'Item', 
        actions: <button data-testid="action-btn">Edit</button>
      },
    ]
    render(<List items={itemsWithActions} />)
    expect(screen.getByTestId('action-btn')).toBeInTheDocument()
  })

  it('calls onItemClick when item is clicked', () => {
    const handleClick = vi.fn()
    render(<List items={mockItems} onItemClick={handleClick} />)
    
    const item1 = screen.getByText('Item 1')
    fireEvent.click(item1)
    
    expect(handleClick).toHaveBeenCalledWith(mockItems[0])
  })

  it('does not call onItemClick for disabled items', () => {
    const handleClick = vi.fn()
    render(<List items={mockItems} onItemClick={handleClick} />)
    
    const item3 = screen.getByText('Item 3')
    fireEvent.click(item3)
    
    expect(handleClick).not.toHaveBeenCalled()
  })

  it('applies hoverable class by default', () => {
    const { container } = render(<List items={mockItems} />)
    expect(container.firstChild).toHaveClass('list--hoverable')
  })

  it('applies dense class when dense prop is true', () => {
    const { container } = render(<List items={mockItems} dense />)
    expect(container.firstChild).toHaveClass('list--dense')
  })

  it('applies custom className', () => {
    const { container } = render(<List items={mockItems} className="custom-list" />)
    expect(container.firstChild).toHaveClass('custom-list')
  })

  it('renders dividers between items when specified', () => {
    const itemsWithDivider = [
      { id: 1, primary: 'Item 1', divider: true },
      { id: 2, primary: 'Item 2' },
    ]
    const { container } = render(<List items={itemsWithDivider} />)
    expect(container.querySelector('.list__divider')).toBeInTheDocument()
  })

  it('has proper role attribute', () => {
    render(<List items={mockItems} />)
    expect(screen.getByRole('list')).toBeInTheDocument()
  })

  it('makes clickable items keyboard accessible', () => {
    const handleClick = vi.fn()
    render(<List items={mockItems} onItemClick={handleClick} />)
    
    const item1 = screen.getByText('Item 1').closest('li') as HTMLElement
    expect(item1).toHaveAttribute('role', 'button')
    expect(item1).toHaveAttribute('tabIndex', '0')
  })

  it('handles keyboard events on clickable items', () => {
    const handleClick = vi.fn()
    render(<List items={mockItems} onItemClick={handleClick} />)
    
    const item1 = screen.getByText('Item 1').closest('li') as HTMLElement
    fireEvent.keyDown(item1, { key: 'Enter' })
    
    expect(handleClick).toHaveBeenCalledWith(mockItems[0])
  })
})
