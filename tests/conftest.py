"""Shared test fixtures and utilities for EA Tool integration tests."""

import os
import pytest
import requests
from typing import Generator


# Configuration from environment or defaults
BASE_URL = os.getenv("EA_API_URL", "http://localhost:8000")
API_KEY = os.getenv("EA_API_KEY", "test-key-12345")
OIDC_TOKEN = os.getenv("EA_OIDC_TOKEN", "")


@pytest.fixture
def api_base_url() -> str:
    """Return the API base URL."""
    return BASE_URL


@pytest.fixture
def api_headers() -> dict:
    """Return default headers for API requests (with API key auth)."""
    return {
        "Content-Type": "application/json",
        "X-Api-Key": API_KEY,
    }


@pytest.fixture
def api_headers_oidc() -> dict:
    """Return headers with OIDC bearer token auth."""
    if not OIDC_TOKEN:
        pytest.skip("OIDC_TOKEN environment variable not set")
    return {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {OIDC_TOKEN}",
    }


@pytest.fixture
def http_client() -> requests.Session:
    """Return a requests session for HTTP calls."""
    session = requests.Session()
    session.timeout = 10
    return session


class APIClient:
    """Helper client for making API requests with common setup."""

    def __init__(self, base_url: str, headers: dict):
        self.base_url = base_url.rstrip("/")
        self.headers = headers
        self.session = requests.Session()

    def get(self, endpoint: str, params: dict = None) -> requests.Response:
        """GET request."""
        url = f"{self.base_url}{endpoint}"
        return self.session.get(url, headers=self.headers, params=params)

    def post(self, endpoint: str, json: dict = None) -> requests.Response:
        """POST request."""
        url = f"{self.base_url}{endpoint}"
        return self.session.post(url, headers=self.headers, json=json)

    def patch(self, endpoint: str, json: dict = None) -> requests.Response:
        """PATCH request."""
        url = f"{self.base_url}{endpoint}"
        return self.session.patch(url, headers=self.headers, json=json)

    def delete(self, endpoint: str) -> requests.Response:
        """DELETE request."""
        url = f"{self.base_url}{endpoint}"
        return self.session.delete(url, headers=self.headers)


@pytest.fixture
def client(api_base_url, api_headers) -> APIClient:
    """Return an APIClient instance."""
    return APIClient(api_base_url, api_headers)


@pytest.fixture
def client_oidc(api_base_url, api_headers_oidc) -> APIClient:
    """Return an APIClient instance with OIDC auth."""
    return APIClient(api_base_url, api_headers_oidc)


@pytest.fixture(scope="session")
def api_is_healthy() -> bool:
    """Check if the API is running and healthy."""
    try:
        response = requests.get(
            f"{BASE_URL}/health",
            timeout=5,
        )
        return response.status_code == 200
    except Exception:
        return False
