import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { Table } from './Table'

describe('Table', () => {
  const mockColumns = [
    { key: 'id', header: 'ID' },
    { key: 'name', header: 'Name', sortable: true },
    { key: 'status', header: 'Status' },
  ]

  const mockData = [
    { id: 1, name: 'Item 1', status: 'Active' },
    { id: 2, name: 'Item 2', status: 'Inactive' },
    { id: 3, name: 'Item 3', status: 'Active' },
  ]

  it('renders without crashing', () => {
    render(<Table columns={mockColumns} data={mockData} />)
    expect(screen.getByRole('table')).toBeInTheDocument()
  })

  it('renders column headers', () => {
    render(<Table columns={mockColumns} data={mockData} />)
    expect(screen.getByText('ID')).toBeInTheDocument()
    expect(screen.getByText(/Name/)).toBeInTheDocument()
    expect(screen.getByText('Status')).toBeInTheDocument()
  })

  it('renders data rows', () => {
    render(<Table columns={mockColumns} data={mockData} />)
    expect(screen.getByText('Item 1')).toBeInTheDocument()
    expect(screen.getByText('Item 2')).toBeInTheDocument()
    expect(screen.getByText('Item 3')).toBeInTheDocument()
  })

  it('displays loading state', () => {
    render(<Table columns={mockColumns} data={[]} loading />)
    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('displays empty message when no data', () => {
    render(<Table columns={mockColumns} data={[]} emptyMessage="No items found" />)
    expect(screen.getByText('No items found')).toBeInTheDocument()
  })

  it('renders custom empty message', () => {
    const customMessage = 'Custom empty state'
    render(<Table columns={mockColumns} data={[]} emptyMessage={customMessage} />)
    expect(screen.getByText(customMessage)).toBeInTheDocument()
  })

  it('applies striped class when striped prop is true', () => {
    const { container } = render(<Table columns={mockColumns} data={mockData} striped />)
    expect(container.querySelector('.table--striped')).toBeInTheDocument()
  })

  it('applies hoverable class when hoverable prop is true', () => {
    const { container } = render(<Table columns={mockColumns} data={mockData} hoverable />)
    expect(container.querySelector('.table--hoverable')).toBeInTheDocument()
  })

  it('handles row click', () => {
    const handleRowClick = vi.fn()
    render(<Table columns={mockColumns} data={mockData} onRowClick={handleRowClick} />)
    
    const firstRow = screen.getByText('Item 1').closest('tr')
    firstRow?.click()
    
    expect(handleRowClick).toHaveBeenCalledWith(mockData[0], 0)
  })

  it('renders custom accessor content', () => {
    const columnsWithAccessor = [
      { key: 'id', header: 'ID' },
      { 
        key: 'name', 
        header: 'Name',
        accessor: (row: any) => <strong>{row.name}</strong>
      },
    ]
    
    render(<Table columns={columnsWithAccessor} data={mockData} />)
    expect(screen.getByText('Item 1').tagName).toBe('STRONG')
  })

  it('shows sort icons for sortable columns', () => {
    const { container } = render(
      <Table 
        columns={mockColumns} 
        data={mockData}
        sortColumn="name"
        sortDirection="asc"
      />
    )
    
    const nameHeader = screen.getByText('Name', { exact: false })
    expect(nameHeader.textContent).toContain('↑')
  })
})
