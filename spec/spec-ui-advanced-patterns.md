---
title: Advanced UI Patterns & Complex Workflows Specification
version: 1.0
date_created: 2026-01-17
owner: EA Platform Team
tags: [patterns, forms, modals, notifications, loading, advanced]
---

# Advanced UI Patterns & Complex Workflows Specification

## 1. Purpose & Scope

This specification defines complex UI patterns for advanced features like dynamic forms, multi-step workflows, loading states, error recovery, and advanced interactions.

## 2. Dynamic Forms

### Conditional Field Visibility
```typescript
interface FormField {
  name: string;
  label: string;
  type: string;
  
  // Conditional visibility
  visibleIf?: (formValues: Record<string, any>) => boolean;
  enabledIf?: (formValues: Record<string, any>) => boolean;
  requiredIf?: (formValues: Record<string, any>) => boolean;
}

// Example: Show PII retention policy only if HasPII is checked
const fields: FormField[] = [
  {
    name: 'hasPii',
    label: 'Contains PII Data',
    type: 'checkbox'
  },
  {
    name: 'piiRetention',
    label: 'PII Retention Period',
    type: 'select',
    visibleIf: (values) => values.hasPii === true,
    requiredIf: (values) => values.hasPii === true
  },
  {
    name: 'piiEncryption',
    label: 'Encryption Required',
    type: 'checkbox',
    visibleIf: (values) => values.hasPii === true
  }
];

// Behavior:
// - Fields hide/show with smooth transition
// - Validation adjusts based on conditions
// - Errors clear when field becomes hidden
// - Form value preserved when field re-appears
```

### Dynamic Arrays/Repeated Sections
```typescript
// Multiple data mappings in integration form
interface DataMapping {
  sourceField: string;
  targetField: string;
  transformation: string;
}

// Form allows adding/removing mappings
const form = {
  name: 'Integration',
  fields: [
    {
      name: 'name',
      type: 'text'
    },
    {
      name: 'mappings',
      type: 'array',
      itemTemplate: {
        sourceField: { type: 'select', options: sourceFields },
        targetField: { type: 'select', options: targetFields },
        transformation: { type: 'text', placeholder: 'Optional transform' }
      },
      minItems: 1,
      maxItems: 100,
      actions: ['add', 'remove', 'duplicate']
    }
  ]
};

// Behavior:
// - Add button below array to add new item
// - Remove icon on each item (except if minItems=1)
// - Duplicate icon to copy item
// - Validation per item
// - Reorder via drag-and-drop
// - Confirmation on delete if values entered
```

### Multi-Step Forms
```typescript
interface FormStep {
  id: string;
  title: string;
  description?: string;
  fields: FormField[];
  validate?: (values: any) => ValidationError[];
}

// Example: Create Integration in 3 steps
const steps: FormStep[] = [
  {
    id: 'basic',
    title: 'Basic Information',
    fields: [
      { name: 'name', type: 'text', required: true },
      { name: 'description', type: 'textarea' }
    ]
  },
  {
    id: 'systems',
    title: 'Source & Target',
    fields: [
      { name: 'sourceSystem', type: 'select', required: true },
      { name: 'targetSystem', type: 'select', required: true }
    ]
  },
  {
    id: 'mapping',
    title: 'Data Mapping',
    fields: [
      { name: 'mappings', type: 'array', required: true }
    ]
  }
];

// Behavior:
// - Step indicator at top (1/3, 2/3, 3/3)
// - Only current step visible
// - Validate before moving to next step
// - "Previous" button to go back
// - "Next" button (disabled if validation fails)
// - "Save" button on final step
// - Progress preserved if user navigates away and returns
// - Confirmation on cancel if data entered
```

### Form Auto-Save
```typescript
// Save form state periodically
const useAutoSave = (formValues, autoSaveInterval = 5000) => {
  const [saveStatus, setSaveStatus] = useState<'saved' | 'saving' | 'unsaved'>('saved');
  const debounceTimer = useRef<NodeJS.Timeout>();
  
  useEffect(() => {
    setSaveStatus('unsaved');
    
    debounceTimer.current = setTimeout(async () => {
      try {
        setSaveStatus('saving');
        await saveFormDraft(formValues);
        setSaveStatus('saved');
      } catch (error) {
        setSaveStatus('unsaved');
        showError('Failed to auto-save form');
      }
    }, autoSaveInterval);
    
    return () => clearTimeout(debounceTimer.current);
  }, [formValues]);
  
  return saveStatus;
};

// UI indicator
// Saved ✓
// Saving...
// Unsaved ⚠️ (with retry button)
```

