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

**CQRS Architecture Alignment:**
This specification is designed to work within the CQRS (Command Query Responsibility Segregation) pattern where:
- **Queries** are read-only operations using GET endpoints
- **Commands** are write operations using POST command endpoints or DELETE with approval context
- The patterns defined here must respect CQRS boundaries to ensure predictable, auditable system behavior

## 1.1 CQRS Constraints on Advanced Patterns

### Auto-Save Pattern Constraints
```typescript
// CONSTRAINT: Auto-save must NOT silently dispatch multiple commands

// ❌ WRONG: Auto-save attempts to save all field changes immediately
const useAutoSaveWrong = (formValues) => {
  const { mutate: saveField } = useMutation((field, value) => 
    dispatchCommand(`/entity/${id}/commands/set-${field}`, value)
  );
  
  useEffect(() => {
    // Dangerous: Could dispatch set-classification + set-owner + set-lifecycle
    // simultaneously without user awareness
    Object.entries(formValues).forEach(([field, value]) => {
      saveField(field, value);
    });
  }, [formValues]);
};

// ✅ CORRECT: Auto-save only for single, idempotent operations or drafts
const useAutoSaveCorrect = (formValues) => {
  const { mutate: saveDraft } = useMutation((data) => 
    saveFormDraft(data) // Draft is local/idempotent, not a command
  );
  
  useEffect(() => {
    // Safe: Draft is ephemeral, not a command dispatch
    debounce(() => saveDraft(formValues), 5000);
  }, [formValues]);
};

// Behavior:
// - Auto-save permitted only for:
//   a) Draft saves (local state, no server commands)
//   b) Single idempotent commands (e.g., update description via update-description command)
// - Auto-save NOT permitted for:
//   a) Multiple simultaneous commands (e.g., set-classification + set-owner)
//   b) Non-idempotent commands without explicit user confirmation
// - Multi-field edits require explicit Submit button
```

### Retry Logic Constraints (Queries vs Commands)
```typescript
// CONSTRAINT: Queries auto-retry on failure; Commands only retry if idempotent

// ✅ CORRECT: Queries auto-retry
const useEntityDetail = (entityId: string) => {
  return useQuery({
    queryKey: ['entity', entityId],
    queryFn: () => getEntity(entityId),
    retry: 3, // Auto-retry up to 3 times
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000)
  });
};

// ✅ CORRECT: Commands only retry with idempotency key
const useEntityCommand = (entityId: string) => {
  return useMutation({
    mutationFn: (command) => dispatchCommand(entityId, command),
    retry: (failureCount, error) => {
      // Only retry if idempotent (has idempotency key) and retriable error
      return (
        command.idempotencyKey &&
        (error.status === 408 || error.status === 429 || error.status === 500)
      );
    }
  });
};

// Behavior:
// - Queries: Automatic exponential backoff retry (up to 30s)
// - Commands: Retry only if:
//   a) Command is marked idempotent (e.g., idempotency key provided)
//   b) Error is retriable (timeout, rate limit, server error)
// - Non-idempotent commands: No retry; show error immediately
```

### Optimistic Updates Constraints
```typescript
// CONSTRAINT: Optimistic updates only for safe, reversible changes; deletes await confirmation

// ✅ CORRECT: Optimistic update for description (reversible)
const useUpdateDescription = (entityId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (newDescription) => 
      dispatchCommand(entityId, 'update-description', { description: newDescription }),
    onMutate: async (newDescription) => {
      // Optimistically update local cache (safe because reversible)
      await queryClient.cancelQueries(['entity', entityId]);
      const previousEntity = queryClient.getQueryData(['entity', entityId]);
      queryClient.setQueryData(['entity', entityId], (old) => ({
        ...old,
        description: newDescription
      }));
      return previousEntity;
    },
    onError: (error, variables, previousEntity) => {
      // Rollback on error
      queryClient.setQueryData(['entity', entityId], previousEntity);
      showError('Failed to update description');
    }
  });
};

// ❌ WRONG: Optimistic delete (irreversible)
const useDeleteWrong = (entityId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: () => deleteEntity(entityId),
    onMutate: () => {
      // WRONG: Optimistically removing from cache could hide data if delete fails
      queryClient.removeQueries(['entity', entityId]);
    }
  });
};

// ✅ CORRECT: Delete waits for server confirmation
const useDeleteCorrect = (entityId: string) => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (approvalData) => 
      deleteEntity(entityId, approvalData.approvalId, approvalData.reason),
    onSuccess: () => {
      // Only remove from cache after DELETE succeeds (204 response)
      queryClient.removeQueries(['entity', entityId]);
      showSuccess('Entity deleted');
    },
    onError: (error) => {
      if (error.status === 409) {
        showError('Cannot delete: entity has dependencies');
      } else {
        showError('Delete failed');
      }
    }
  });
};

// Behavior:
// - Optimistic updates: Permitted only for safe, field-level changes (description, tags, etc.)
// - Optimistic deletes: NOT permitted; delete operations await server confirmation
// - All optimistic updates must have rollback on error
```

### Bulk Operations Constraints
```typescript
// CONSTRAINT: Bulk operations prefer backend bulk APIs; manual sequential commands use backoff

// ✅ CORRECT: Use backend bulk endpoint if available
const useBulkDeleteWithApproval = (entityIds: string[]) => {
  return useMutation({
    mutationFn: (approvalData) =>
      // Assumes backend has /bulk/delete endpoint
      bulkDeleteEntities(entityIds, approvalData.approvalId, approvalData.reason),
    onSuccess: () => {
      showSuccess(`Deleted ${entityIds.length} entities`);
      // Invalidate list queries
      queryClient.invalidateQueries(['entities']);
    }
  });
};

// ✅ CORRECT: Manual sequential commands with backoff
const useBulkCommandSequential = (entityIds: string[], commandName: string) => {
  const [progress, setProgress] = useState({ current: 0, total: entityIds.length });
  const [errors, setErrors] = useState<Record<string, string>>({});
  
  return useMutation({
    mutationFn: async (commandData) => {
      const results: any[] = [];
      const errs: Record<string, string> = {};
      
      for (let i = 0; i < entityIds.length; i++) {
        const entityId = entityIds[i];
        try {
          setProgress({ current: i + 1, total: entityIds.length });
          const result = await dispatchCommand(entityId, commandName, commandData);
          results.push(result);
          
          // Exponential backoff between commands to avoid overwhelming server
          if (i < entityIds.length - 1) {
            await sleep(Math.min(100 * (2 ** i), 5000));
          }
        } catch (error) {
          errs[entityId] = error.message;
        }
      }
      
      if (Object.keys(errs).length > 0) {
        setErrors(errs);
        throw new Error(`${Object.keys(errs).length} operations failed`);
      }
      return results;
    }
  });
};

// ❌ WRONG: Dispatching all commands simultaneously
const useBulkCommandWrong = (entityIds: string[]) => {
  return useMutation({
    mutationFn: (commandData) =>
      // Dangerous: All commands fire at once, could overwhelm server
      Promise.all(
        entityIds.map(id => dispatchCommand(id, 'set-status', commandData))
      )
  });
};

// Behavior:
// - Preferred: Use backend bulk/batch endpoints if available
// - Manual sequential: Dispatch commands with exponential backoff (100ms → 5s max)
// - Never fire all commands simultaneously without rate limiting
```

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
