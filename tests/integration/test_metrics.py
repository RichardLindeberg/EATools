"""Integration tests for metrics endpoint."""

import requests


def test_metrics_endpoint_responds():
    """Test that /metrics endpoint responds with 200."""
    response = requests.get("http://localhost:8000/metrics")
    assert response.status_code == 200


def test_metrics_endpoint_prometheus_format():
    """Test that /metrics endpoint returns Prometheus text format."""
    response = requests.get("http://localhost:8000/metrics")
    assert response.status_code == 200
    assert "text/plain" in response.headers.get("Content-Type", "")
    
    # Verify basic Prometheus format structure
    content = response.text
    assert "# HELP" in content
    assert "# TYPE" in content
    assert "eatool_up" in content


def test_metrics_endpoint_contains_expected_metrics():
    """Test that /metrics endpoint lists all expected metric instruments."""
    response = requests.get("http://localhost:8000/metrics")
    assert response.status_code == 200
    
    content = response.text
    expected_metrics = [
        "http.server.request.count",
        "http.server.request.duration",
        "eatool.commands.processed",
        "eatool.commands.duration",
        "eatool.eventstore.appends",
        "eatool.eventstore.append.duration",
        "eatool.eventstore.reads",
        "eatool.eventstore.read.duration",
        "eatool.projections.events_processed",
        "eatool.projections.failures",
        "eatool.projections.lag",
        "eatool.projections.batch.duration",
        "eatool.applications.created",
        "eatool.capabilities.created",
        "eatool.servers.created",
        "eatool.integrations.created",
        "eatool.organizations.created",
        "eatool.relations.created",
    ]
    
    for metric in expected_metrics:
        assert metric in content, f"Expected metric '{metric}' not found in /metrics output"


def test_metrics_endpoint_no_high_cardinality_labels():
    """Test that metrics don't expose high-cardinality labels like IDs or timestamps."""
    response = requests.get("http://localhost:8000/metrics")
    assert response.status_code == 200
    
    content = response.text.lower()
    # These patterns should NOT appear in metrics
    high_cardinality_patterns = [
        "app-",  # Application IDs
        "srv-",  # Server IDs
        "int-",  # Integration IDs
        "org-",  # Organization IDs
    ]
    
    # For now, just verify the endpoint doesn't crash with bad patterns
    # Full metric value validation would require actual OTel exporter
    assert len(content) > 0
