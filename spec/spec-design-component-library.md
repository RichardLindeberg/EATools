---
title: Component Library & Implementation Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [components, library, typescript, react]
---

# Component Library & Implementation Specification

## 1. Purpose & Scope

This specification defines the detailed API, props, state, and composition for all UI components. It serves as the contract between component developers and feature developers.

## 2. Component Structure Pattern

### TypeScript Component Template
```typescript
import React, { FC, ReactNode, CSSProperties } from 'react';

interface ComponentProps {
  // Core props
  id?: string;
  className?: string;
  style?: CSSProperties;
  
  // Behavior
  disabled?: boolean;
  loading?: boolean;
  
  // Content
  children?: ReactNode;
  
  // Callbacks
  onClick?: () => void;
  onChange?: (value: any) => void;
  onError?: (error: Error) => void;
}

interface ComponentState {
  // Internal state tracked by component
}

const Component: FC<ComponentProps> = ({
  id,
  className = '',
  style,
  disabled = false,
  loading = false,
  children,
  onClick,
  onChange,
  onError,
}) => {
  // Implementation
  return <div className={className} style={style}>{children}</div>;
};

export default Component;
```

## 3. Navigation Components

### Header Component
```typescript
interface HeaderProps {
  // Layout
  logoSrc: string;
  logoAlt: string;
  title: string;
  
  // Search
  onSearch?: (query: string) => void;
  searchPlaceholder?: string;
  
  // User Menu
  userDisplayName: string;
  userAvatar?: string;
  onUserClick?: () => void;
  onLogout?: () => void;
  
  // Navigation
  breadcrumbs?: BreadcrumbItem[];
  
  // Styling
  sticky?: boolean;
  backgroundColor?: string;
}

interface BreadcrumbItem {
  label: string;
  path?: string;
  onClick?: () => void;
}

// Behavior:
// - Fixed height: 60px
// - Sticky by default
// - Search debounced 300ms
// - User menu dropdown on click
// - Breadcrumbs auto-truncate on small screens
```

### Sidebar Navigation Component
```typescript
interface SidebarProps {
  // Menu items
  items: MenuItem[];
  activeItemId?: string;
  onItemClick?: (itemId: string, path: string) => void;
  
  // State
  collapsed?: boolean;
  onCollapsedChange?: (collapsed: boolean) => void;
  
  // Styling
  backgroundColor?: string;
  width?: number; // default 250px
}

interface MenuItem {
  id: string;
  label: string;
  icon?: ReactNode;
  path: string;
  badge?: string | number;
  children?: MenuItem[];
  disabled?: boolean;
}

// Behavior:
// - Collapsible to icon-only view
// - Active item highlighted
// - Nested items expandable
// - Keyboard navigation (arrow keys, enter)
// - Smooth collapse animation
```

### Breadcrumbs Component
```typescript
interface BreadcrumbsProps {
  items: BreadcrumbItem[];
  separator?: ReactNode; // default '/'
  maxItems?: number; // default 5, rest collapsed
  onNavigate?: (path: string) => void;
}

// Behavior:
// - Responsive truncation
// - Ellipsis for collapsed items
// - Hover shows full path
// - Last item non-clickable
```

### Tabs Component
```typescript
interface TabsProps {
  items: TabItem[];
  activeTabId: string;
  onTabChange: (tabId: string) => void;
  variant?: 'normal' | 'card';
  lazy?: boolean; // only render active tab content
}

interface TabItem {
  id: string;
  label: string;
  icon?: ReactNode;
  content: ReactNode;
  disabled?: boolean;
  badge?: string;
}

// Behavior:
// - Scrollable tab list on overflow
// - Keyboard: arrow keys navigate, enter selects
// - Smooth content transition
```

## 4. Data Display Components

### Table Component
```typescript
interface TableProps<T> {
  // Data
  data: T[];
  columns: TableColumn<T>[];
  
  // Behavior
  selectable?: boolean;
  selectedIds?: string[];
  onSelectionChange?: (ids: string[]) => void;
  
  sortable?: boolean;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
  onSort?: (column: string, direction: 'asc' | 'desc') => void;
  
  // Pagination
  paginated?: boolean;
  pageSize?: number;
  currentPage?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  
  // Row actions
  onRowClick?: (row: T) => void;
  rowActions?: RowAction<T>[];
  
  // State
  loading?: boolean;
  error?: string;
  
  // Styling
  striped?: boolean;
  hover?: boolean;
}

interface TableColumn<T> {
  key: keyof T;
  header: string;
  width?: string;
  sortable?: boolean;
  render?: (value: any, row: T) => ReactNode;
  align?: 'left' | 'center' | 'right';
}

interface RowAction<T> {
  label: string;
  icon?: ReactNode;
  onClick: (row: T) => void;
  variant?: 'primary' | 'secondary' | 'danger';
  disabled?: (row: T) => boolean;
}

// Behavior:
// - Virtual scrolling for 1000+ rows
// - Sticky header on scroll
// - Column resize handles
// - Context menu on right-click rows
// - Keyboard: arrow keys navigate, space selects
```

