import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Header } from './Header'

describe('Header', () => {
  it('renders without crashing', () => {
    const { container } = render(<Header />)
    expect(container.querySelector('.header')).toBeInTheDocument()
  })

  it('renders logo when provided', () => {
    render(
      <Header logo={<img src="/logo.png" alt="Company Logo" />} />
    )
    expect(screen.getByAltText('Company Logo')).toBeInTheDocument()
  })

  it('renders children content', () => {
    render(
      <Header>
        <div>Search Bar</div>
      </Header>
    )
    expect(screen.getByText('Search Bar')).toBeInTheDocument()
  })

  it('renders actions when provided', () => {
    render(
      <Header actions={<button>User Menu</button>} />
    )
    expect(screen.getByText('User Menu')).toBeInTheDocument()
  })

  it('renders all sections together', () => {
    render(
      <Header
        logo={<span>Logo</span>}
        actions={<button>Actions</button>}
      >
        <div>Content</div>
      </Header>
    )
    expect(screen.getByText('Logo')).toBeInTheDocument()
    expect(screen.getByText('Content')).toBeInTheDocument()
    expect(screen.getByText('Actions')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    const { container } = render(<Header className="custom-header" />)
    expect(container.firstChild).toHaveClass('header', 'custom-header')
  })

  it('does not render logo section when logo is not provided', () => {
    const { container } = render(<Header />)
    expect(container.querySelector('.header__logo')).not.toBeInTheDocument()
  })

  it('does not render content section when children are not provided', () => {
    const { container } = render(<Header />)
    expect(container.querySelector('.header__content')).not.toBeInTheDocument()
  })

  it('does not render actions section when actions are not provided', () => {
    const { container } = render(<Header />)
    expect(container.querySelector('.header__actions')).not.toBeInTheDocument()
  })

  it('renders as header element', () => {
    const { container } = render(<Header />)
    expect(container.querySelector('header')).toBeInTheDocument()
  })
})
