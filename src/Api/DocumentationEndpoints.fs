namespace EATool.Api

open System
open System.IO
open Microsoft.AspNetCore.Http
open Giraffe

/// OpenAPI and Documentation endpoints
module DocumentationEndpoints =
    
    /// Serve OpenAPI specification as JSON
    let getOpenApiJson : HttpHandler =
        fun next ctx ->
            task {
                try
                    let openApiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "openapi.yaml")
                    if File.Exists(openApiPath) then
                        let content = File.ReadAllText(openApiPath)
                        // Could convert YAML to JSON here if needed, for now return as-is with YAML content-type
                        ctx.SetContentType "application/x-yaml"
                        return! text content next ctx
                    else
                        return! RequestErrors.NOT_FOUND "OpenAPI specification not found" next ctx
                with
                | ex ->
                    ctx.SetStatusCode 500
                    return! text (sprintf "Error loading OpenAPI spec: %s" ex.Message) next ctx
            }
    
    /// Serve Swagger UI for OpenAPI documentation
    let getSwaggerUI : HttpHandler =
        fun next ctx ->
            task {
                let htmlResponse = """<!DOCTYPE html>
<html>
<head>
    <title>API Documentation - EA Tool</title>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@3/swagger-ui.css">
    <style>
        body {
            margin: 0;
            padding: 0;
            background: #fafafa;
        }
        .topbar {
            background-color: #1e90ff;
            padding: 20px;
            color: white;
            text-align: center;
            box-shadow: 0 1px 2px rgba(0,0,0,.15);
        }
        .topbar h1 {
            margin: 0;
            font-size: 2em;
            font-weight: 300;
        }
        .topbar p {
            margin: 5px 0 0 0;
            font-size: 0.9em;
            opacity: 0.9;
        }
    </style>
</head>
<body>
    <div class="topbar">
        <h1>EA Tool API Documentation</h1>
        <p>Interactive OpenAPI Specification</p>
    </div>
    <div id="swagger-ui"></div>
    <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@3/swagger-ui-bundle.js"></script>
    <script>
        SwaggerUIBundle({
            url: "/api/documentation/openapi",
            dom_id: '#swagger-ui',
            presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIBundle.SwaggerUIStandalonePreset
            ],
            layout: "BaseLayout",
            requestInterceptor: (request) => {
                request.headers['X-Request-ID'] = 'swagger-ui-' + Date.now();
                return request;
            }
        });
    </script>
</body>
</html>"""
                
                return! htmlString htmlResponse next ctx
            }
    
    /// Serve ReDoc UI for OpenAPI documentation (alternative)
    let getReDocUI : HttpHandler =
        fun next ctx ->
            task {
                let html = """<!DOCTYPE html>
<html>
<head>
    <title>API Documentation - EA Tool (ReDoc)</title>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link href="https://fonts.googleapis.com/css?family=Montserrat:300,400,700|Roboto:300,400,700" rel="stylesheet">
    <style>
        body {
            margin: 0;
            padding: 0;
            background: #fafafa;
        }
    </style>
</head>
<body>
    <redoc spec-url="/api/documentation/openapi"></redoc>
    <script src="https://cdn.jsdelivr.net/npm/redoc@latest/bundles/redoc.standalone.js"></script>
</body>
</html>"""
                
                return! htmlString html next ctx
            }
    
    /// Serve markdown-rendered API guide
    let getApiGuide : HttpHandler =
        fun next ctx ->
            task {
                let markdown = """# EA Tool API Guide

## Overview

The EA Tool API provides a comprehensive REST API for managing enterprise architecture components.

### Base URL

```
http://localhost:8000/api
```

### Authentication

All endpoints require authentication via:
- **Bearer Token**: OpenID Connect (OIDC) JWT tokens
- **API Key**: Custom API key (X-API-Key header)

### Response Format

All responses are JSON unless otherwise specified.

### Error Handling

Errors follow standard HTTP status codes:
- `200 OK` - Success
- `400 Bad Request` - Invalid parameters
- `401 Unauthorized` - Authentication failed
- `403 Forbidden` - Authorization failed
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

Error responses include a detailed error object:

```json
{
  "code": "INVALID_REQUEST",
  "message": "Description of the error",
  "details": {}
}
```

## Endpoints

### Health Check

**GET** `/api/health`

Check if the API is running.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-17T10:00:00Z"
}
```

### Applications

#### List Applications

**GET** `/api/applications`

Get all applications.

#### Create Application

**POST** `/api/applications`

Create a new application.

**Request:**
```json
{
  "name": "My App",
  "description": "Application description",
  "owner": "owner@example.com",
  "environment": "production"
}
```

### Servers

#### List Servers

**GET** `/api/servers`

Get all servers.

#### Create Server

**POST** `/api/servers`

Create a new server.

**Request:**
```json
{
  "name": "Web Server",
  "host": "server.example.com",
  "environment": "production",
  "osType": "Linux"
}
```

### Integrations

#### List Integrations

**GET** `/api/integrations`

Get all integrations.

#### Create Integration

**POST** `/api/integrations`

Create a new integration.

**Request:**
```json
{
  "name": "Integration Name",
  "protocol": "REST",
  "sla": "99.5%",
  "frequency": "real-time"
}
```

## Best Practices

### Error Handling

Always check the HTTP status code and handle errors appropriately.

### Rate Limiting

API rate limits are applied per client. Check response headers for rate limit information.

### Pagination

List endpoints support pagination with `skip` and `take` parameters.

```
GET /api/applications?skip=0&take=20
```

### Filtering

Use query parameters to filter results.

```
GET /api/applications?environment=production&owner=team@example.com
```

## OpenAPI Documentation

For complete API documentation, visit:
- [Swagger UI](/api/documentation/swagger)
- [ReDoc UI](/api/documentation/redoc)

## Support

For issues or questions, please contact the EA Platform Team.
"""
                let htmlContent = MarkdownRenderer.renderMarkdown markdown
                let wrappedHtml = MarkdownRenderer.wrapHtml (Some "EA Tool API Guide") htmlContent
                ctx.SetContentType "text/html; charset=utf-8"
                return! htmlString wrappedHtml next ctx
            }
    
    /// All documentation routes
    let routes : HttpHandler list =
        [
            GET >=> route "/api/documentation/openapi" >=> getOpenApiJson
            GET >=> route "/api/documentation/swagger" >=> getSwaggerUI
            GET >=> route "/api/documentation/redoc" >=> getReDocUI
            GET >=> route "/api/documentation/guide" >=> getApiGuide
            GET >=> route "/docs" >=> getSwaggerUI  // Convenience alias
        ]
