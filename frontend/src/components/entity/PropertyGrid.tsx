/**
 * PropertyGrid Component
 * Displays entity properties in a responsive grid layout
 */

import React from 'react';
import './PropertyGrid.css';

export interface Property {
  key: string;
  label: string;
  value: any;
  format?: (value: any) => React.ReactNode;
  span?: 1 | 2 | 3; // How many columns to span (default: 1)
}

interface PropertyGridProps {
  properties: Property[];
  columns?: 2 | 3; // Number of columns (default: 2)
}

export const PropertyGrid: React.FC<PropertyGridProps> = ({ properties, columns = 2 }) => {
  const formatValue = (property: Property): React.ReactNode => {
    if (property.value === null || property.value === undefined) {
      return <span className="property-empty">—</span>;
    }

    if (property.format) {
      return property.format(property.value);
    }

    // Default formatting
    if (typeof property.value === 'boolean') {
      return property.value ? 'Yes' : 'No';
    }

    if (property.value instanceof Date) {
      return property.value.toLocaleDateString();
    }

    if (typeof property.value === 'string' && property.value.match(/^\d{4}-\d{2}-\d{2}T/)) {
      // ISO date string
      return new Date(property.value).toLocaleString();
    }

    if (Array.isArray(property.value)) {
      return property.value.join(', ');
    }

    if (typeof property.value === 'object') {
      return JSON.stringify(property.value, null, 2);
    }

    return String(property.value);
  };

  return (
    <div className={`property-grid property-grid-${columns}`}>
      {properties.map((property) => (
        <div
          key={property.key}
          className={`property-item ${property.span ? `span-${property.span}` : ''}`}
        >
          <dt className="property-label">{property.label}</dt>
          <dd className="property-value">{formatValue(property)}</dd>
        </div>
      ))}
    </div>
  );
};
