import React, { useState, useEffect } from 'react';
import { Modal } from '../Modal/Modal';
import { FormFieldWrapper } from './FormFieldWrapper';
import { Button } from '../Button/Button';
import './DeleteConfirmModal.css';

interface DeleteConfirmModalProps {
  isOpen: boolean;
  entityLabel?: string;
  loading?: boolean;
  onConfirm: (approvalId: string, reason: string) => Promise<void> | void;
  onCancel: () => void;
}

/**
 * Confirmation modal that captures approval_id and reason before performing a delete.
 */
export function DeleteConfirmModal({
  isOpen,
  entityLabel = 'entity',
  loading = false,
  onConfirm,
  onCancel,
}: DeleteConfirmModalProps) {
  const [approvalId, setApprovalId] = useState('');
  const [reason, setReason] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setApprovalId('');
      setReason('');
      setError(null);
    }
  }, [isOpen]);

  const handleConfirm = async () => {
    if (!approvalId || !reason) {
      setError('Approval ID and reason are required.');
      return;
    }

    setError(null);
    await onConfirm(approvalId, reason);
  };

  return (
    <Modal isOpen={isOpen} onClose={onCancel} size="md" title={`Delete ${entityLabel}`}>\
      <div className="delete-confirm-modal">
        <p className="delete-confirm-text">
          Deleting this {entityLabel} requires an approval ID and a reason for audit purposes.
        </p>

        {error && <div className="delete-confirm-error">{error}</div>}

        <FormFieldWrapper label="Approval ID" required htmlFor="approvalId">
          <input
            id="approvalId"
            type="text"
            value={approvalId}
            onChange={(e) => setApprovalId(e.target.value)}
            placeholder="Enter approval reference"
            disabled={loading}
          />
        </FormFieldWrapper>

        <FormFieldWrapper
          label="Reason"
          required
          htmlFor="deleteReason"
          helpText="Provide a concise reason for deletion"
        >
          <textarea
            id="deleteReason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Reason for deletion"
            disabled={loading}
          />
        </FormFieldWrapper>

        <div className="delete-confirm-actions">
          <Button variant="secondary" onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleConfirm} loading={loading}>
            Confirm Delete
          </Button>
        </div>
      </div>
    </Modal>
  );
}