### Card Component
```typescript
interface CardProps {
  // Layout
  image?: string;
  imageAlt?: string;
  title: string;
  subtitle?: string;
  description?: string;
  
  // Content
  children?: ReactNode;
  
  // Footer
  footer?: ReactNode;
  
  // Actions
  actions?: CardAction[];
  
  // State
  selected?: boolean;
  onSelected?: (selected: boolean) => void;
  
  // Styling
  clickable?: boolean;
  shadow?: 'none' | 'sm' | 'md' | 'lg';
  borderRadius?: string;
}

interface CardAction {
  label: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary';
}

// Behavior:
// - Image lazy loading
// - Hover elevation on clickable cards
// - Responsive image sizing
// - Footer sticky at bottom
```

### List Component
```typescript
interface ListProps<T> {
  items: T[];
  renderItem: (item: T, index: number) => ReactNode;
  
  // Behavior
  selectable?: boolean;
  selectedIds?: string[];
  onSelectionChange?: (ids: string[]) => void;
  
  // Pagination
  paginated?: boolean;
  pageSize?: number;
  
  // State
  loading?: boolean;
  error?: string;
  emptyMessage?: string;
}

// Behavior:
// - Virtual scrolling for large lists
// - Smooth item animations
// - Keyboard navigation
```

## 5. Input Components

### Text Input Component
```typescript
interface TextInputProps {
  // Value
  value: string;
  onChange: (value: string) => void;
  
  // Validation
  error?: string;
  errorMessage?: string;
  required?: boolean;
  
  // Appearance
  label?: string;
  placeholder?: string;
  icon?: ReactNode;
  clearable?: boolean;
  
  // Behavior
  disabled?: boolean;
  readOnly?: boolean;
  type?: 'text' | 'password' | 'email' | 'number' | 'tel' | 'url';
  
  // Callbacks
  onFocus?: () => void;
  onBlur?: () => void;
  onClear?: () => void;
  
  // Validation
  maxLength?: number;
  pattern?: RegExp;
  validate?: (value: string) => string | null;
}

// Behavior:
// - Real-time validation on blur
// - Inline error display
// - Character counter (if maxLength)
// - Keyboard: enter submits form context
```

### Select Component
```typescript
interface SelectProps<T> {
  // Options
  options: SelectOption<T>[];
  optionLabel?: (option: T) => string;
  optionValue?: (option: T) => string | number;
  
  // Value
  value?: T | T[];
  onChange: (value: T | T[]) => void;
  
  // Multi-select
  multiple?: boolean;
  
  // Appearance
  label?: string;
  placeholder?: string;
  searchable?: boolean;
  clearable?: boolean;
  
  // Validation
  required?: boolean;
  error?: string;
  
  // Behavior
  disabled?: boolean;
  
  // Callbacks
  onSearch?: (query: string) => void;
}

interface SelectOption<T> {
  label: string;
  value: T;
  disabled?: boolean;
  groupLabel?: string;
}

// Behavior:
// - Filterable dropdown
// - Group support
// - Keyboard: arrow keys, enter select, escape close
// - Multi-select with tags
// - Async option loading
```

### Checkbox Component
```typescript
interface CheckboxProps {
  // Value
  checked: boolean;
  onChange: (checked: boolean) => void;
  
  // Appearance
  label?: string;
  
  // States
  disabled?: boolean;
  indeterminate?: boolean;
  error?: string;
  
  // Behavior
  required?: boolean;
}

// Behavior:
// - Supports indeterminate state (for select all)
// - Keyboard: space toggles
// - High contrast outline on focus
```

### Radio Group Component
```typescript
interface RadioGroupProps<T> {
  options: RadioOption<T>[];
  value: T;
  onChange: (value: T) => void;
  
  label?: string;
  disabled?: boolean;
  error?: string;
  
  layout?: 'vertical' | 'horizontal';
}

interface RadioOption<T> {
  label: string;
  value: T;
  disabled?: boolean;
}

// Behavior:
// - Vertical or horizontal layout
// - Keyboard: arrow keys navigate, enter select
```

