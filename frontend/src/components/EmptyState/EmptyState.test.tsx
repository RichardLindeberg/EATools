import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { EmptyState } from './EmptyState'

describe('EmptyState', () => {
  it('renders without crashing', () => {
    render(<EmptyState title="No data" />)
    expect(screen.getByText('No data')).toBeInTheDocument()
  })

  it('displays title text', () => {
    render(<EmptyState title="No results found" />)
    expect(screen.getByText('No results found')).toBeInTheDocument()
  })

  it('displays description when provided', () => {
    render(
      <EmptyState 
        title="No items" 
        description="Try adjusting your filters"
      />
    )
    expect(screen.getByText('Try adjusting your filters')).toBeInTheDocument()
  })

  it('renders icon when provided', () => {
    render(
      <EmptyState 
        title="Empty" 
        icon={<span data-testid="custom-icon">📭</span>}
      />
    )
    expect(screen.getByTestId('custom-icon')).toBeInTheDocument()
  })

  it('renders action buttons when provided', () => {
    render(
      <EmptyState 
        title="No data" 
        action={<button>Create New</button>}
      />
    )
    expect(screen.getByText('Create New')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(
      <EmptyState title="Test" className="custom-empty" />
    )
    expect(container.firstChild).toHaveClass('custom-empty')
  })

  it('renders all elements together', () => {
    render(
      <EmptyState
        title="No projects"
        description="Get started by creating your first project"
        icon={<span data-testid="icon">🚀</span>}
        action={
          <div>
            <button>Create Project</button>
            <button>Import</button>
          </div>
        }
      />
    )
    
    expect(screen.getByText('No projects')).toBeInTheDocument()
    expect(screen.getByText('Get started by creating your first project')).toBeInTheDocument()
    expect(screen.getByTestId('icon')).toBeInTheDocument()
    expect(screen.getByText('Create Project')).toBeInTheDocument()
    expect(screen.getByText('Import')).toBeInTheDocument()
  })

  it('has centered layout', () => {
    const { container } = render(<EmptyState title="Empty" />)
    expect(container.firstChild).toHaveClass('empty-state')
  })
})
