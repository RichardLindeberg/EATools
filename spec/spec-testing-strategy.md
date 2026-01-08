---
title: Testing Strategy Specification
version: 1.0
date_created: 2026-01-08
last_updated: 2026-01-08
owner: EA Platform Team
tags: [testing, quality, automation, ci]
---

# Introduction

This specification defines the automated testing strategy for the EA Tool platform, covering unit, integration, and performance validation across F# services and Python-based API tests.

## 1. Purpose & Scope

- Establish consistent testing requirements for F# unit tests and Python pytest integration suites.
- Define coverage expectations, data management, mocking patterns, and CI/CD execution gates.
- Applies to all backend code in `src/` and API-facing scenarios validated through `tests/`.
- Excludes manual exploratory testing and frontend/UI testing (covered separately when introduced).

## 2. Definitions

- **Unit Test**: Code-level test targeting a single F# module or function, executed via xUnit.
- **Integration Test**: Cross-component/API test executed via pytest against a running EA Tool API instance.
- **Fixture**: Reusable setup/teardown provider (e.g., pytest fixtures in `conftest.py`, F# helper modules in `tests/fixtures`).
- **Test Double**: Stub, fake, or mock used to isolate external dependencies (e.g., `MockOTelExporter`).
- **Coverage**: Statement/branch coverage measured via Coverlet (`dotnet test /p:CollectCoverage`) and pytest `--cov`.
- **Observability Signals**: Traces, metrics, and logs emitted via OpenTelemetry during request processing.

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: Every change to domain/application logic MUST include or update F# unit tests using the Arrange-Act-Assert pattern.
- **REQ-002**: All REST endpoints MUST be covered by pytest integration tests that validate success, failure, and contract/validation behavior.
- **REQ-003**: Tests MUST assert observability signals (trace context, metrics labels, structured logs) for critical paths (create/update/delete commands and health endpoints).
- **REQ-004**: Test data generation MUST be deterministic, self-cleaning, and free of PII; fixtures must delete or rollback created resources.
- **REQ-005**: Coverage targets: F# unit tests ≥ 85% statement coverage for `src/`; Python integration tests ≥ 70% line coverage for exercised modules.
- **REQ-006 (Deferred)**: Performance thresholds and latency SLOs SHALL be defined in backlog Item-066; current scope MUST capture latency metrics but SHALL NOT enforce numeric thresholds.
- **REQ-007**: CI pipeline MUST fail on test failures, coverage regression below thresholds, or missing JUnit/coverage artifacts.
- **SEC-001**: Secrets (API keys, OIDC tokens) MUST come from environment variables and MUST NOT be hard-coded or logged in tests.
- **CON-001**: F# tests SHALL use xUnit with `[<Fact>]` or `[<Theory>]`; Python tests SHALL use pytest with markers (`integration`, `slow`) for runtime selection.
- **CON-002**: External calls in unit tests are prohibited; use in-memory doubles (e.g., mock exporters, fake stores) to isolate side effects.
- **GUD-001**: Prefer builder helpers/fixtures over inline literals for complex payloads; centralize in `tests/fixtures` for reuse.
- **PAT-001**: Naming pattern for tests: `ModuleNameTests` (F#) and `Test*` classes/functions (pytest). Use descriptive test names with backtick strings in F# for clarity.
- **PAT-002**: Integration tests MUST tag destructive cases with `@pytest.mark.integration` and clean up created entities within the test body or fixture finalizers.

## 4. Interfaces & Data Contracts

### Test Execution Interfaces

| Layer | Command | Purpose |
| --- | --- | --- |
| Unit (F#) | `dotnet test tests/EATool.Tests.fsproj /p:CollectCoverage=true /p:CoverletOutputFormat="cobertura"` | Run unit suite with coverage via Coverlet |
| Integration (Python) | `pytest tests/integration -m integration --cov=tests --cov-report=xml` | Run API integration suite with coverage and markers |
| Full CI | `dotnet test ...` then `pytest ...` | Execute both layers, publish coverage XML and JUnit XML artifacts |

### Environment & Fixtures

- Environment variables: `EA_API_URL` (default `http://localhost:8000`), `EA_API_KEY` (default `test-key-12345`), `EA_OIDC_TOKEN` (optional for OIDC paths).
- Pytest fixtures: `client` (API key), `client_oidc` (OIDC), `api_base_url`, `api_headers`, `api_headers_oidc`, `http_client`, `api_is_healthy`.
- F# fixtures: helper modules in `tests/fixtures` (e.g., `MockOTelExporter`, `OTelTestHelpers`) for observability assertions and in-memory metrics/log capture.
- Data contracts under test follow OpenAPI definitions; tests MUST validate required fields, types, UUID formats, and ISO 8601 timestamps with `Z` suffix.

## 5. Acceptance Criteria

- **AC-001**: Given a new domain handler, when `dotnet test` runs, then at least one `[<Fact>]` asserts success and failure paths for that handler.
- **AC-002**: Given a new REST endpoint, when `pytest -m integration` runs, then successful and validation/error responses are asserted including status codes and schema shape.
- **AC-003**: Given observability is enabled, when tests exercise create/update flows, then traceparent format, low-cardinality metric labels, and structured logs are validated.
- **AC-004**: Given CI execution, when coverage reports are generated, then thresholds in REQ-005 are met and artifacts (coverage XML, JUnit XML) are produced.
- **AC-005**: Given destructive integration tests, when they complete, then created entities are removed or isolated to avoid state bleed between tests.

## 6. Test Automation Strategy

- **Test Levels**: F# unit tests for domain/infrastructure logic; pytest integration tests for API contracts; lightweight performance smoke hooks embedded in integration suite (capture timing without failing on thresholds until Item-066 defines SLOs).
- **Frameworks**: xUnit with Coverlet for F#; pytest with `requests`, `pytest-cov` for Python; optional `pytest-xdist` for parallelization when stable.
- **Execution Order**: Unit tests run first; integration tests require a running API (`dotnet run` or container) and reuse fixtures from `conftest.py`.
- **Data Management**: Use `fixtures/sample_data.py` and builder helpers; clean up via API `DELETE` calls or fixture finalizers; avoid shared mutable state.
- **Mocking Strategy**: Use in-memory doubles (`MockOTelExporter`, fake stores) for unit tests; for integration, mock only external third-party calls, not the EA API itself.
- **CI/CD Integration**: Pipeline steps publish `coverage.cobertura.xml`, `coverage.xml`, and `test-results.xml`; failures block merge. Cache Python/NUGet dependencies to reduce runtimes.
- **Performance Checks**: Capture timing metrics in integration tests; do not fail on latency until Item-066 defines SLO thresholds, but record values for observability.

## 7. Rationale & Context

- Mixed-language stack (F# backend, Python integration) requires aligned patterns to maintain contract fidelity and observability guarantees.
- Observability is a first-class requirement; tests enforce W3C trace context and low-cardinality metrics to avoid telemetry bloat.
- Deterministic fixtures and cleanup prevent environment coupling and flaky runs across CI and local environments.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: EA Tool API instance reachable at `EA_API_URL` for integration tests.

### Third-Party Services
- **SVC-001**: OpenTelemetry collector endpoint (optional) when validating real telemetry pipelines; otherwise use mock exporters.

### Infrastructure Dependencies
- **INF-001**: .NET SDK 10.0 for F# test projects; Python 3.11+ for pytest execution.

### Data Dependencies
- **DAT-001**: Seeded test data via `fixtures/sample_data.py`; autogenerated UUIDv4 identifiers per request.

### Technology Platform Dependencies
- **PLT-001**: xUnit, Coverlet, and pytest with requests/pytest-cov; consistent versions pinned in `tests/requirements.txt` and `tests/EATool.Tests.fsproj`.

### Compliance Dependencies
- **COM-001**: Telemetry data used in tests must exclude PII and conform to internal data classification (`internal` by default for test data).

## 9. Examples & Edge Cases

```fsharp
[<Fact>]
let ``Event store returns error on duplicate id`` () =
    let store = InMemoryEventStore()
    let evt = sampleEvent "org-123"
    store.Append(evt) |> ignore
    let ex = Assert.Throws<Exception>(fun () -> store.Append(evt) |> ignore)
    Assert.Contains("duplicate", ex.Message)
```

```python
@pytest.mark.integration
def test_create_application_validation_error(client):
    payload = {"name": "", "owner": "", "lifecycle": "invalid", "dataClassification": "internal"}
    response = client.post("/applications", json=payload)
    assert response.status_code in [400, 422]
    body = response.json()
    assert "error" in body or "details" in body
```

Edge cases include empty payloads, invalid UUIDs, missing authentication headers, high-cardinality metric labels, and traceparent headers with malformed segments.

## 10. Validation Criteria

- CI run shows passing `dotnet test` and `pytest -m integration` with published coverage/JUnit artifacts.
- Coverage reports meet or exceed REQ-005 thresholds.
- Observability assertions for traceparent format, metric label cardinality, and structured logs pass.
- Integration tests leave no residual data (verified by clean environment or teardown logs).

## 11. Related Specifications / Further Reading

- [spec-process-observability.md](spec-process-observability.md)
- [spec-tool-error-handling.md](spec-tool-error-handling.md)
- [spec-tool-query-patterns.md](spec-tool-query-patterns.md)
- [spec-architecture-event-sourcing.md](spec-architecture-event-sourcing.md)