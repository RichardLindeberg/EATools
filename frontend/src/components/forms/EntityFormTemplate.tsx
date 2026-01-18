import React from 'react';
import './EntityFormTemplate.css';

interface EntityFormTemplateProps {
  title: string;
  subtitle?: string;
  isEdit?: boolean;
  isLoading?: boolean;
  error?: string | null;
  children: React.ReactNode;
  onSubmit: (e: React.FormEvent) => void;
  onCancel: () => void;
  submitButtonLabel?: string;
  submitButtonSecondary?: {
    label: string;
    onClick: () => void;
  };
}

/**
 * Reusable template for entity create/edit forms
 * Provides consistent layout, header, form sections, and action buttons
 */
export function EntityFormTemplate({
  title,
  subtitle,
  isEdit = false,
  isLoading = false,
  error,
  children,
  onSubmit,
  onCancel,
  submitButtonLabel = isEdit ? 'Save Changes' : 'Create',
  submitButtonSecondary,
}: EntityFormTemplateProps) {
  return (
    <div className="entity-form-template">
      <div className="entity-form-header">
        <div>
          <h1 className="entity-form-title">{title}</h1>
          {subtitle && <p className="entity-form-subtitle">{subtitle}</p>}
        </div>
      </div>

      {error && <div className="entity-form-error-banner">{error}</div>}

      <form onSubmit={onSubmit} className="entity-form">
        <div className="entity-form-content">{children}</div>

        <div className="entity-form-actions">
          <button type="button" onClick={onCancel} className="entity-form-button-cancel">
            Cancel
          </button>

          {submitButtonSecondary && (
            <button
              type="button"
              onClick={submitButtonSecondary.onClick}
              className="entity-form-button-secondary"
              disabled={isLoading}
            >
              {submitButtonSecondary.label}
            </button>
          )}

          <button
            type="submit"
            className="entity-form-button-submit"
            disabled={isLoading}
          >
            {isLoading ? 'Saving...' : submitButtonLabel}
          </button>
        </div>
      </form>
    </div>
  );
}
