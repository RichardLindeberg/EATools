# EA Tool Specifications

This directory contains all formal specifications for the EA Tool system. Specifications are designed for both human developers and AI agents, following a consistent template structure.

## Getting Started

**Start here:** [spec-index.md](spec-index.md) - Central index linking to all specifications

## Directory Structure

```
spec/
├── README.md                              # This file
├── spec-index.md                          # Central index (START HERE)
│
├── spec-schema-domain-model.md            # [ORIGINAL] Complete domain model (comprehensive)
│
├── spec-schema-domain-overview.md         # Domain model overview
├── spec-schema-entities-business.md       # Business layer entities
├── spec-schema-entities-application.md    # Application layer entities  
├── spec-schema-entities-infrastructure.md # Infrastructure entities
├── spec-schema-entities-data.md           # Data entities
├── spec-schema-entities-meta.md           # Meta entities (Relation, View)
├── spec-schema-entities-supporting.md     # Supporting entities
├── spec-schema-validation.md              # Validation rules and matrices
└── spec-schema-examples.md                # Examples and patterns
```

## File Naming Convention

Specifications follow this naming pattern:
```
spec-{category}-{topic}.md

Where:
  category = schema | tool | architecture | process | data | infrastructure | design
  topic    = descriptive name with hyphens
```

### Categories

- **schema** - Domain models, entities, data structures
- **tool** - API contracts, endpoints, SDK specifications
- **architecture** - System design, components, deployment
- **process** - Workflows, authorization, operations
- **data** - Database schemas, migrations, persistence
- **infrastructure** - Hosting, networking, scaling
- **design** - UI/UX, visualization, rendering

## Specification Template

All specifications follow this structure:

1. **Front Matter** (YAML) - title, version, dates, owner, tags
2. **Introduction** - High-level purpose
3. **Purpose & Scope** - Clear boundaries
4. **Definitions** - Domain terminology
5. **Requirements, Constraints & Guidelines** - Explicit rules
6. **Interfaces & Data Contracts** - Schemas/APIs
7. **Acceptance Criteria** - Testable conditions
8. **Test Automation Strategy** - Testing approach
9. **Rationale & Context** - Design decisions explained
10. **Dependencies & External Integrations** - External requirements
11. **Examples & Edge Cases** - Working samples
12. **Validation Criteria** - Compliance checks
13. **Related Specifications** - Cross-references

## Quick Links by Role

### Backend Developers
1. [Domain Model Overview](spec-schema-domain-overview.md)
2. [Application Layer Entities](spec-schema-entities-application.md)
3. [Validation Rules](spec-schema-validation.md)
4. [Error Handling](spec-tool-error-handling.md)
5. [Query Patterns](spec-tool-query-patterns.md)

### Frontend Developers
1. [API Contract](spec-tool-api-contract.md) *(coming soon)*
2. [Schema Examples](spec-schema-examples.md)
3. [Application Layer Entities](spec-schema-entities-application.md)
4. [Query Patterns](spec-tool-query-patterns.md)

### Integration Partners
1. [API Contract](spec-tool-api-contract.md) *(coming soon)*
2. [Authentication](spec-process-authentication.md)
3. [Error Handling](spec-tool-error-handling.md)
4. [Webhook Events](spec-process-webhooks.md) *(coming soon)*
5. [Query Patterns](spec-tool-query-patterns.md)

### Architects
1. [Domain Model Overview](spec-schema-domain-overview.md)
2. [System Architecture](spec-architecture-system-design.md) *(coming soon)*
3. [Data Architecture](spec-architecture-data.md)
4. [Authentication](spec-process-authentication.md)
5. [Authorization Model](spec-process-authorization.md)
6. [Error Handling](spec-tool-error-handling.md)
7. [Query Patterns](spec-tool-query-patterns.md)

## Status Legend

- ✅ **Complete** - Fully documented and implemented
- 🚧 **In Progress** - Being drafted or reviewed
- 📋 **Planned** - Identified but not started

## Current Status

| Specification | Status | Notes |
|--------------|--------|-------|
| spec-index.md | ✅ Complete | Central index |
| spec-schema-domain-model.md | ✅ Complete | Original comprehensive version |
| spec-schema-domain-overview.md | ✅ Complete | Split from domain-model |
| spec-schema-entities-business.md | 📋 Planned | Extract from domain-model |
| spec-schema-entities-application.md | 📋 Planned | Extract from domain-model |
| spec-schema-entities-infrastructure.md | 📋 Planned | Extract from domain-model |
| spec-schema-entities-data.md | 📋 Planned | Extract from domain-model |
| spec-schema-entities-meta.md | 📋 Planned | Extract from domain-model |
| spec-schema-entities-supporting.md | 📋 Planned | Extract from domain-model |
| spec-schema-validation.md | 📋 Planned | Extract from domain-model |
| spec-schema-examples.md | 📋 Planned | Extract from domain-model |
| spec-process-authorization.md | ✅ Complete | Authorization model, OPA/Rego |
| spec-process-authentication.md | ✅ Complete | OIDC, JWT, API keys |
| spec-architecture-data.md | ✅ Complete | Data architecture, schemas, migrations |
| spec-tool-error-handling.md | ✅ Complete | Error codes, responses, trace IDs |
| spec-tool-query-patterns.md | ✅ Complete | Pagination, filtering, sorting, search |

## Contributing

When creating or updating specifications:

1. Follow the standard template structure
2. Use appropriate prefixes (REQ-, SEC-, CON-, GUD-, PAT-, AC-, etc.)
3. Include cross-references to related specs
4. Add examples for complex concepts
5. Update the index when adding new specs
6. Keep specifications focused and modular

## Related Documentation

- [../docs/](../docs/) - User-facing documentation
- [../openapi.yaml](../openapi.yaml) - Machine-readable API contract
- [../.github/prompts/](../.github/prompts/) - AI agent prompts

---

**Maintained by**: EA Platform Team  
**Last Updated**: 2026-01-08
