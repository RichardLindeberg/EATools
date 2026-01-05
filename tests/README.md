"""README for EA Tool Integration Tests."""

# Integration Tests for EA Tool API

This directory contains integration tests that validate the EA Tool API implementation against the OpenAPI specification.

## Structure

```
tests/
├── conftest.py              # Shared fixtures and utilities
├── pytest.ini               # Pytest configuration
├── requirements.txt         # Python dependencies
├── integration/             # Integration test modules
│   ├── test_organizations.py
│   ├── test_applications.py
│   ├── test_servers.py
│   └── ...
└── fixtures/               # Test data and fixtures
    └── sample_data.py
```

## Setup

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Ensure the API is running:**
   ```bash
   # From the backend directory
   dotnet run
   # Server should be listening on http://localhost:8000
   ```

## Running Tests

### Run all tests:
```bash
pytest
```

### Run with coverage:
```bash
pytest --cov
```

### Run specific test file:
```bash
pytest tests/integration/test_organizations.py
```

### Run specific test:
```bash
pytest tests/integration/test_organizations.py::TestOrganizations::test_list_organizations
```

### Run only integration tests:
```bash
pytest -m integration
```

### Skip slow tests:
```bash
pytest -m "not slow"
```

## Environment Variables

Configure the test environment via environment variables:

- `EA_API_URL` — API base URL (default: `http://localhost:8000`)
- `EA_API_KEY` — API key for authentication (default: `test-key-12345`)
- `EA_OIDC_TOKEN` — OIDC bearer token (optional, for OIDC auth tests)

Example:
```bash
EA_API_URL=https://staging.api.example.com EA_API_KEY=secret-key pytest
```

## Test Patterns

### Using the client fixture (API key auth):
```python
def test_example(client):
    response = client.get("/organizations")
    assert response.status_code == 200
```

### Using OIDC auth:
```python
def test_example_oidc(client_oidc):
    response = client_oidc.get("/organizations")
    assert response.status_code == 200
```

### Validating error responses:
```python
def test_missing_field(client):
    response = client.post("/organizations", json={})
    assert response.status_code in [400, 422]
    assert "error" in response.json()
```

## Coverage

HTML coverage reports are generated in `htmlcov/index.html` after running tests.

## CI/CD Integration

Tests are designed to run in CI pipelines:
```bash
pytest --cov --cov-report=xml --junitxml=test-results.xml
```

This generates:
- JUnit XML for CI systems (GitHub Actions, GitLab, etc.)
- Coverage reports for coverage tracking services

## Notes

- All tests assume the API is running and accessible
- Tests clean up after themselves (delete created resources)
- Timestamps are validated as ISO 8601 with UTC timezone (Z suffix)
- Authorization tests require appropriate OIDC tokens or API keys