## 3. Loading States

### Skeleton Screens
```typescript
// Show placeholder structure while loading
const ApplicationListSkeleton = () => (
  <div className="list">
    {[...Array(5)].map((_, i) => (
      <div key={i} className="list-item">
        <Skeleton width="30%" height="20px" /> {/* Name */}
        <Skeleton width="25%" height="20px" /> {/* Owner */}
        <Skeleton width="15%" height="20px" /> {/* Status */}
      </div>
    ))}
  </div>
);

// Behavior:
// - Shows immediately while loading
// - Same layout as real content
// - Animated pulse effect
// - Prevents content shift on load
```

### Progressive Loading
```typescript
// Load content in priority order
const ApplicationDetail = ({ appId }: Props) => {
  const [overview, setOverview] = useState(null);
  const [integrations, setIntegrations] = useState(null);
  const [servers, setServers] = useState(null);
  
  useEffect(() => {
    // Load overview first (critical)
    fetchOverview(appId).then(setOverview);
    
    // Then load related data (lower priority)
    setTimeout(() => fetchIntegrations(appId).then(setIntegrations), 100);
    setTimeout(() => fetchServers(appId).then(setServers), 200);
  }, [appId]);
  
  return (
    <DetailView>
      {overview ? <OverviewTab {...overview} /> : <SkeletonScreen />}
      
      <Tabs>
        <Tab title="Integrations">
          {integrations ? <IntegrationsList {...integrations} /> : <LoadingSpinner />}
        </Tab>
        
        <Tab title="Servers">
          {servers ? <ServersList {...servers} /> : <LoadingSpinner />}
        </Tab>
      </Tabs>
    </DetailView>
  );
};
```

### Lazy Loading (Infinite Scroll)
```typescript
interface LazyListProps<T> {
  loadMore: (page: number) => Promise<T[]>;
  renderItem: (item: T) => ReactNode;
  hasMore: boolean;
  pageSize?: number;
}

const LazyList = <T,>({ loadMore, renderItem, hasMore, pageSize = 20 }: LazyListProps<T>) => {
  const [items, setItems] = useState<T[]>([]);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(false);
  const observerTarget = useRef(null);
  
  useEffect(() => {
    const observer = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting && hasMore && !loading) {
        setLoading(true);
        loadMore(page + 1)
          .then(newItems => {
            setItems(prev => [...prev, ...newItems]);
            setPage(prev => prev + 1);
          })
          .finally(() => setLoading(false));
      }
    });
    
    if (observerTarget.current) {
      observer.observe(observerTarget.current);
    }
    
    return () => observer.disconnect();
  }, [page, hasMore, loading]);
  
  return (
    <>
      {items.map((item, idx) => <div key={idx}>{renderItem(item)}</div>)}
      {loading && <LoadingSpinner />}
      {hasMore && <div ref={observerTarget} />}
    </>
  );
};
```

## 4. Error Recovery

### Error Boundaries
```typescript
class FormErrorBoundary extends React.Component {
  state = { hasError: false, error: null };
  
  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }
  
  render() {
    if (this.state.hasError) {
      return (
        <Alert variant="error">
          <p>An error occurred while processing your form.</p>
          <p>{this.state.error?.message}</p>
          <Button onClick={() => this.setState({ hasError: false })}>
            Try Again
          </Button>
        </Alert>
      );
    }
    
    return this.props.children;
  }
}
```

