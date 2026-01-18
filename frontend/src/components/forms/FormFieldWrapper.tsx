import React from 'react';
import './FormFieldWrapper.css';

interface FormFieldWrapperProps {
  label: string;
  required?: boolean;
  error?: string;
  helpText?: string;
  children: React.ReactNode;
  htmlFor?: string;
}

/**
 * Wrapper component for form fields
 * Provides consistent styling, error display, help text, and required indicators
 */
export function FormFieldWrapper({
  label,
  required = false,
  error,
  helpText,
  children,
  htmlFor,
}: FormFieldWrapperProps) {
  return (
    <div className="form-field-wrapper">
      <label htmlFor={htmlFor} className="form-field-label">
        {label}
        {required && <span className="form-field-required">*</span>}
      </label>

      <div className="form-field-input-wrapper">{children}</div>

      {error && <div className="form-field-error">{error}</div>}

      {helpText && !error && <div className="form-field-help-text">{helpText}</div>}
    </div>
  );
}