### Date Picker Component
```typescript
interface DatePickerProps {
  value: Date | null;
  onChange: (date: Date | null) => void;
  
  label?: string;
  placeholder?: string;
  
  // Range support
  range?: boolean;
  startDate?: Date;
  endDate?: Date;
  onRangeChange?: (start: Date, end: Date) => void;
  
  // Constraints
  minDate?: Date;
  maxDate?: Date;
  disabledDates?: Date[];
  
  // Behavior
  disabled?: boolean;
  clearable?: boolean;
  
  // Format
  format?: string; // default 'YYYY-MM-DD'
}

// Behavior:
// - Inline calendar picker or input
// - Range selection
// - Keyboard: arrow keys navigate, enter select
// - Preset ranges (Today, This Week, This Month, etc.)
```

### Tags Input Component
```typescript
interface TagsInputProps {
  tags: string[];
  onChange: (tags: string[]) => void;
  
  placeholder?: string;
  label?: string;
  
  // Suggestions
  suggestions?: string[];
  onSearch?: (query: string) => void;
  
  // Constraints
  maxTags?: number;
  maxTagLength?: number;
  
  // Validation
  allowDuplicates?: boolean;
  allowSpaces?: boolean;
  
  disabled?: boolean;
  error?: string;
}

// Behavior:
// - Add by typing + enter or comma
// - Remove by backspace or X button
// - Autocomplete suggestions
// - Keyboard: arrow keys navigate suggestions
```

## 6. Feedback Components

### Alert Component
```typescript
interface AlertProps {
  // Appearance
  variant: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  
  // Content
  children?: ReactNode;
  icon?: ReactNode;
  
  // Actions
  actions?: AlertAction[];
  
  // Dismissible
  dismissible?: boolean;
  onDismiss?: () => void;
  autoClose?: number; // ms
}

interface AlertAction {
  label: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary';
}

// Behavior:
// - Auto-close after duration
// - Dismissible with X
// - Auto-announce to screen readers
```

### Toast Component (Notification)
```typescript
interface ToastProps {
  id: string;
  variant: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  
  action?: {
    label: string;
    onClick: () => void;
  };
  
  duration?: number; // default 5000ms
  onClose?: () => void;
}

// Behavior:
// - Auto-dismiss after duration
// - Queue at top-right
// - Max 3 toasts visible
// - Swipe to dismiss
// - Keyboard: ESC dismisses top toast
// - ARIA live region for announcements
```

### Modal/Dialog Component
```typescript
interface ModalProps {
  // Visibility
  open: boolean;
  onClose: () => void;
  
  // Content
  title: string;
  children: ReactNode;
  
  // Actions
  actions?: ModalAction[];
  
  // Behavior
  dismissible?: boolean;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  
  // Callbacks
  onConfirm?: () => void;
  onCancel?: () => void;
}

interface ModalAction {
  label: string;
  onClick: () => void;
  variant?: 'primary' | 'secondary' | 'danger';
  loading?: boolean;
}

// Behavior:
// - Fade in/out animation
// - Focus trap
// - Scroll lock on body
// - Keyboard: ESC to close, Tab cycles focus
// - Default focus on primary action
// - Backdrop click dismisses (if dismissible)
```

### Popover Component
```typescript
interface PopoverProps {
  // Visibility
  open: boolean;
  onClose?: () => void;
  
  // Content
  title?: string;
  children: ReactNode;
  
  // Position
  anchor: HTMLElement | null;
  placement?: 'top' | 'bottom' | 'left' | 'right';
  offset?: { x: number; y: number };
  
  // Behavior
  closeOnClickOutside?: boolean;
  closeOnEscape?: boolean;
}

// Behavior:
// - Auto-position to stay in viewport
// - Arrow pointing to anchor
// - Smooth animation
```

### Tooltip Component
```typescript
interface TooltipProps {
  content: ReactNode;
  children: ReactNode;
  
  placement?: 'top' | 'bottom' | 'left' | 'right';
  delay?: number; // default 500ms
  
  // Behavior
  disabled?: boolean;
}

// Behavior:
// - Show on hover after delay
// - Hide on mouse leave
// - Arrow pointing to element
// - Smart positioning (avoid viewport edges)
```

## 7. Action Components