### Retry Logic
```typescript
interface RetryOptions {
  maxRetries?: number;
  backoffMs?: number;
  backoffMultiplier?: number;
}

const withRetry = async (
  fn: () => Promise<any>,
  options: RetryOptions = {}
) => {
  const { maxRetries = 3, backoffMs = 1000, backoffMultiplier = 2 } = options;
  
  let lastError;
  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;
      if (attempt < maxRetries) {
        const delay = backoffMs * Math.pow(backoffMultiplier, attempt);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }
  
  throw lastError;
};

// Usage
const handleSave = async () => {
  try {
    await withRetry(() => saveEntity(formData), { maxRetries: 3 });
    showSuccess('Saved successfully');
  } catch (error) {
    showError(`Failed after 3 attempts: ${error.message}`);
    // Show manual retry button
  }
};
```

### Optimistic Updates with Rollback
```typescript
const useOptimisticUpdate = () => {
  const [items, setItems] = useState([]);
  
  const updateItem = async (id: string, updates: any) => {
    // Optimistic update
    const previousItems = items;
    const optimisticItems = items.map(item =>
      item.id === id ? { ...item, ...updates } : item
    );
    setItems(optimisticItems);
    
    try {
      // Send to server
      await api.patch(`/items/${id}`, updates);
      // Confirm with server response
      const response = await api.get(`/items/${id}`);
      setItems(items =>
        items.map(item =>
          item.id === id ? response.data : item
        )
      );
    } catch (error) {
      // Rollback on error
      setItems(previousItems);
      showError('Update failed. Reverted to previous state.');
    }
  };
  
  return { items, updateItem };
};
```

## 5. Conflict Resolution

### Last-Write-Wins with User Notification
```typescript
// When saving fails due to conflicts
interface ConflictResolutionModal {
  serverVersion: any;    // Current server state
  clientVersion: any;    // User's version
  options: {
    label: 'Keep My Changes' | 'Use Latest' | 'Manual Merge';
    onClick: () => void;
  }[];
}

// Show modal:
// ┌─ Conflict Detected ─────────────────┐
// │ This item was modified by another   │
// │ user while you were editing.        │
// │                                     │
// │ Server Version: [Show Diff]         │
// │ Your Version:   [Show Diff]         │
// │                                     │
// │ [Keep My Changes] [Use Latest]      │
// └─────────────────────────────────────┘
```

### Merge Conflict UI
```typescript
// For complex data structures
const ConflictMergeEditor = ({ original, client, server }: Props) => {
  const [merged, setMerged] = useState(original);
  
  return (
    <div className="merge-editor">
      <div className="pane">
        <h3>Original</h3>
        <CodeBlock value={JSON.stringify(original, null, 2)} readOnly />
      </div>
      
      <div className="pane">
        <h3>Your Changes</h3>
        <CodeBlock value={JSON.stringify(client, null, 2)} readOnly />
      </div>
      
      <div className="pane">
        <h3>Server Changes</h3>
        <CodeBlock value={JSON.stringify(server, null, 2)} readOnly />
      </div>
      
      <div className="pane">
        <h3>Merged Result</h3>
        <CodeBlock value={JSON.stringify(merged, null, 2)} onChange={setMerged} />
      </div>
      
      <div className="actions">
        <Button onClick={() => handleMerge(merged)}>Merge</Button>
        <Button variant="secondary" onClick={() => cancel()}>Cancel</Button>
      </div>
    </div>
  );
};
```

## 6. Bulk Operations with Progress

```typescript
interface BulkOperation {
  action: 'delete' | 'update' | 'export' | 'archive';
  ids: string[];
  data?: Record<string, any>;
}

const BulkProgressModal = ({ operation, onComplete }: Props) => {
  const [progress, setProgress] = useState<BulkProgress>({
    total: operation.ids.length,
    completed: 0,
    failed: 0,
    errors: []
  });
  
  useEffect(() => {
    const processItems = async () => {
      for (let i = 0; i < operation.ids.length; i++) {
        try {
          await executeBulkItem(operation.action, operation.ids[i], operation.data);
          setProgress(prev => ({
            ...prev,
            completed: prev.completed + 1
          }));
        } catch (error) {
          setProgress(prev => ({
            ...prev,
            failed: prev.failed + 1,
            errors: [...prev.errors, { id: operation.ids[i], error: error.message }]
          }));
        }
      }
      onComplete(progress);
    };
    
    processItems();
  }, []);
  
  const percentComplete = (progress.completed + progress.failed) / progress.total * 100;
  
  return (
    <Modal>
      <h3>Processing {operation.ids.length} items...</h3>
      <ProgressBar value={percentComplete} />
      <p>{progress.completed} completed, {progress.failed} failed</p>
      
      {progress.errors.length > 0 && (
        <Alert variant="warning">
          <p>{progress.errors.length} items failed:</p>
          <ul>
            {progress.errors.map(e => <li key={e.id}>{e.id}: {e.error}</li>)}
          </ul>
        </Alert>
      )}
      
      <Button disabled={progress.completed + progress.failed < progress.total}>
        Done
      </Button>
    </Modal>
  );
};
```

