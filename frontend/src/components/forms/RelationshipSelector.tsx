import React, { useCallback } from 'react';
import { useRelationshipSearch } from '../../hooks/useEntityForm';
import { FormFieldWrapper } from './FormFieldWrapper';
import './RelationshipSelector.css';

interface RelationshipSelectorProps {
  label: string;
  entityType: string;
  value: string | string[];
  onChange: (value: string | string[]) => void;
  error?: string;
  required?: boolean;
  multiple?: boolean;
  disabled?: boolean;
}

/**
 * Component for selecting related entities
 * Provides searchable entity selection
 */
export function RelationshipSelector({
  label,
  entityType,
  value,
  onChange,
  error,
  required = false,
  multiple = false,
  disabled = false,
}: RelationshipSelectorProps) {
  const { searchTerm, results, isLoading, search } = useRelationshipSearch(entityType);
  const [isOpen, setIsOpen] = React.useState(false);

  const handleSearch = useCallback(
    (term: string) => {
      search(term);
      setIsOpen(true);
    },
    [search]
  );

  const handleSelect = (selectedId: string) => {
    if (multiple) {
      const currentValues = Array.isArray(value) ? value : [];
      if (currentValues.includes(selectedId)) {
        onChange(currentValues.filter((id) => id !== selectedId));
      } else {
        onChange([...currentValues, selectedId]);
      }
    } else {
      onChange(selectedId);
      setIsOpen(false);
    }
  };

  const handleRemove = (id: string) => {
    if (multiple) {
      const currentValues = Array.isArray(value) ? value : [];
      onChange(currentValues.filter((v) => v !== id));
    }
  };

  const selectedIds = Array.isArray(value) ? value : value ? [value] : [];

  return (
    <FormFieldWrapper label={label} required={required} error={error}>
      <div className="relationship-selector">
        <input
          type="text"
          placeholder={`Search ${entityType}...`}
          value={searchTerm}
          onChange={(e) => handleSearch(e.target.value)}
          onFocus={() => setIsOpen(true)}
          disabled={disabled}
          className="relationship-selector-input"
        />

        {isLoading && <div className="relationship-selector-loading">Loading...</div>}

        {isOpen && !isLoading && results.length > 0 && (
          <div className="relationship-selector-dropdown">
            {results.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => handleSelect(item.id)}
                className={`relationship-selector-option ${
                  selectedIds.includes(item.id) ? 'selected' : ''
                }`}
              >
                {multiple && (
                  <input
                    type="checkbox"
                    checked={selectedIds.includes(item.id)}
                    onChange={() => {}}
                  />
                )}
                {item.name || item.title || String(item.id).substring(0, 8)}
              </button>
            ))}
          </div>
        )}

        {multiple && selectedIds.length > 0 && (
          <div className="relationship-selector-selected">
            {selectedIds.map((id) => (
              <span key={id} className="relationship-selector-tag">
                {results.find((r) => r.id === id)?.name || id}
                <button
                  type="button"
                  onClick={() => handleRemove(id)}
                  className="relationship-selector-tag-remove"
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        )}
      </div>
    </FormFieldWrapper>
  );
}
