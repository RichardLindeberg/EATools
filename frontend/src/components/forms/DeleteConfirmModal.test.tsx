/**
 * DeleteConfirmModal Component Tests
 * Tests the delete confirmation modal with approval_id and reason fields
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DeleteConfirmModal } from './DeleteConfirmModal';

describe('DeleteConfirmModal', () => {
  const mockOnConfirm = vi.fn();
  const mockOnCancel = vi.fn();

  beforeEach(() => {
    mockOnConfirm.mockClear();
    mockOnCancel.mockClear();
  });

  it('renders when isOpen is true', () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText(/Delete application/i)).toBeInTheDocument();
  });

  it('does not render when isOpen is false', () => {
    render(
      <DeleteConfirmModal
        isOpen={false}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    // Modal should not be visible - the title should not be in the document
    expect(screen.queryByText(/Delete application/i)).not.toBeInTheDocument();
  });

  it('shows approval_id and reason input fields', () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="server"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByLabelText(/Approval ID/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Reason/i)).toBeInTheDocument();
  });

  it('calls onCancel when cancel button is clicked', async () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await userEvent.click(cancelButton);

    expect(mockOnCancel).toHaveBeenCalled();
  });

  it('shows error when fields are empty and confirm is clicked', async () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });
    await userEvent.click(confirmButton);

    expect(screen.getByText(/required/i)).toBeInTheDocument();
    expect(mockOnConfirm).not.toHaveBeenCalled();
  });

  it('calls onConfirm with approval_id and reason when valid', async () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const approvalIdInput = screen.getByLabelText(/Approval ID/i);
    const reasonInput = screen.getByLabelText(/Reason/i);
    const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });

    await userEvent.type(approvalIdInput, 'APPR-001');
    await userEvent.type(reasonInput, 'Redundant system');
    await userEvent.click(confirmButton);

    await waitFor(() => {
      expect(mockOnConfirm).toHaveBeenCalledWith('APPR-001', 'Redundant system');
    });
  });

  it('disables confirm button when loading is true', () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        loading={true}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    const approvalIdInput = screen.getByLabelText(/Approval ID/i);
    const reasonInput = screen.getByLabelText(/Reason/i);

    // Cancel button should be disabled
    expect(cancelButton).toBeDisabled();
    // Input fields should be disabled
    expect(approvalIdInput).toBeDisabled();
    expect(reasonInput).toBeDisabled();
  });

  it('clears fields when modal is closed and reopened', async () => {
    const { rerender } = render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const approvalIdInput = screen.getByLabelText(/Approval ID/i) as HTMLInputElement;
    await userEvent.type(approvalIdInput, 'APPR-001');

    // Close modal
    rerender(
      <DeleteConfirmModal
        isOpen={false}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    // Reopen modal
    rerender(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const newApprovalIdInput = screen.getByLabelText(/Approval ID/i) as HTMLInputElement;
    expect(newApprovalIdInput.value).toBe('');
  });

  it('uses default entity label when not provided', () => {
    render(
      <DeleteConfirmModal isOpen={true} onConfirm={mockOnConfirm} onCancel={mockOnCancel} />
    );

    expect(screen.getByText(/Delete entity/i)).toBeInTheDocument();
  });

  it('shows custom entity label', () => {
    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="data entity"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByText(/Delete data entity/i)).toBeInTheDocument();
  });

  it('handles async onConfirm callback', async () => {
    const asyncOnConfirm = vi.fn().mockResolvedValue(undefined);

    render(
      <DeleteConfirmModal
        isOpen={true}
        entityLabel="application"
        onConfirm={asyncOnConfirm}
        onCancel={mockOnCancel}
      />
    );

    const approvalIdInput = screen.getByLabelText(/Approval ID/i);
    const reasonInput = screen.getByLabelText(/Reason/i);
    const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });

    await userEvent.type(approvalIdInput, 'APPR-001');
    await userEvent.type(reasonInput, 'System cleanup');
    await userEvent.click(confirmButton);

    await waitFor(() => {
      expect(asyncOnConfirm).toHaveBeenCalledWith('APPR-001', 'System cleanup');
    });
  });
});
