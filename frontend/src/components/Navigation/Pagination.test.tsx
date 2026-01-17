import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Pagination } from './Pagination'

describe('Pagination', () => {
  it('renders without crashing', () => {
    render(<Pagination currentPage={1} totalPages={5} onPageChange={() => {}} />)
    expect(screen.getByRole('navigation')).toBeInTheDocument()
  })

  it('displays current page number', () => {
    render(<Pagination currentPage={3} totalPages={5} onPageChange={() => {}} />)
    const currentPageButton = screen.getByText('3')
    expect(currentPageButton).toHaveClass('pagination__button--active')
  })

  it('calls onPageChange when page button is clicked', () => {
    const handlePageChange = vi.fn()
    render(<Pagination currentPage={1} totalPages={5} onPageChange={handlePageChange} />)
    
    const page2Button = screen.getByText('2')
    fireEvent.click(page2Button)
    expect(handlePageChange).toHaveBeenCalledWith(2)
  })

  it('disables previous button on first page', () => {
    render(<Pagination currentPage={1} totalPages={5} onPageChange={() => {}} />)
    const prevButton = screen.getByLabelText('Go to previous page')
    expect(prevButton).toBeDisabled()
  })

  it('disables next button on last page', () => {
    render(<Pagination currentPage={5} totalPages={5} onPageChange={() => {}} />)
    const nextButton = screen.getByLabelText('Go to next page')
    expect(nextButton).toBeDisabled()
  })

  it('enables previous button when not on first page', () => {
    render(<Pagination currentPage={2} totalPages={5} onPageChange={() => {}} />)
    const prevButton = screen.getByLabelText('Go to previous page')
    expect(prevButton).not.toBeDisabled()
  })

  it('enables next button when not on last page', () => {
    render(<Pagination currentPage={2} totalPages={5} onPageChange={() => {}} />)
    const nextButton = screen.getByLabelText('Go to next page')
    expect(nextButton).not.toBeDisabled()
  })

  it('navigates to previous page', () => {
    const handlePageChange = vi.fn()
    render(<Pagination currentPage={3} totalPages={5} onPageChange={handlePageChange} />)
    
    const prevButton = screen.getByLabelText('Go to previous page')
    fireEvent.click(prevButton)
    expect(handlePageChange).toHaveBeenCalledWith(2)
  })

  it('navigates to next page', () => {
    const handlePageChange = vi.fn()
    render(<Pagination currentPage={3} totalPages={5} onPageChange={handlePageChange} />)
    
    const nextButton = screen.getByLabelText('Go to next page')
    fireEvent.click(nextButton)
    expect(handlePageChange).toHaveBeenCalledWith(4)
  })

  it('shows ellipsis for large page counts', () => {
    render(<Pagination currentPage={5} totalPages={20} onPageChange={() => {}} />)
    const ellipsis = screen.getAllByText('…')
    expect(ellipsis.length).toBeGreaterThan(0)
  })

  it('applies custom className', () => {
    const { container } = render(
      <Pagination currentPage={1} totalPages={5} onPageChange={() => {}} className="custom-pagination" />
    )
    expect(container.firstChild).toHaveClass('custom-pagination')
  })

  it('shows all pages when totalPages is small', () => {
    render(<Pagination currentPage={2} totalPages={3} onPageChange={() => {}} />)
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('handles single page correctly', () => {
    render(<Pagination currentPage={1} totalPages={1} onPageChange={() => {}} />)
    const prevButton = screen.getByLabelText('Go to previous page')
    const nextButton = screen.getByLabelText('Go to next page')
    expect(prevButton).toBeDisabled()
    expect(nextButton).toBeDisabled()
  })

  it('has proper ARIA labels', () => {
    render(<Pagination currentPage={2} totalPages={5} onPageChange={() => {}} />)
    expect(screen.getByRole('navigation')).toHaveAttribute('aria-label', 'Pagination')
  })
})
