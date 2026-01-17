import React from 'react'
import './Pagination.css'

export interface PaginationProps {
  /**
   * Current page (1-indexed)
   */
  currentPage: number

  /**
   * Total number of pages
   */
  totalPages: number

  /**
   * Page change handler
   */
  onPageChange: (page: number) => void

  /**
   * Number of page buttons to show around current page
   */
  siblingCount?: number

  /**
   * Show first/last page buttons
   */
  showFirstLast?: boolean

  /**
   * Additional CSS class
   */
  className?: string
}

/**
 * Pagination Component
 *
 * Accessible pagination with page numbers and navigation.
 * Follows WCAG 2.1 AA standards.
 *
 * @example
 * <Pagination 
 *   currentPage={page}
 *   totalPages={totalPages}
 *   onPageChange={setPage}
 * />
 */
export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  siblingCount = 1,
  showFirstLast = true,
  className,
}) => {
  const paginationClasses = ['pagination', className].filter(Boolean).join(' ')

  // Generate page numbers to display
  const getPageNumbers = (): (number | string)[] => {
    const pages: (number | string)[] = []

    if (totalPages <= 7) {
      // Show all pages if 7 or fewer
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      // Always show first page
      pages.push(1)

      const leftSibling = Math.max(currentPage - siblingCount, 2)
      const rightSibling = Math.min(currentPage + siblingCount, totalPages - 1)

      // Add left ellipsis
      if (leftSibling > 2) {
        pages.push('...')
      }

      // Add page numbers around current page
      for (let i = leftSibling; i <= rightSibling; i++) {
        pages.push(i)
      }

      // Add right ellipsis
      if (rightSibling < totalPages - 1) {
        pages.push('...')
      }

      // Always show last page
      pages.push(totalPages)
    }

    return pages
  }

  const pageNumbers = getPageNumbers()

  const handlePrevious = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1)
    }
  }

  const handleNext = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1)
    }
  }

  const handleFirst = () => {
    onPageChange(1)
  }

  const handleLast = () => {
    onPageChange(totalPages)
  }

  return (
    <nav className={paginationClasses} aria-label="Pagination">
      <ul className="pagination__list">
        {showFirstLast && (
          <li>
            <button
              type="button"
              className="pagination__button"
              onClick={handleFirst}
              disabled={currentPage === 1}
              aria-label="Go to first page"
            >
              ««
            </button>
          </li>
        )}

        <li>
          <button
            type="button"
            className="pagination__button"
            onClick={handlePrevious}
            disabled={currentPage === 1}
            aria-label="Go to previous page"
          >
            ‹
          </button>
        </li>

        {pageNumbers.map((page, index) => {
          if (page === '...') {
            return (
              <li key={`ellipsis-${index}`}>
                <span className="pagination__ellipsis" aria-hidden="true">
                  …
                </span>
              </li>
            )
          }

          const pageNum = page as number
          const isActive = pageNum === currentPage

          return (
            <li key={pageNum}>
              <button
                type="button"
                className={`pagination__button ${
                  isActive ? 'pagination__button--active' : ''
                }`}
                onClick={() => onPageChange(pageNum)}
                aria-label={`Go to page ${pageNum}`}
                aria-current={isActive ? 'page' : undefined}
              >
                {pageNum}
              </button>
            </li>
          )
        })}

        <li>
          <button
            type="button"
            className="pagination__button"
            onClick={handleNext}
            disabled={currentPage === totalPages}
            aria-label="Go to next page"
          >
            ›
          </button>
        </li>

        {showFirstLast && (
          <li>
            <button
              type="button"
              className="pagination__button"
              onClick={handleLast}
              disabled={currentPage === totalPages}
              aria-label="Go to last page"
            >
              »»
            </button>
          </li>
        )}
      </ul>
    </nav>
  )
}

export default Pagination
