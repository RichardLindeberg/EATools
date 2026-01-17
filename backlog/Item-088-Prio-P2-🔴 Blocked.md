# Item-088: Implement Advanced Search & Filtering

**Status:** � Blocked  
**Priority:** P2 - MEDIUM  
**Effort:** 32-48 hours  
**Created:** 2026-01-17  
**Owner:** Full-Stack Team

---

## Problem Statement

The current search functionality only supports basic text search on entity lists. Users need advanced search capabilities including cross-entity search, complex filter combinations, saved searches, and search result highlighting to efficiently find information across large datasets.

Advanced search improves discoverability and productivity when working with hundreds or thousands of entities as specified in [spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md).

This is a Phase 2 advanced feature for improved user experience.

---

## Affected Files

**Backend:**
- `src/EATool/Search/SearchTypes.fs` - Search domain types
- `src/EATool/Search/SearchService.fs` - Search business logic
- `src/EATool/Search/SearchIndex.fs` - In-memory search index (or integrate external search engine)
- `src/EATool/Api/SearchEndpoints.fs` - Search API endpoints
- `src/EATool/Database/SearchQueries.fs` - Database search queries

**Frontend:**
- `frontend/src/components/search/GlobalSearch.tsx` - Global search bar
- `frontend/src/components/search/AdvancedSearchModal.tsx` - Advanced search interface
- `frontend/src/components/search/SearchResults.tsx` - Search results display
- `frontend/src/components/search/SavedSearches.tsx` - Saved search manager
- `frontend/src/components/search/SearchFilters.tsx` - Filter builder
- `frontend/src/hooks/useGlobalSearch.ts` - Global search hook
- `frontend/src/hooks/useSavedSearches.ts` - Saved searches hook
- `frontend/src/api/searchApi.ts` - Search API client
- `frontend/src/pages/SearchResultsPage.tsx` - Dedicated search results page

---

## Specifications

- [spec/spec-ui-entity-workflows.md](../spec/spec-ui-entity-workflows.md) - Cross-cutting concerns including global search
- [spec/spec-ui-routing-navigation.md](../spec/spec-ui-routing-navigation.md) - Search routing
- [FRONTEND-READINESS-REPORT.md](../FRONTEND-READINESS-REPORT.md) - Phase 2 feature

---

## Detailed Tasks

### Phase 1: Backend Search Infrastructure (16-20 hours)

**Search Endpoints:**
- [ ] GET /api/search?q={query}&type={entity_type}&filters={json} - Global search
- [ ] GET /api/search/suggestions?q={query} - Search suggestions (autocomplete)
- [ ] POST /api/search/advanced - Advanced search with complex filters
- [ ] GET /api/search/saved - Get user's saved searches
- [ ] POST /api/search/saved - Save search
- [ ] DELETE /api/search/saved/{id} - Delete saved search

**Search Service:**
- [ ] Implement full-text search across all entity types
- [ ] Support searching by: Name, Description, Tags, Owner, Metadata
- [ ] Implement AND/OR/NOT logic for filters
- [ ] Support nested filters (e.g., (Type=API OR Type=Database) AND Status=Active)
- [ ] Implement fuzzy matching (typo tolerance)
- [ ] Implement stemming and synonyms (optional)
- [ ] Support date range filters (created_at, updated_at)
- [ ] Support numeric range filters (e.g., CPU > 8)
- [ ] Implement relevance scoring (rank results by relevance)
- [ ] Support pagination (skip/take)

**Search Index:**
- [ ] Create in-memory search index (or integrate Elasticsearch/Meilisearch)
- [ ] Index all 9 entity types on application start
- [ ] Update index when entities created/updated/deleted
- [ ] Support reindexing on demand (POST /api/search/reindex)
- [ ] Optimize index for performance (<100ms search)

**Saved Searches:**
- [ ] Store saved searches in database (user_id, name, query, filters)
- [ ] Support creating, reading, updating, deleting saved searches
- [ ] Allow sharing saved searches with team (optional)

**Search Suggestions:**
- [ ] Implement autocomplete suggestions based on query
- [ ] Return top 10 matching entities across types
- [ ] Include entity type, name, and preview in suggestions
- [ ] Cache suggestions for performance

### Phase 2: Frontend Global Search (12-16 hours)

