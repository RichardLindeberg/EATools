import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import { Tooltip } from './Tooltip'

describe('Tooltip', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.restoreAllMocks()
    vi.useRealTimers()
  })

  it('renders without crashing', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )
    expect(screen.getByText('Hover me')).toBeInTheDocument()
  })

  it('does not show tooltip by default', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )
    expect(screen.queryByText('Tooltip text')).not.toBeInTheDocument()
  })

  it('shows tooltip on mouse enter after delay', async () => {
    render(
      <Tooltip content="Tooltip text" delay={200}>
        <button>Hover me</button>
      </Tooltip>
    )

    const button = screen.getByText('Hover me')
    fireEvent.mouseEnter(button.parentElement!)

    // Before delay
    expect(screen.queryByText('Tooltip text')).not.toBeInTheDocument()

    // After delay
    act(() => {
      vi.advanceTimersByTime(200)
    })
    
    expect(screen.getByText('Tooltip text')).toBeInTheDocument()
  })

  it('hides tooltip on mouse leave', () => {
    render(
      <Tooltip content="Tooltip text" delay={0}>
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(0)
    })
    
    expect(screen.getByText('Tooltip text')).toBeInTheDocument()

    fireEvent.mouseLeave(wrapper)

    expect(screen.queryByText('Tooltip text')).not.toBeInTheDocument()
  })

  it('shows tooltip on focus', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Focus me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Focus me').parentElement!
    fireEvent.focus(wrapper)

    expect(screen.getByText('Tooltip text')).toBeInTheDocument()
  })

  it('hides tooltip on blur', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Focus me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Focus me').parentElement!
    
    fireEvent.focus(wrapper)
    expect(screen.getByText('Tooltip text')).toBeInTheDocument()

    fireEvent.blur(wrapper)
    
    expect(screen.queryByText('Tooltip text')).not.toBeInTheDocument()
  })

  it('applies top placement by default', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('.tooltip')
    expect(tooltip).toHaveClass('tooltip--top')
  })

  it('applies bottom placement when specified', () => {
    render(
      <Tooltip content="Tooltip text" placement="bottom">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('.tooltip')
    expect(tooltip).toHaveClass('tooltip--bottom')
  })

  it('applies left placement when specified', () => {
    render(
      <Tooltip content="Tooltip text" placement="left">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('.tooltip')
    expect(tooltip).toHaveClass('tooltip--left')
  })

  it('applies right placement when specified', () => {
    render(
      <Tooltip content="Tooltip text" placement="right">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('.tooltip')
    expect(tooltip).toHaveClass('tooltip--right')
  })

  it('applies custom className', () => {
    render(
      <Tooltip content="Tooltip text" className="custom-tooltip">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('.tooltip')
    expect(tooltip).toHaveClass('custom-tooltip')
  })

  it('has proper ARIA attributes', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('[role="tooltip"]')
    expect(tooltip).toBeInTheDocument()
    expect(tooltip).toHaveAttribute('id')
  })

  it('connects tooltip to trigger with aria-describedby', () => {
    render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )

    const button = screen.getByText('Hover me')
    const wrapper = button.parentElement!
    
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const tooltip = screen.getByText('Tooltip text').closest('[role="tooltip"]')
    const tooltipId = tooltip?.getAttribute('id')
    expect(button).toHaveAttribute('aria-describedby', tooltipId)
  })

  it('cancels show timeout on mouse leave before delay', () => {
    render(
      <Tooltip content="Tooltip text" delay={500}>
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(100)
    })
    
    fireEvent.mouseLeave(wrapper)
    act(() => {
      vi.advanceTimersByTime(500)
    })

    expect(screen.queryByText('Tooltip text')).not.toBeInTheDocument()
  })

  it('renders tooltip arrow', () => {
    const { container } = render(
      <Tooltip content="Tooltip text">
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    const arrow = container.querySelector('.tooltip__arrow')
    expect(arrow).toBeInTheDocument()
  })

  it('renders complex content', () => {
    render(
      <Tooltip content={<div><strong>Bold</strong> text</div>}>
        <button>Hover me</button>
      </Tooltip>
    )

    const wrapper = screen.getByText('Hover me').parentElement!
    fireEvent.mouseEnter(wrapper)
    act(() => {
      vi.advanceTimersByTime(200)
    })

    expect(screen.getByText('Bold')).toBeInTheDocument()
    expect(screen.getByText('text')).toBeInTheDocument()
  })
})
