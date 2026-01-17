/**
 * FilterPanel Component
 * Dynamic filter UI for entity list pages
 */

import React, { useState } from 'react';
import { Button } from '../Button/Button';
import { Input } from '../Input/Input';
import { Select } from '../Select/Select';
import './FilterPanel.css';

export interface FilterDefinition {
  key: string;
  label: string;
  type: 'select' | 'checkbox' | 'date-range' | 'number-range' | 'search';
  options?: Array<{ value: string; label: string }>;
  placeholder?: string;
}

export interface FilterValues {
  [key: string]: string | string[] | { min?: number; max?: number } | { start?: string; end?: string };
}

export interface FilterPanelProps {
  filters: FilterDefinition[];
  values: FilterValues;
  onFilterChange: (key: string, value: any) => void;
  onClearFilters: () => void;
  loading?: boolean;
  onApply?: () => void;
  expanded?: boolean;
}

export const FilterPanel: React.FC<FilterPanelProps> = ({
  filters,
  values,
  onFilterChange,
  onClearFilters,
  loading = false,
  onApply,
  expanded = true,
}) => {
  const [isExpanded, setIsExpanded] = useState(expanded);
  const activeFilterCount = Object.values(values).filter(
    (v) => v !== undefined && v !== '' && v !== null
  ).length;

  const renderFilterInput = (filter: FilterDefinition) => {
    const value = values[filter.key];

    switch (filter.type) {
      case 'select':
        return (
          <Select
            key={filter.key}
            label={filter.label}
            options={filter.options || []}
            value={(value as string) || ''}
            onChange={(e) => onFilterChange(filter.key, e.target.value)}
            disabled={loading}
            placeholder={filter.placeholder}
          />
        );

      case 'checkbox':
        return (
          <div key={filter.key} className="filter-panel__checkbox-group">
            <label className="filter-panel__label">{filter.label}</label>
            <div className="filter-panel__checkbox-options">
              {(filter.options || []).map((option) => (
                <label key={option.value} className="filter-panel__checkbox">
                  <input
                    type="checkbox"
                    value={option.value}
                    checked={
                      Array.isArray(value)
                        ? value.includes(option.value)
                        : false
                    }
                    onChange={(e) => {
                      const checked = e.target.checked;
                      const currentValues = Array.isArray(value) ? [...value] : [];
                      if (checked) {
                        currentValues.push(option.value);
                      } else {
                        currentValues.splice(
                          currentValues.indexOf(option.value),
                          1
                        );
                      }
                      onFilterChange(filter.key, currentValues);
                    }}
                    disabled={loading}
                  />
                  <span>{option.label}</span>
                </label>
              ))}
            </div>
          </div>
        );

      case 'date-range':
        const dateValue = value as { start?: string; end?: string } | undefined;
        return (
          <div key={filter.key} className="filter-panel__date-range">
            <label className="filter-panel__label">{filter.label}</label>
            <div className="filter-panel__date-inputs">
              <Input
                type="date"
                placeholder="From"
                value={dateValue?.start || ''}
                onChange={(e) =>
                  onFilterChange(filter.key, {
                    ...dateValue,
                    start: e.target.value,
                  })
                }
                disabled={loading}
              />
              <Input
                type="date"
                placeholder="To"
                value={dateValue?.end || ''}
                onChange={(e) =>
                  onFilterChange(filter.key, {
                    ...dateValue,
                    end: e.target.value,
                  })
                }
                disabled={loading}
              />
            </div>
          </div>
        );

      case 'number-range':
        const numberValue = value as { min?: number; max?: number } | undefined;
        return (
          <div key={filter.key} className="filter-panel__number-range">
            <label className="filter-panel__label">{filter.label}</label>
            <div className="filter-panel__number-inputs">
              <Input
                type="number"
                placeholder="Min"
                value={numberValue?.min ?? ''}
                onChange={(e) =>
                  onFilterChange(filter.key, {
                    ...numberValue,
                    min: e.target.value ? Number(e.target.value) : undefined,
                  })
                }
                disabled={loading}
              />
              <Input
                type="number"
                placeholder="Max"
                value={numberValue?.max ?? ''}
                onChange={(e) =>
                  onFilterChange(filter.key, {
                    ...numberValue,
                    max: e.target.value ? Number(e.target.value) : undefined,
                  })
                }
                disabled={loading}
              />
            </div>
          </div>
        );

      case 'search':
      default:
        return (
          <Input
            key={filter.key}
            label={filter.label}
            placeholder={filter.placeholder || 'Search...'}
            value={(value as string) || ''}
            onChange={(e) => onFilterChange(filter.key, e.target.value)}
            disabled={loading}
          />
        );
    }
  };

  return (
    <div className="filter-panel">
      <div className="filter-panel__header">
        <button
          className="filter-panel__toggle"
          onClick={() => setIsExpanded(!isExpanded)}
          aria-expanded={isExpanded}
        >
          <span className="filter-panel__toggle-icon">
            {isExpanded ? '▼' : '▶'}
          </span>
          <span className="filter-panel__toggle-label">Filters</span>
          {activeFilterCount > 0 && (
            <span className="filter-panel__badge">{activeFilterCount}</span>
          )}
        </button>

        {activeFilterCount > 0 && (
          <Button
            variant="secondary"
            size="sm"
            onClick={onClearFilters}
            disabled={loading}
          >
            Clear All
          </Button>
        )}
      </div>

      {isExpanded && (
        <div className="filter-panel__content">
          <div className="filter-panel__grid">
            {filters.map((filter) => renderFilterInput(filter))}
          </div>

          {onApply && (
            <div className="filter-panel__footer">
              <Button
                variant="primary"
                size="sm"
                onClick={onApply}
                disabled={loading}
              >
                Apply Filters
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