**Global Search Bar:**
- [ ] Add search bar to application header (visible on all pages)
- [ ] Implement keyboard shortcut (Ctrl+K or Cmd+K)
- [ ] Show search suggestions dropdown as user types (debounce 300ms)
- [ ] Display suggestions with entity type icon, name, and preview
- [ ] Highlight matching text in suggestions
- [ ] Support keyboard navigation (up/down arrows, enter to select)
- [ ] Navigate to entity detail on selection
- [ ] Add "See all results" link for comprehensive search

**Search Results Page:**
- [ ] Create dedicated search results page (/search?q=query)
- [ ] Display results grouped by entity type
- [ ] Show result count per entity type
- [ ] Support filtering results by entity type (tabs or checkboxes)
- [ ] Highlight matching text in result snippets
- [ ] Display entity metadata (type, owner, last updated)
- [ ] Support pagination (load more or infinite scroll)
- [ ] Add "Advanced Search" button to open advanced search modal

**Advanced Search Modal:**
- [ ] Create modal for building complex search queries
- [ ] Add entity type selector (search specific types or all)
- [ ] Add field-specific filters (Name, Description, Status, Owner, etc.)
- [ ] Support multiple filters with AND/OR logic
- [ ] Add date range pickers (Created, Updated)
- [ ] Add numeric range inputs (e.g., CPU cores)
- [ ] Show preview of search query
- [ ] Execute search and show results in modal or navigate to results page
- [ ] Add "Save Search" button

**Saved Searches:**
- [ ] Display saved searches in dropdown (from global search bar)
- [ ] Allow quick execution of saved search
- [ ] Create SavedSearchesModal for managing saved searches
- [ ] Support renaming, editing, deleting saved searches
- [ ] Add "Pin" feature to show favorite searches prominently

---

## Acceptance Criteria

**Backend:**
- [ ] GET /api/search?q={query} searches across all entity types
- [ ] Search includes Name, Description, Tags, Owner, Metadata
- [ ] Search supports AND/OR/NOT logic
- [ ] Search supports filters (status, type, owner, date ranges)
- [ ] Results ranked by relevance
- [ ] Search completes in <100ms for 1000+ entities
- [ ] Suggestions endpoint returns autocomplete results
- [ ] Saved searches stored and retrieved correctly
- [ ] Search respects user permissions (no unauthorized entities in results)
- [ ] Search index updates on entity changes

**Frontend:**
- [ ] Global search bar visible in header on all pages
- [ ] Keyboard shortcut (Ctrl+K / Cmd+K) opens search
- [ ] Suggestions appear as user types (300ms debounce)
- [ ] Matching text highlighted in suggestions
- [ ] Selecting suggestion navigates to entity
- [ ] "See all results" opens dedicated search results page
- [ ] Search results page displays results grouped by type
- [ ] Result snippets show matching text with highlighting
- [ ] Advanced search modal allows building complex queries
- [ ] Filters work correctly (entity type, fields, date ranges)
- [ ] Saved searches displayed and executable
- [ ] Saved searches can be managed (rename, edit, delete)

**UX:**
- [ ] Search fast and responsive (<300ms perceived)
- [ ] Suggestions relevant and accurate
- [ ] No flickering during typing (debouncing works)
- [ ] Clear visual feedback during search
- [ ] Empty states handled (no results found)
- [ ] Error states handled (search service unavailable)

**Performance:**
- [ ] Search 1000+ entities in <100ms backend
- [ ] Suggestions displayed in <300ms
- [ ] Search index does not impact app startup time
- [ ] No memory leaks from search index

---

## Dependencies

**Blocked by:**  
- Item-075 (Frontend project setup)
- Item-076 (Component library - needs Modal, SearchInput)
- Items 079-081 (Entity pages to search)

**Blocks:** None (enhances existing features)

---

## Notes

- **Phase 2 Feature:** Can be implemented after MVP Phase 1
- Consider integrating Elasticsearch or Meilisearch for production-grade search
- For MVP, implement SQL-based full-text search (SQLite FTS5)
- Test with large datasets (1000+ entities)
- Add search analytics (track popular searches)
- Consider adding search filters persistence in URL (deep linking)
- Add search history (recent searches)
- Consider adding "Did you mean?" suggestions for typos
- Test search performance with complex queries
- Add search result export (CSV, JSON)
- Consider adding search within specific entity (e.g., search within Applications only)
- Test keyboard navigation thoroughly
- Consider adding search result preview (quick view modal)