## 7. Complex Modal Workflows

```typescript
// Multi-action modal with dependencies
interface ComplexConfirmModal {
  title: string;
  message: string;
  impact: {
    label: string;
    count: number;
    warning: boolean;
  }[];
  actions: {
    label: string;
    onClick: () => Promise<void>;
    variant: 'primary' | 'danger';
    disabled?: boolean;
  }[];
}

// Example: Delete application with cascading deletes
// ┌─ Delete Application ─────────────────────┐
// │ Are you sure? This will also:            │
// │ • Delete 5 integrations                  │
// │ • Unassign from 3 servers                │
// │ • Archive 12 services                    │
// │                                          │
// │ ⚠️ This action cannot be undone.         │
// │                                          │
// │ [Cancel] [Delete Everything]             │
// └──────────────────────────────────────────┘
```

## 8. Wizards & Guided Flows

```typescript
interface WizardStep {
  id: string;
  title: string;
  description?: string;
  component: FC;
  validate?: () => Promise<boolean>;
  onNext?: () => Promise<void>;
  canSkip?: boolean;
  skipLabel?: string;
}

const Wizard = ({ steps, onComplete }: Props) => {
  const [currentStepIdx, setCurrentStepIdx] = useState(0);
  const [stepData, setStepData] = useState({});
  
  const currentStep = steps[currentStepIdx];
  const isLastStep = currentStepIdx === steps.length - 1;
  
  const handleNext = async () => {
    if (currentStep.validate) {
      const valid = await currentStep.validate();
      if (!valid) return;
    }
    
    if (currentStep.onNext) {
      await currentStep.onNext();
    }
    
    if (isLastStep) {
      onComplete(stepData);
    } else {
      setCurrentStepIdx(prev => prev + 1);
    }
  };
  
  return (
    <Modal>
      <StepIndicator currentStep={currentStepIdx + 1} totalSteps={steps.length} />
      <h2>{currentStep.title}</h2>
      <p>{currentStep.description}</p>
      
      <currentStep.component data={stepData[currentStep.id]} onDataChange={...} />
      
      <div className="actions">
        <Button onClick={() => setCurrentStepIdx(prev => prev - 1)} disabled={currentStepIdx === 0}>
          Previous
        </Button>
        {currentStep.canSkip && (
          <Button variant="secondary" onClick={() => setCurrentStepIdx(prev => prev + 1)}>
            {currentStep.skipLabel || 'Skip'}
          </Button>
        )}
        <Button onClick={handleNext}>
          {isLastStep ? 'Finish' : 'Next'}
        </Button>
      </div>
    </Modal>
  );
};
```

## 9. Validation Criteria

Advanced patterns must support:
- [ ] Conditional form fields
- [ ] Dynamic arrays/repeated sections
- [ ] Multi-step form workflows
- [ ] Auto-save with status indicator
- [ ] Skeleton screens for loading
- [ ] Progressive content loading
- [ ] Infinite scroll/lazy loading
- [ ] Error boundaries
- [ ] Automatic retry with backoff
- [ ] Optimistic updates with rollback
- [ ] Conflict resolution UI
- [ ] Bulk operation progress tracking
- [ ] Complex modal workflows
- [ ] Guided wizards
- [ ] Form state persistence

## 10. Related Specifications

- [spec-design-component-library.md](spec-design-component-library.md) - Component APIs
- [spec-ui-api-integration.md](spec-ui-api-integration.md) - Error handling
- [spec-design-ui-architecture.md](spec-design-ui-architecture.md) - Layout patterns
