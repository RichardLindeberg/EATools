# API Usage Guide

This guide provides practical instructions for interacting with the EA Tool API.

## Table of Contents

- [Authentication](#authentication)
- [Making Requests](#making-requests)
- [Pagination](#pagination)
- [Filtering and Search](#filtering-and-search)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Authentication

### OIDC (Recommended for Production)

The API supports OpenID Connect (OIDC) for authentication. Configure your client with the following:

1. Register your application with your OIDC provider
2. Obtain client credentials (client_id and client_secret)
3. Exchange credentials for an access token:

```bash
curl -X POST https://your-oidc-provider/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=YOUR_CLIENT_ID" \
  -d "client_secret=YOUR_CLIENT_SECRET" \
  -d "scope=api"
```

4. Use the access token in API requests:

```bash
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  https://api.example.com/applications
```

### API Key (Development)

For development and testing, you can use API keys:

```bash
curl -H "X-API-Key: your-api-key" \
  http://localhost:8000/applications
```

## Making Requests

### Base URL

- **Development:** `http://localhost:8000`
- **Staging:** `https://staging.api.example.com`
- **Production:** `https://api.example.com`

### Request Format

All requests use JSON for request and response bodies.

```bash
curl -X GET https://api.example.com/applications \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: unique-correlation-id"
```

### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | Bearer token or API key |
| `Content-Type` | For POST/PUT | Must be `application/json` |
| `X-Correlation-Id` | Optional | Unique ID for request tracing |
| `X-Actor` | Optional | Identifier of the requesting actor |
| `X-Actor-Type` | Optional | Type of actor: `user`, `service`, `system` |

### HTTP Methods

- **GET** - Retrieve resources
- **POST** - Create new resources or execute commands
- **PUT** - Update existing resources
- **DELETE** - Delete resources

### Response Format

All responses include:

```json
{
  "data": {},
  "metadata": {
    "page": 1,
    "limit": 50,
    "total": 100,
    "hasMore": true
  }
}
```

## Pagination

List endpoints support pagination using `page` and `limit` parameters.

### Parameters

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `page` | integer | 1 | - | Page number (1-indexed) |
| `limit` | integer | 50 | 200 | Items per page |

### Example

```bash
# Get first 25 applications
curl "https://api.example.com/applications?page=1&limit=25" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get second page
curl "https://api.example.com/applications?page=2&limit=25" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Response Metadata

```json
{
  "data": [...],
  "metadata": {
    "page": 2,
    "limit": 25,
    "total": 100,
    "hasMore": true
  }
}
```

- `page`: Current page number
- `limit`: Items returned per page
- `total`: Total number of items across all pages
- `hasMore`: Whether additional pages exist

## Filtering and Search

### Global Search

Use the `search` parameter for full-text search on name/text fields:

```bash
curl "https://api.example.com/applications?search=payment&limit=50" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Entity-Specific Filters

#### Applications

```bash
# Filter by owner
curl "https://api.example.com/applications?owner=john.doe&page=1" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filter by lifecycle
curl "https://api.example.com/applications?lifecycle=active&page=1" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Combine filters
curl "https://api.example.com/applications?search=payment&owner=john.doe&lifecycle=active" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Servers

```bash
# Filter by environment
curl "https://api.example.com/servers?environment=production" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filter by region
curl "https://api.example.com/servers?region=us-east-1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Data Entities

```bash
# Filter by domain
curl "https://api.example.com/data-entities?domain=customer" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filter by classification
curl "https://api.example.com/data-entities?classification=confidential" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Filter by owner
curl "https://api.example.com/data-entities?owner=jane.smith" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Business Capabilities

```bash
# Filter by parent capability
curl "https://api.example.com/business-capabilities?parent_id=bc-001" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Search within parent hierarchy
curl "https://api.example.com/business-capabilities?parent_id=bc-001&search=reporting" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Relations

```bash
# Find all relations from an application
curl "https://api.example.com/relations?source_id=app-001" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Find specific relation types
curl "https://api.example.com/relations?relation_type=depends_on" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Bidirectional search
curl "https://api.example.com/relations?source_id=app-001&target_id=app-002" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Error Handling

### Error Response Format

All errors follow a consistent format:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "name",
        "message": "Name is required"
      }
    ],
    "requestId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Common Error Codes

| HTTP Status | Code | Description |
|-------------|------|-------------|
| 400 | `VALIDATION_ERROR` | Input validation failed |
| 400 | `INVALID_REQUEST` | Malformed request |
| 401 | `UNAUTHORIZED` | Authentication required or invalid |
| 403 | `FORBIDDEN` | Insufficient permissions |
| 404 | `NOT_FOUND` | Resource not found |
| 409 | `CONFLICT` | Resource already exists or version conflict |
| 429 | `RATE_LIMITED` | Rate limit exceeded |
| 500 | `INTERNAL_ERROR` | Server error |
| 503 | `SERVICE_UNAVAILABLE` | Service temporarily unavailable |

### Example: Handling Errors

```bash
response=$(curl -s -w "\n%{http_code}" https://api.example.com/applications/invalid \
  -H "Authorization: Bearer YOUR_TOKEN")

http_code=$(tail -n1 <<< "$response")
body=$(head -n-1 <<< "$response")

if [ "$http_code" -eq 404 ]; then
  echo "Application not found"
elif [ "$http_code" -eq 401 ]; then
  echo "Authentication failed"
else
  echo "Unexpected error: $body"
fi
```

## Rate Limiting

The API implements rate limiting to ensure fair usage.

### Rate Limit Headers

All responses include rate limit information:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1626998400
```

| Header | Description |
|--------|-------------|
| `X-RateLimit-Limit` | Maximum requests per window |
| `X-RateLimit-Remaining` | Requests remaining in current window |
| `X-RateLimit-Reset` | Unix timestamp when limit resets |

### Rate Limit Tiers

- **Unauthenticated:** 100 requests/hour
- **Authenticated:** 1000 requests/hour
- **Service Account:** 10,000 requests/hour

### Handling Rate Limits

When you receive a 429 response:

```bash
# Retry after the reset time
reset_time=$(curl -I https://api.example.com/applications \
  -H "Authorization: Bearer YOUR_TOKEN" | grep "X-RateLimit-Reset" | cut -d' ' -f2)

sleep_time=$((reset_time - $(date +%s)))
sleep $sleep_time
# Retry the request
```

## Best Practices

### 1. Use Correlation IDs

Include a unique `X-Correlation-Id` header for traceability:

```bash
curl -H "X-Correlation-Id: order-process-12345" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  https://api.example.com/applications
```

### 2. Implement Retry Logic

Use exponential backoff for transient failures:

```bash
max_attempts=3
attempt=1
while [ $attempt -le $max_attempts ]; do
  response=$(curl -s -o /dev/null -w "%{http_code}" \
    https://api.example.com/applications \
    -H "Authorization: Bearer YOUR_TOKEN")
  
  if [ "$response" = "200" ]; then
    break
  elif [ "$response" = "429" ] || [ "$response" = "503" ]; then
    backoff=$((2 ** (attempt - 1)))
    sleep $backoff
  fi
  
  attempt=$((attempt + 1))
done
```

### 3. Cache When Appropriate

Cache GET responses to reduce API calls:

```bash
# Store ETags from responses
etag=$(curl -I https://api.example.com/applications/app-001 \
  -H "Authorization: Bearer YOUR_TOKEN" | grep -i "etag" | cut -d' ' -f2)

# Use cached response if unchanged
curl -H "If-None-Match: $etag" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  https://api.example.com/applications/app-001
```

### 4. Batch Operations

Retrieve related data efficiently:

```bash
# Get applications with their relations
curl "https://api.example.com/applications?include=relations" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 5. Use Appropriate Pagination

Adjust `limit` based on your needs, but avoid repeatedly iterating through all pages:

```bash
# Efficient: Get all items at once with larger limit
curl "https://api.example.com/applications?limit=200" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Inefficient: Multiple small requests
for i in {1..100}; do
  curl "https://api.example.com/applications?page=$i&limit=1" \
    -H "Authorization: Bearer YOUR_TOKEN"
done
```

### 6. Monitor Rate Limits

Track rate limit consumption to avoid hitting limits:

```bash
remaining=$(curl -I https://api.example.com/applications \
  -H "Authorization: Bearer YOUR_TOKEN" | grep "X-RateLimit-Remaining" | cut -d' ' -f2)

if [ $remaining -lt 100 ]; then
  echo "Warning: $remaining requests remaining"
fi
```

## Troubleshooting

### 401 Unauthorized

**Problem:** Authentication failed

**Solutions:**
- Verify token is not expired (compare `exp` claim to current time)
- Check token format: `Authorization: Bearer <token>`
- Verify OIDC provider is accessible
- For API keys, check header is `X-API-Key`

### 403 Forbidden

**Problem:** Insufficient permissions

**Solutions:**
- Verify user has required roles/permissions
- Check authorization policy allows the operation
- Confirm resource ownership or access grants
- Review audit logs for permission changes

### 404 Not Found

**Problem:** Resource doesn't exist

**Solutions:**
- Verify the resource ID is correct
- Check resource hasn't been deleted
- Ensure you're using the correct endpoint URL
- Verify pagination hasn't reached the end

### 429 Rate Limited

**Problem:** Too many requests

**Solutions:**
- Implement exponential backoff in retry logic
- Reduce request frequency
- Use larger pagination limits to reduce requests
- Consider service account tier for higher limits

### 500 Internal Server Error

**Problem:** Server error

**Solutions:**
- Retry the request (use exponential backoff)
- Check API status page for outages
- Review request payload for invalid data
- Contact support with correlation ID from response header

### Slow Responses

**Problem:** API calls are taking too long

**Solutions:**
- Reduce `limit` parameter (large pages take longer to serialize)
- Add filters to reduce result set size
- Use `search` to narrow down results
- Check server load and consider rate limiting
- Use connection pooling if making many requests

### Connection Timeout

**Problem:** Cannot connect to API

**Solutions:**
- Verify API endpoint URL is correct
- Check network connectivity
- Verify firewall/proxy settings allow the connection
- Check API status page for outages
- Try a different network if behind corporate proxy

## SDK Examples

### Python

```python
import requests
from datetime import datetime, timedelta

class EAToolClient:
    def __init__(self, base_url, token):
        self.base_url = base_url
        self.headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }
    
    def get_applications(self, page=1, limit=50, search=None, owner=None):
        params = {"page": page, "limit": limit}
        if search:
            params["search"] = search
        if owner:
            params["owner"] = owner
        
        response = requests.get(
            f"{self.base_url}/applications",
            params=params,
            headers=self.headers
        )
        response.raise_for_status()
        return response.json()
    
    def get_application(self, app_id):
        response = requests.get(
            f"{self.base_url}/applications/{app_id}",
            headers=self.headers
        )
        response.raise_for_status()
        return response.json()

# Usage
client = EAToolClient("https://api.example.com", "your-token")
apps = client.get_applications(search="payment", owner="john.doe")
```

### JavaScript/Node.js

```javascript
const axios = require('axios');

class EAToolClient {
  constructor(baseURL, token) {
    this.client = axios.create({
      baseURL,
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
  }

  async getApplications(page = 1, limit = 50, filters = {}) {
    const params = { page, limit, ...filters };
    const response = await this.client.get('/applications', { params });
    return response.data;
  }

  async getApplication(appId) {
    const response = await this.client.get(`/applications/${appId}`);
    return response.data;
  }
}

// Usage
const client = new EAToolClient('https://api.example.com', 'your-token');
const apps = await client.getApplications(1, 50, { search: 'payment', owner: 'john.doe' });
```

### cURL Examples

```bash
#!/bin/bash

API_URL="https://api.example.com"
TOKEN="your-access-token"

# Get all applications with pagination
get_applications() {
  local page=${1:-1}
  local limit=${2:-50}
  
  curl -X GET \
    "${API_URL}/applications?page=${page}&limit=${limit}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json"
}

# Search applications
search_applications() {
  local search_term=$1
  
  curl -X GET \
    "${API_URL}/applications?search=${search_term}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json"
}

# Get specific application
get_application() {
  local app_id=$1
  
  curl -X GET \
    "${API_URL}/applications/${app_id}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json"
}

# Usage
get_applications 1 50
search_applications "payment"
get_application "app-001"
```
