import { describe, it, expect } from 'vitest'
import { formatDate, truncate, toTitleCase, isEmpty } from '@/utils/helpers'

describe('Utility Functions', () => {
  describe('formatDate', () => {
    it('should format ISO date string to readable format', () => {
      const date = '2026-01-17T12:30:45Z'
      const result = formatDate(date)
      expect(result).toContain('2026')
      expect(result).toContain('January')
    })

    it('should return original string on invalid date', () => {
      const invalid = 'not-a-date'
      const result = formatDate(invalid)
      expect(result).toBe(invalid)
    })
  })

  describe('truncate', () => {
    it('should truncate string longer than max length', () => {
      const result = truncate('This is a long string', 10)
      expect(result).toBe('This is...')
      expect(result.length).toBeLessThanOrEqual(10)
    })

    it('should not truncate string shorter than max length', () => {
      const result = truncate('Short', 10)
      expect(result).toBe('Short')
    })
  })

  describe('toTitleCase', () => {
    it('should convert string to title case', () => {
      expect(toTitleCase('hello world')).toBe('Hello World')
      expect(toTitleCase('UPPERCASE')).toBe('Uppercase')
    })
  })

  describe('isEmpty', () => {
    it('should detect empty values', () => {
      expect(isEmpty(null)).toBe(true)
      expect(isEmpty(undefined)).toBe(true)
      expect(isEmpty('')).toBe(true)
      expect(isEmpty([])).toBe(true)
      expect(isEmpty({})).toBe(true)
    })

    it('should detect non-empty values', () => {
      expect(isEmpty('hello')).toBe(false)
      expect(isEmpty([1])).toBe(false)
      expect(isEmpty({ key: 'value' })).toBe(false)
    })
  })
})
