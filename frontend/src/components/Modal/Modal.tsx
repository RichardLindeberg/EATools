import React, { useEffect, ReactNode } from 'react'
import './Modal.css'

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl'

export interface ModalProps {
  /**
   * Modal visibility
   */
  isOpen: boolean

  /**
   * Close handler
   */
  onClose: () => void

  /**
   * Modal title
   */
  title?: string

  /**
   * Modal content
   */
  children: ReactNode

  /**
   * Footer content
   */
  footer?: ReactNode

  /**
   * Modal size
   */
  size?: ModalSize

  /**
   * Prevent closing on backdrop click
   */
  disableBackdropClick?: boolean

  /**
   * Prevent closing on Escape key
   */
  disableEscapeKey?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Modal Component
 *
 * A modal dialog with backdrop, accessible keyboard navigation,
 * and focus management. Follows WCAG 2.1 AA standards.
 *
 * @example
 * <Modal
 *   isOpen={showModal}
 *   onClose={() => setShowModal(false)}
 *   title="Confirm Action"
 *   footer={<Button onClick={handleSave}>Save</Button>}
 * >
 *   <p>Are you sure?</p>
 * </Modal>
 */
export const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  footer,
  size = 'md',
  disableBackdropClick = false,
  disableEscapeKey = false,
  className,
}) => {
  useEffect(() => {
    if (!isOpen) return

    const handleEscape = (event: KeyboardEvent) => {
      if (!disableEscapeKey && event.key === 'Escape') {
        onClose()
      }
    }

    document.addEventListener('keydown', handleEscape)
    document.body.style.overflow = 'hidden'

    return () => {
      document.removeEventListener('keydown', handleEscape)
      document.body.style.overflow = ''
    }
  }, [isOpen, onClose, disableEscapeKey])

  if (!isOpen) return null

  const handleBackdropClick = (event: React.MouseEvent) => {
    if (!disableBackdropClick && event.target === event.currentTarget) {
      onClose()
    }
  }

  const modalClasses = ['modal__content', `modal__content--${size}`, className]
    .filter(Boolean)
    .join(' ')

  return (
    <div className="modal" onClick={handleBackdropClick}>
      <div
        className={modalClasses}
        role="dialog"
        aria-modal="true"
        aria-labelledby={title ? 'modal-title' : undefined}
      >
        {title && (
          <div className="modal__header">
            <h2 id="modal-title" className="modal__title">
              {title}
            </h2>
            <button
              type="button"
              className="modal__close"
              onClick={onClose}
              aria-label="Close modal"
            >
              ×
            </button>
          </div>
        )}

        <div className="modal__body">{children}</div>

        {footer && <div className="modal__footer">{footer}</div>}
      </div>
    </div>
  )
}

export default Modal
