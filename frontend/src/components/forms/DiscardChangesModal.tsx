import React from 'react';
import './DiscardChangesModal.css';

interface DiscardChangesModalProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

/**
 * Modal confirmation dialog for discarding unsaved form changes
 */
export function DiscardChangesModal({
  isOpen,
  onConfirm,
  onCancel,
}: DiscardChangesModalProps) {
  if (!isOpen) return null;

  return (
    <div className="discard-changes-modal-overlay">
      <div className="discard-changes-modal">
        <h2 className="discard-changes-modal-title">Discard Changes?</h2>
        
        <p className="discard-changes-modal-message">
          You have unsaved changes. Are you sure you want to discard them?
        </p>

        <div className="discard-changes-modal-actions">
          <button
            type="button"
            onClick={onCancel}
            className="discard-changes-modal-button-cancel"
          >
            Keep Editing
          </button>
          <button
            type="button"
            onClick={onConfirm}
            className="discard-changes-modal-button-confirm"
          >
            Discard
          </button>
        </div>
      </div>
    </div>
  );
}
