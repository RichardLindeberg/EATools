"""
Integration tests for health endpoint
"""

import pytest
import requests


class TestHealth:
    """Test suite for health endpoint"""

    BASE_URL = "http://localhost:8000"

    def test_health_endpoint_responds(self):
        """Test that health endpoint returns 200 OK"""
        response = requests.get(f"{self.BASE_URL}/health")
        assert response.status_code == 200

    def test_health_endpoint_structure(self):
        """Test that health endpoint returns expected JSON structure"""
        response = requests.get(f"{self.BASE_URL}/health")
        assert response.status_code == 200
        
        data = response.json()
        
        # Check required fields
        assert "status" in data
        assert "service" in data
        assert "version" in data
        assert "environment" in data
        assert "instance_id" in data
        assert "uptime_seconds" in data
        assert "timestamp" in data
        assert "observability" in data
        
        # Check observability sub-structure
        obs = data["observability"]
        assert "tracing_enabled" in obs
        assert "metrics_enabled" in obs
        assert "otlp_endpoint" in obs
        
        # Check types
        assert isinstance(data["status"], str)
        assert isinstance(data["service"], str)
        assert isinstance(data["version"], str)
        assert isinstance(data["environment"], str)
        assert isinstance(data["uptime_seconds"], (int, float))
        assert isinstance(obs["tracing_enabled"], bool)
        assert isinstance(obs["metrics_enabled"], bool)

    def test_health_endpoint_values(self):
        """Test that health endpoint returns sensible values"""
        response = requests.get(f"{self.BASE_URL}/health")
        assert response.status_code == 200
        
        data = response.json()
        
        # Status should be healthy
        assert data["status"] == "healthy"
        
        # Service name should be set
        assert data["service"] in ["eatool-api", "eatool"]
        
        # Uptime should be non-negative
        assert data["uptime_seconds"] >= 0
        
        # Timestamp should be ISO 8601 format
        assert "T" in data["timestamp"]
        assert "Z" in data["timestamp"] or "+" in data["timestamp"]

    def test_health_endpoint_multiple_calls(self):
        """Test that uptime increases between calls"""
        import time
        
        response1 = requests.get(f"{self.BASE_URL}/health")
        assert response1.status_code == 200
        uptime1 = response1.json()["uptime_seconds"]
        
        # Wait a bit
        time.sleep(0.5)
        
        response2 = requests.get(f"{self.BASE_URL}/health")
        assert response2.status_code == 200
        uptime2 = response2.json()["uptime_seconds"]
        
        # Uptime should have increased
        assert uptime2 > uptime1
