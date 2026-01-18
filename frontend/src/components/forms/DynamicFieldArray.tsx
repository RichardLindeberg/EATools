import React from 'react';
import { FormFieldWrapper } from './FormFieldWrapper';
import './DynamicFieldArray.css';

interface DynamicFieldArrayProps {
  label: string;
  value: string[];
  onChange: (value: string[]) => void;
  error?: string;
  required?: boolean;
  placeholder?: string;
  maxItems?: number;
  disabled?: boolean;
}

/**
 * Component for managing dynamic arrays of text fields
 * Used for tags, multi-select, repeating fields
 */
export function DynamicFieldArray({
  label,
  value = [],
  onChange,
  error,
  required = false,
  placeholder = 'Enter value and press Enter',
  maxItems,
  disabled = false,
}: DynamicFieldArrayProps) {
  const [inputValue, setInputValue] = React.useState('');

  const handleAddItem = () => {
    if (!inputValue.trim()) return;
    if (maxItems && value.length >= maxItems) return;

    const newValue = [...value, inputValue.trim()];
    onChange(newValue);
    setInputValue('');
  };

  const handleRemoveItem = (index: number) => {
    const newValue = value.filter((_, i) => i !== index);
    onChange(newValue);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddItem();
    }
  };

  const isFull = maxItems && value.length >= maxItems;

  return (
    <FormFieldWrapper label={label} required={required} error={error}>
      <div className="dynamic-field-array">
        <div className="dynamic-field-input-group">
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            disabled={disabled || isFull}
            className="dynamic-field-input"
          />
          <button
            type="button"
            onClick={handleAddItem}
            disabled={disabled || isFull || !inputValue.trim()}
            className="dynamic-field-add-button"
          >
            +
          </button>
        </div>

        {value.length > 0 && (
          <div className="dynamic-field-items">
            {value.map((item, index) => (
              <div key={index} className="dynamic-field-item">
                <span className="dynamic-field-item-text">{item}</span>
                <button
                  type="button"
                  onClick={() => handleRemoveItem(index)}
                  disabled={disabled}
                  className="dynamic-field-item-remove"
                  aria-label={`Remove ${item}`}
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        )}

        {maxItems && (
          <div className="dynamic-field-info">
            {value.length} / {maxItems} items
          </div>
        )}
      </div>
    </FormFieldWrapper>
  );
}
