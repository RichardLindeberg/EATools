import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import { Skeleton } from './Skeleton'

describe('Skeleton', () => {
  it('renders without crashing', () => {
    const { container } = render(<Skeleton />)
    expect(container.firstChild).toBeInTheDocument()
  })

  it('applies variant classes correctly', () => {
    const { container, rerender } = render(<Skeleton variant="text" />)
    expect(container.firstChild).toHaveClass('skeleton--text')

    rerender(<Skeleton variant="card" />)
    expect(container.firstChild).toHaveClass('skeleton--card')

    rerender(<Skeleton variant="table" />)
    expect(container.firstChild).toHaveClass('skeleton--table')

    rerender(<Skeleton variant="circle" />)
    expect(container.firstChild).toHaveClass('skeleton--circle')
  })

  it('applies default variant when not specified', () => {
    const { container } = render(<Skeleton />)
    expect(container.firstChild).toHaveClass('skeleton--text')
  })

  it('applies custom width', () => {
    const { container } = render(<Skeleton width="200px" />)
    const skeleton = container.firstChild as HTMLElement
    expect(skeleton.style.width).toBe('200px')
  })

  it('applies custom height', () => {
    const { container } = render(<Skeleton height="100px" />)
    const skeleton = container.firstChild as HTMLElement
    expect(skeleton.style.height).toBe('100px')
  })

  it('applies both width and height', () => {
    const { container } = render(<Skeleton width="300px" height="150px" />)
    const skeleton = container.firstChild as HTMLElement
    expect(skeleton.style.width).toBe('300px')
    expect(skeleton.style.height).toBe('150px')
  })

  it('applies custom className', () => {
    const { container } = render(<Skeleton className="custom-skeleton" />)
    expect(container.firstChild).toHaveClass('custom-skeleton')
  })

  it('renders single skeleton by default', () => {
    const { container } = render(<Skeleton />)
    const skeletons = container.querySelectorAll('.skeleton')
    expect(skeletons).toHaveLength(1)
  })

  it('combines variant, dimensions, and className', () => {
    const { container } = render(
      <Skeleton variant="card" width="250px" height="180px" className="custom" />
    )
    const skeleton = container.firstChild as HTMLElement
    expect(skeleton).toHaveClass('skeleton--card')
    expect(skeleton).toHaveClass('custom')
    expect(skeleton.style.width).toBe('250px')
    expect(skeleton.style.height).toBe('180px')
  })
})
