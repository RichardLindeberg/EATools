import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Card } from './Card'

describe('Card', () => {
  it('renders without crashing', () => {
    const { container } = render(<Card>Card content</Card>)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('renders children content', () => {
    render(<Card>Test content</Card>)
    expect(screen.getByText('Test content')).toBeInTheDocument()
  })

  it('renders header when provided', () => {
    render(
      <Card header={<h2>Card Header</h2>}>
        Content
      </Card>
    )
    expect(screen.getByText('Card Header')).toBeInTheDocument()
  })

  it('renders footer when provided', () => {
    render(
      <Card footer={<button>Action</button>}>
        Content
      </Card>
    )
    expect(screen.getByText('Action')).toBeInTheDocument()
  })

  it('renders header, body, and footer together', () => {
    render(
      <Card
        header={<h2>Header</h2>}
        footer={<button>Footer</button>}
      >
        Body Content
      </Card>
    )
    expect(screen.getByText('Header')).toBeInTheDocument()
    expect(screen.getByText('Body Content')).toBeInTheDocument()
    expect(screen.getByText('Footer')).toBeInTheDocument()
  })

  it('applies elevation class when hoverable', () => {
    const { container } = render(<Card hoverable>Content</Card>)
    expect(container.firstChild).toHaveClass('card--hoverable')
  })

  it('applies custom className', () => {
    const { container } = render(<Card className="custom-card">Content</Card>)
    expect(container.firstChild).toHaveClass('custom-card')
  })

  it('renders complex content structure', () => {
    render(
      <Card
        header={
          <div>
            <h3>Title</h3>
            <span>Subtitle</span>
          </div>
        }
        footer={
          <div>
            <button>Cancel</button>
            <button>Save</button>
          </div>
        }
      >
        <p>Paragraph 1</p>
        <p>Paragraph 2</p>
      </Card>
    )
    expect(screen.getByText('Title')).toBeInTheDocument()
    expect(screen.getByText('Subtitle')).toBeInTheDocument()
    expect(screen.getByText('Paragraph 1')).toBeInTheDocument()
    expect(screen.getByText('Paragraph 2')).toBeInTheDocument()
    expect(screen.getByText('Cancel')).toBeInTheDocument()
    expect(screen.getByText('Save')).toBeInTheDocument()
  })
})
