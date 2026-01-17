import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react'
import './Toast.css'

export type ToastVariant = 'info' | 'success' | 'warning' | 'danger'
export type ToastPosition = 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left' | 'top-center' | 'bottom-center'

export interface Toast {
  id: string
  message: string
  variant?: ToastVariant
  duration?: number
  action?: {
    label: string
    onClick: () => void
  }
}

interface ToastContextType {
  toasts: Toast[]
  addToast: (toast: Omit<Toast, 'id'>) => string
  removeToast: (id: string) => void
  clearAll: () => void
}

const ToastContext = createContext<ToastContextType | undefined>(undefined)

export interface ToastProviderProps {
  children: ReactNode
  position?: ToastPosition
  maxToasts?: number
}

/**
 * Toast Provider Component
 *
 * Provides toast notification context to the application.
 * Wrap your app with this provider to enable toast notifications.
 *
 * @example
 * <ToastProvider position="top-right">
 *   <App />
 * </ToastProvider>
 */
export const ToastProvider: React.FC<ToastProviderProps> = ({
  children,
  position = 'top-right',
  maxToasts = 5,
}) => {
  const [toasts, setToasts] = useState<Toast[]>([])

  const addToast = useCallback(
    (toast: Omit<Toast, 'id'>) => {
      const id = Math.random().toString(36).substr(2, 9)
      const duration = toast.duration ?? 5000

      setToasts((prev) => {
        const newToasts = [...prev, { ...toast, id }]
        return newToasts.slice(-maxToasts)
      })

      if (duration > 0) {
        setTimeout(() => {
          removeToast(id)
        }, duration)
      }

      return id
    },
    [maxToasts]
  )

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id))
  }, [])

  const clearAll = useCallback(() => {
    setToasts([])
  }, [])

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast, clearAll }}>
      {children}
      <ToastContainer toasts={toasts} position={position} onClose={removeToast} />
    </ToastContext.Provider>
  )
}

interface ToastContainerProps {
  toasts: Toast[]
  position: ToastPosition
  onClose: (id: string) => void
}

const ToastContainer: React.FC<ToastContainerProps> = ({ toasts, position, onClose }) => {
  if (toasts.length === 0) return null

  return (
    <div className={`toast-container toast-container--${position}`}>
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} onClose={onClose} />
      ))}
    </div>
  )
}

interface ToastItemProps {
  toast: Toast
  onClose: (id: string) => void
}

const ToastItem: React.FC<ToastItemProps> = ({ toast, onClose }) => {
  const variant = toast.variant || 'info'

  const getIcon = () => {
    switch (variant) {
      case 'success':
        return '✓'
      case 'warning':
        return '⚠'
      case 'danger':
        return '✕'
      default:
        return 'ℹ'
    }
  }

  return (
    <div
      className={`toast toast--${variant}`}
      role="alert"
      aria-live="polite"
    >
      <div className="toast__icon" aria-hidden="true">
        {getIcon()}
      </div>

      <div className="toast__content">
        <div className="toast__message">{toast.message}</div>
        {toast.action && (
          <button
            type="button"
            className="toast__action"
            onClick={toast.action.onClick}
          >
            {toast.action.label}
          </button>
        )}
      </div>

      <button
        type="button"
        className="toast__close"
        onClick={() => onClose(toast.id)}
        aria-label="Close notification"
      >
        ×
      </button>
    </div>
  )
}

/**
 * useToast Hook
 *
 * Hook to access toast notification functions.
 * Must be used within ToastProvider.
 *
 * @example
 * const { addToast } = useToast()
 * addToast({ message: 'Saved!', variant: 'success' })
 */
export const useToast = () => {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast must be used within ToastProvider')
  }
  return context
}

export default ToastProvider
