import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Tabs } from './Tabs'

describe('Tabs', () => {
  const mockTabs = [
    { id: 'tab1', label: 'Tab 1' },
    { id: 'tab2', label: 'Tab 2' },
    { id: 'tab3', label: 'Tab 3', disabled: true },
  ]

  const defaultProps = {
    tabs: mockTabs,
    activeTab: 'tab1',
    onChange: vi.fn(),
  }

  it('renders without crashing', () => {
    render(<Tabs {...defaultProps} />)
    expect(screen.getByText('Tab 1')).toBeInTheDocument()
  })

  it('renders all tab labels', () => {
    render(<Tabs {...defaultProps} />)
    expect(screen.getByText('Tab 1')).toBeInTheDocument()
    expect(screen.getByText('Tab 2')).toBeInTheDocument()
    expect(screen.getByText('Tab 3')).toBeInTheDocument()
  })

  it('calls onChange when tab is clicked', () => {
    const handleChange = vi.fn()
    render(<Tabs {...defaultProps} onChange={handleChange} />)
    
    const tab2Button = screen.getByText('Tab 2')
    fireEvent.click(tab2Button)
    
    expect(handleChange).toHaveBeenCalledWith('tab2')
  })

  it('does not call onChange for disabled tab', () => {
    const handleChange = vi.fn()
    render(<Tabs {...defaultProps} onChange={handleChange} />)
    
    const tab3Button = screen.getByText('Tab 3')
    fireEvent.click(tab3Button)
    
    expect(handleChange).not.toHaveBeenCalled()
  })

  it('respects activeTab prop', () => {
    render(<Tabs {...defaultProps} activeTab="tab2" />)
    const tab2 = screen.getByRole('tab', { name: 'Tab 2' })
    expect(tab2).toHaveAttribute('aria-selected', 'true')
  })

  it('disables specified tabs', () => {
    render(<Tabs {...defaultProps} />)
    const tab3Button = screen.getByRole('tab', { name: 'Tab 3' })
    expect(tab3Button).toBeDisabled()
    expect(tab3Button).toHaveAttribute('aria-disabled', 'true')
  })

  it('applies horizontal orientation by default', () => {
    const { container } = render(<Tabs {...defaultProps} />)
    expect(container.querySelector('.tabs--horizontal')).toBeInTheDocument()
  })

  it('applies vertical orientation when specified', () => {
    const { container } = render(<Tabs {...defaultProps} orientation="vertical" />)
    expect(container.querySelector('.tabs--vertical')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Tabs {...defaultProps} className="custom-tabs" />)
    expect(container.firstChild).toHaveClass('custom-tabs')
  })

  it('has proper ARIA attributes', () => {
    render(<Tabs {...defaultProps} />)
    const tabList = screen.getByRole('tablist')
    expect(tabList).toBeInTheDocument()
    
    const tabs = screen.getAllByRole('tab')
    expect(tabs).toHaveLength(3)
  })

  it('sets aria-selected on active tab', () => {
    render(<Tabs {...defaultProps} activeTab="tab1" />)
    const tab1 = screen.getByRole('tab', { name: 'Tab 1' })
    expect(tab1).toHaveAttribute('aria-selected', 'true')
  })

  it('supports keyboard navigation with ArrowRight', () => {
    const handleChange = vi.fn()
    render(<Tabs {...defaultProps} onChange={handleChange} />)
    
    const tab1Button = screen.getByText('Tab 1')
    fireEvent.keyDown(tab1Button, { key: 'ArrowRight' })
    
    expect(handleChange).toHaveBeenCalledWith('tab2')
  })

  it('supports keyboard navigation with ArrowLeft', () => {
    const handleChange = vi.fn()
    render(<Tabs {...defaultProps} activeTab="tab2" onChange={handleChange} />)
    
    const tab2Button = screen.getByText('Tab 2')
    fireEvent.keyDown(tab2Button, { key: 'ArrowLeft' })
    
    expect(handleChange).toHaveBeenCalledWith('tab1')
  })

  it('supports vertical keyboard navigation', () => {
    const handleChange = vi.fn()
    render(<Tabs {...defaultProps} orientation="vertical" activeTab="tab1" onChange={handleChange} />)
    
    const tab1Button = screen.getByText('Tab 1')
    // In vertical mode, ArrowDown should navigate
    fireEvent.keyDown(tab1Button, { key: 'ArrowDown' })
    
    expect(handleChange).toHaveBeenCalledWith('tab2')
  })
})
