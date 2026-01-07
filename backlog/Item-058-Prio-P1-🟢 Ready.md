# Item-058: Field Format Validation - Email, DNS, Hostname

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 6-8 hours  
**Created:** 2026-01-07  
**Owner:** TBD

---

## Problem Statement

The specification requires format validation for several fields, but the implementation doesn't enforce them. This allows invalid data:

- Organization.contacts can contain invalid email addresses
- Organization.domains can contain invalid DNS domains
- Server.hostname can contain invalid hostnames
- Integration.protocol field should be required

This violates CON-BUS-002, CON-BUS-003, CON-INF-003 constraints and allows invalid data to be stored.

---

## Affected Files

**Create:**
- `src/Infrastructure/Validation/EmailValidator.fs` - RFC 5322 email validation
- `src/Infrastructure/Validation/DnsValidator.fs` - DNS domain name validation
- `src/Infrastructure/Validation/HostnameValidator.fs` - Hostname format validation

**Modify:**
- `src/Api/Requests/CreateOrganizationRequest.fs` - Add contact email validation
- `src/Api/Handlers/OrganizationHandlers.fs` - Validate emails and domains before create/update
- `src/Infrastructure/Repository/OrganizationRepository.fs` - Validate at persistence layer
- `src/Api/Requests/CreateServerRequest.fs` - Make protocol required, add hostname validation
- `src/Api/Handlers/ServerHandlers.fs` - Validate hostname before create/update
- `src/Infrastructure/Repository/ServerRepository.fs` - Validate at persistence layer
- `src/Api/Requests/CreateIntegrationRequest.fs` - Make protocol required
- `src/Api/Handlers/IntegrationHandlers.fs` - Validate protocol required
- `tests/OrganizationCommandTests.fs` - Test email/domain validation
- `tests/ServerCommandTests.fs` - Test hostname validation
- `tests/IntegrationCommandTests.fs` - Test protocol required

---

## Specifications

- [spec/spec-schema-entities-business.md](../spec/spec-schema-entities-business.md) - CON-BUS-002: DNS domain validation, CON-BUS-003: Email validation
- [spec/spec-schema-entities-infrastructure.md](../spec/spec-schema-entities-infrastructure.md) - CON-INF-003: Hostname validation
- [spec/spec-schema-validation.md](../spec/spec-schema-validation.md) - Field format requirements
- [spec/spec-implementation-status.md](../spec/spec-implementation-status.md) - Gaps #10, #11, #12, #13

---

## Detailed Tasks

- [ ] **Create EmailValidator.fs**:
  - Implement RFC 5322 email validation (or use proven library)
  - Function: `validateEmail : string -> Result<string, string>`
  - Reject: empty strings, spaces, multiple @ symbols, invalid domains
  - Accept: standard email formats (user@domain.com, firstname.lastname@domain.co.uk)
  - Add tests for edge cases (max length, special chars, IDN domains)

- [ ] **Create DnsValidator.fs**:
  - Implement DNS domain name validation per RFC 1123
  - Function: `validateDomain : string -> Result<string, string>`
  - Pattern: alphanumeric + hyphens, dots as separators, min 2 labels (example.com)
  - Reject: leading/trailing hyphens, invalid chars, too long labels (>63 chars)
  - Reject: invalid TLDs (single letter, numeric-only)
  - Add tests: valid domains (example.com, sub.example.co.uk), invalid domains

- [ ] **Create HostnameValidator.fs**:
  - Implement hostname validation per RFC 952/1123
  - Function: `validateHostname : string -> Result<string, string>`
  - Pattern: alphanumeric + hyphens, dots as separators, underscores allowed
  - Reject: leading/trailing hyphens, leading dots, spaces
  - Reject: hostnames starting with numbers (optional, depends on spec)
  - Add tests: valid hostnames (server.example.com, web-01.prod), invalid hostnames

- [ ] **Organization.contacts email validation**:
  - Update CreateOrganizationRequest.contacts to validate each email
  - Add to OrganizationHandlers.createOrganization: validate all emails before create
  - Add to OrganizationHandlers.updateOrganization: validate all emails before update
  - Add to OrganizationRepository.create/update: validate at persistence layer
  - Error: "Invalid email address: '{email}' - must be valid RFC 5322 email"
  - Allow empty array (optional field), but validate non-empty entries

- [ ] **Organization.domains DNS validation**:
  - Update CreateOrganizationRequest.domains to validate each domain
  - Add to OrganizationHandlers.createOrganization: validate all domains before create
  - Add to OrganizationHandlers.updateOrganization: validate all domains before update
  - Add to OrganizationRepository.create/update: validate at persistence layer
  - Error: "Invalid domain: '{domain}' - must be valid DNS domain name"
  - Allow empty array (optional field), but validate non-empty entries

- [ ] **Server.hostname validation**:
  - Update CreateServerRequest.hostname (make required if optional)
  - Add to ServerHandlers.createServer: validate hostname before create
  - Add to ServerHandlers.updateServer: validate hostname before update
  - Add to ServerRepository.create/update: validate at persistence layer
  - Error: "Invalid hostname: '{hostname}' - must be valid server hostname"

- [ ] **Integration.protocol required field**:
  - Update CreateIntegrationRequest.protocol (remove optionality)
  - Add to IntegrationHandlers.createIntegration: validate protocol provided
  - Add to IntegrationRepository.create: enforce not null
  - List valid protocols if bounded (HTTP, SOAP, gRPC, Kafka, etc.)
  - Error: "Protocol is required and must be specified"

- [ ] **Validation error responses**:
  - Return 400 Bad Request for format validation failures
  - Include field name and validation rule in error message
  - Example: `{"error": "Invalid email in contacts[2]: 'user@invalid' - must be valid email address"}`

- [ ] **Test coverage**:
  - Email validation: valid formats, invalid formats, edge cases
  - Domain validation: valid TLDs, invalid chars, length limits
  - Hostname validation: valid names, invalid names, case sensitivity
  - Protocol required: missing protocol returns error, valid protocols accepted
  - Integration tests: full create/update flow with validation
  - Error message tests: verify clarity and spec alignment

---

## Acceptance Criteria

- [ ] Organization.contacts validates each email as RFC 5322 compliant
- [ ] Organization.domains validates each domain as valid DNS name
- [ ] Server.hostname validates as valid hostname format
- [ ] Integration.protocol is required and validated
- [ ] Invalid emails return 400 Bad Request with clear error
- [ ] Invalid domains return 400 Bad Request with clear error
- [ ] Invalid hostnames return 400 Bad Request with clear error
- [ ] Missing protocol returns 400 Bad Request
- [ ] Repository layer validates in addition to handler layer
- [ ] All tests pass (20+ test cases across validators)
- [ ] Validators are reusable (can be used in other handlers)
- [ ] Build succeeds with 0 errors, 0 warnings
- [ ] Performance acceptable (validation <10ms per field)

---

## Dependencies

**Blocks:**
- None

**Depends On:**
- Item-056 - Required fields (complementary validation)

**Related:**
- Item-057 - Unique constraints (complementary validation)
- Item-021 - Implementation status tracker (gap analysis)

---

## Notes

Format validation is critical for data quality. Use proven validation libraries where possible (e.g., System.Net.Mail.MailAddress for email, regex for DNS/hostname). Keep validators focused and reusable so they can be used throughout the codebase.
