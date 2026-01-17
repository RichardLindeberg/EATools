import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, act } from '@testing-library/react'
import { ToastProvider, useToast } from './Toast'

// Test component that uses the hook
const TestComponent = () => {
  const { addToast, removeToast, clearAll } = useToast()

  return (
    <div>
      <button onClick={() => addToast({ message: 'Test toast' })}>
        Add Toast
      </button>
      <button onClick={() => addToast({ message: 'Success!', variant: 'success' })}>
        Add Success
      </button>
      <button onClick={() => addToast({ message: 'Warning!', variant: 'warning' })}>
        Add Warning
      </button>
      <button onClick={() => addToast({ message: 'Error!', variant: 'danger' })}>
        Add Danger
      </button>
      <button onClick={() => addToast({ message: 'With action', action: { label: 'Undo', onClick: () => {} } })}>
        Add With Action
      </button>
      <button onClick={() => clearAll()}>
        Clear All
      </button>
    </div>
  )
}

describe('Toast', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.restoreAllMocks()
    vi.useRealTimers()
  })

  it('renders ToastProvider without crashing', () => {
    render(
      <ToastProvider>
        <div>Test</div>
      </ToastProvider>
    )
    expect(screen.getByText('Test')).toBeInTheDocument()
  })

  it('throws error when useToast is used outside provider', () => {
    const ConsoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    
    expect(() => {
      render(<TestComponent />)
    }).toThrow('useToast must be used within ToastProvider')
    
    ConsoleError.mockRestore()
  })

  it('adds toast when addToast is called', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    const button = screen.getByText('Add Toast')
    fireEvent.click(button)

    expect(screen.getByText('Test toast')).toBeInTheDocument()
  })

  it('displays success toast with correct variant', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Success'))
    
    const toast = screen.getByText('Success!').closest('.toast')
    expect(toast).toHaveClass('toast--success')
  })

  it('displays warning toast with correct variant', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Warning'))
    
    const toast = screen.getByText('Warning!').closest('.toast')
    expect(toast).toHaveClass('toast--warning')
  })

  it('displays danger toast with correct variant', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Danger'))
    
    const toast = screen.getByText('Error!').closest('.toast')
    expect(toast).toHaveClass('toast--danger')
  })

  it('renders action button when action is provided', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add With Action'))
    
    expect(screen.getByText('Undo')).toBeInTheDocument()
  })

  it('removes toast when close button is clicked', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Toast'))
    expect(screen.getByText('Test toast')).toBeInTheDocument()

    const closeButton = screen.getByLabelText('Close notification')
    fireEvent.click(closeButton)

    expect(screen.queryByText('Test toast')).not.toBeInTheDocument()
  })

  it('clears all toasts when clearAll is called', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Toast'))
    fireEvent.click(screen.getByText('Add Success'))
    
    expect(screen.getByText('Test toast')).toBeInTheDocument()
    expect(screen.getByText('Success!')).toBeInTheDocument()

    fireEvent.click(screen.getByText('Clear All'))

    expect(screen.queryByText('Test toast')).not.toBeInTheDocument()
    expect(screen.queryByText('Success!')).not.toBeInTheDocument()
  })

  it('auto-removes toast after duration', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    act(() => {
      fireEvent.click(screen.getByText('Add Toast'))
    })

    expect(screen.getByText('Test toast')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(5000)
    })

    expect(screen.queryByText('Test toast')).not.toBeInTheDocument()
  })

  it('respects maxToasts limit', () => {
    render(
      <ToastProvider maxToasts={2}>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Toast'))
    fireEvent.click(screen.getByText('Add Success'))
    fireEvent.click(screen.getByText('Add Warning'))

    // Should only show last 2 toasts
    expect(screen.queryByText('Test toast')).not.toBeInTheDocument()
    expect(screen.getByText('Success!')).toBeInTheDocument()
    expect(screen.getByText('Warning!')).toBeInTheDocument()
  })

  it('applies position class to container', () => {
    const { container } = render(
      <ToastProvider position="bottom-left">
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Toast'))

    const toastContainer = container.querySelector('.toast-container--bottom-left')
    expect(toastContainer).toBeInTheDocument()
  })

  it('has proper ARIA attributes', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Toast'))
    
    const toast = screen.getByText('Test toast').closest('.toast')
    expect(toast).toHaveAttribute('role', 'alert')
    expect(toast).toHaveAttribute('aria-live', 'polite')
  })

  it('displays correct icons for variants', () => {
    const { container } = render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    fireEvent.click(screen.getByText('Add Success'))
    expect(container.querySelector('.toast__icon')).toHaveTextContent('✓')

    fireEvent.click(screen.getByText('Add Warning'))
    const icons = container.querySelectorAll('.toast__icon')
    expect(icons[1]).toHaveTextContent('⚠')
  })
})
