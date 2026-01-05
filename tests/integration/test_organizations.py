"""Integration tests for Organizations endpoints."""

import pytest
from conftest import APIClient


@pytest.mark.integration
class TestOrganizations:
    """Tests for /organizations endpoints."""

    def test_list_organizations(self, client: APIClient):
        """GET /organizations should return 200 with paginated list."""
        response = client.get("/organizations")
        
        assert response.status_code == 200
        data = response.json()
        
        # Validate response structure matches OpenAPI spec
        assert "items" in data or isinstance(data, list)
        # Each item should have required fields
        if isinstance(data, list) and len(data) > 0:
            org = data[0]
            assert "id" in org
            assert "name" in org
            assert "created_at" in org
            assert "updated_at" in org

    def test_list_organizations_with_pagination(self, client: APIClient):
        """GET /organizations with page/limit params."""
        response = client.get("/organizations", params={"page": 1, "limit": 10})
        
        assert response.status_code == 200
        data = response.json()
        assert data is not None

    def test_list_organizations_with_search(self, client: APIClient):
        """GET /organizations with search param."""
        response = client.get("/organizations", params={"search": "example"})
        
        assert response.status_code == 200

    def test_create_organization(self, client: APIClient):
        """POST /organizations should create and return 201."""
        payload = {
            "name": "Test Organization",
            "domains": ["test.example.com"],
            "contacts": ["admin@example.com"],
        }
        response = client.post("/organizations", json=payload)
        
        # Should return 201 or 200 depending on implementation
        assert response.status_code in [200, 201]
        data = response.json()
        
        assert "id" in data
        assert data["name"] == payload["name"]
        
        return data["id"]  # Return ID for cleanup tests

    def test_create_organization_missing_required_field(self, client: APIClient):
        """POST /organizations without required 'name' should return 400 or 422."""
        payload = {
            "domains": ["test.example.com"],
        }
        response = client.post("/organizations", json=payload)
        
        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_organization(self, client: APIClient):
        """GET /organizations/{id} should return 200."""
        # First create an org
        create_response = client.post(
            "/organizations",
            json={"name": "Org for Get Test"},
        )
        
        if create_response.status_code not in [200, 201]:
            pytest.skip("Failed to create organization for test")
        
        org_id = create_response.json()["id"]
        
        # Now get it
        response = client.get(f"/organizations/{org_id}")
        
        assert response.status_code == 200
        data = response.json()
        assert data["id"] == org_id
        assert data["name"] == "Org for Get Test"

    def test_get_nonexistent_organization(self, client: APIClient):
        """GET /organizations/{nonexistent_id} should return 404."""
        response = client.get("/organizations/nonexistent-id-12345")
        
        assert response.status_code == 404

    def test_update_organization(self, client: APIClient):
        """PATCH /organizations/{id} should update and return 200."""
        # Create
        create_response = client.post(
            "/organizations",
            json={"name": "Original Name"},
        )
        org_id = create_response.json()["id"]
        
        # Update
        patch_response = client.patch(
            f"/organizations/{org_id}",
            json={"name": "Updated Name"},
        )
        
        assert patch_response.status_code == 200
        data = patch_response.json()
        assert data["name"] == "Updated Name"

    def test_delete_organization(self, client: APIClient):
        """DELETE /organizations/{id} should return 204."""
        # Create
        create_response = client.post(
            "/organizations",
            json={"name": "Org to Delete"},
        )
        org_id = create_response.json()["id"]
        
        # Delete
        response = client.delete(f"/organizations/{org_id}")
        
        assert response.status_code == 204
        
        # Verify it's gone (should get 404)
        get_response = client.get(f"/organizations/{org_id}")
        assert get_response.status_code == 404
