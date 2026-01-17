/**
 * BulkActionBar Component
 * Toolbar for bulk actions on selected items
 */

import React from 'react';
import { Button } from '../Button/Button';
import './BulkActionBar.css';

export interface BulkActionBarProps {
  selectedCount: number;
  totalCount: number;
  onDelete?: () => void;
  onArchive?: () => void;
  onExport?: () => void;
  onClearSelection: () => void;
  loading?: boolean;
  visible?: boolean;
}

export const BulkActionBar: React.FC<BulkActionBarProps> = ({
  selectedCount,
  totalCount,
  onDelete,
  onArchive,
  onExport,
  onClearSelection,
  loading = false,
  visible = true,
}) => {
  if (!visible || selectedCount === 0) {
    return null;
  }

  return (
    <div className="bulk-action-bar">
      <div className="bulk-action-bar__info">
        <span className="bulk-action-bar__count">
          {selectedCount} of {totalCount} selected
        </span>
      </div>

      <div className="bulk-action-bar__actions">
        {onExport && (
          <Button variant="secondary" size="sm" onClick={onExport} disabled={loading}>
            Export ({selectedCount})
          </Button>
        )}

        {onArchive && (
          <Button variant="secondary" size="sm" onClick={onArchive} disabled={loading}>
            Archive ({selectedCount})
          </Button>
        )}

        {onDelete && (
          <Button
            variant="danger"
            size="sm"
            onClick={onDelete}
            disabled={loading}
          >
            Delete ({selectedCount})
          </Button>
        )}

        <Button
          variant="secondary"
          size="sm"
          onClick={onClearSelection}
          disabled={loading}
        >
          Clear Selection
        </Button>
      </div>
    </div>
  );
};