### Button Component
```typescript
interface ButtonProps {
  // Content
  children: ReactNode;
  icon?: ReactNode;
  iconPosition?: 'left' | 'right';
  
  // Variants
  variant?: 'primary' | 'secondary' | 'tertiary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  
  // State
  disabled?: boolean;
  loading?: boolean;
  
  // Behavior
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
  
  // Link behavior (optional)
  href?: string;
  target?: '_blank' | '_self';
  
  // Styling
  fullWidth?: boolean;
}

// Behavior:
// - Ripple effect on click
// - Loading spinner replaces icon
// - Disabled state grayed out
// - Keyboard: space/enter triggers
```

### Icon Button Component
```typescript
interface IconButtonProps {
  icon: ReactNode;
  label?: string; // tooltip
  
  variant?: 'default' | 'primary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  
  disabled?: boolean;
  loading?: boolean;
  
  onClick?: () => void;
  
  badge?: string | number;
}

// Behavior:
// - Square shape
// - Tooltip on hover (if label)
// - Badge in top-right corner
```

### Button Group Component
```typescript
interface ButtonGroupProps {
  buttons: ButtonGroupItem[];
  onSelect?: (index: number) => void;
  
  variant?: 'outlined' | 'filled';
  size?: 'sm' | 'md' | 'lg';
}

interface ButtonGroupItem {
  label: string;
  icon?: ReactNode;
  disabled?: boolean;
}

// Behavior:
// - Horizontal button group
// - First button pill-left, last pill-right
// - Keyboard: arrow keys navigate
```

### Dropdown/Menu Component
```typescript
interface DropdownProps {
  // Trigger
  label: string;
  icon?: ReactNode;
  variant?: 'default' | 'primary' | 'secondary';
  
  // Menu items
  items: MenuItem[];
  onSelect?: (itemId: string) => void;
  
  // Behavior
  placement?: 'bottom' | 'top' | 'left' | 'right';
  closeOnSelect?: boolean;
  
  // Styling
  maxHeight?: number;
}

interface MenuItem {
  id: string;
  label: string;
  icon?: ReactNode;
  disabled?: boolean;
  divider?: boolean;
  onClick?: () => void;
}

// Behavior:
// - Keyboard: arrow keys navigate, enter select, ESC close
// - Auto-position in viewport
// - Smooth animations
```

### Split Button Component
```typescript
interface SplitButtonProps {
  mainAction: {
    label: string;
    onClick: () => void;
  };
  
  dropdownItems: MenuItem[];
  
  variant?: 'primary' | 'secondary';
  size?: 'sm' | 'md' | 'lg';
}

// Behavior:
// - Left side: primary action
// - Right side: dropdown trigger
// - Disabled state affects both
```

## 8. Form Components

### Form Container Component
```typescript
interface FormProps {
  // Layout
  layout?: 'vertical' | 'horizontal';
  columnCount?: 1 | 2 | 3;
  
  // Submission
  onSubmit: (data: Record<string, any>) => void;
  onCancel?: () => void;
  
  // State
  loading?: boolean;
  error?: string;
  
  // Children
  children: ReactNode;
  
  // Actions
  submitLabel?: string;
  cancelLabel?: string;
  showCancel?: boolean;
}

// Behavior:
// - Tracks all field values
// - Prevents submit if any field invalid
// - Auto-focus first error on submit
// - Keyboard: enter submits (if no textarea focused)
```

### Form Field Component
```typescript
interface FormFieldProps {
  name: string;
  label: string;
  
  // Validation
  required?: boolean;
  validate?: (value: any) => string | null;
  
  // Help text
  hint?: string;
  helpText?: string;
  
  // Layout
  fullWidth?: boolean;
  
  children: ReactNode;
}

// Behavior:
// - Display validation errors
// - Show required indicator
// - Connect with parent form for value tracking
```

## 9. Validation Criteria

Component Library must support:
- [ ] All components have TypeScript prop interfaces
- [ ] Consistent prop naming across components
- [ ] All components keyboard accessible
- [ ] Loading and disabled states
- [ ] Error state handling
- [ ] Screen reader announcements
- [ ] Focus management
- [ ] Smooth animations (300ms default)
- [ ] Mobile touch interactions
- [ ] Theme color support

## 10. Related Specifications

- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Design system
- [spec-ui-advanced-patterns.md](spec-ui-advanced-patterns.md) - Complex patterns
- [spec-design-tokens.md](spec-design-tokens.md) - CSS variables
