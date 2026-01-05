import pytest
from conftest import APIClient


@pytest.mark.integration


class TestBusinessCapabilities:
    def test_list_capabilities(self, client: APIClient):
        """GET /business-capabilities should return 200 with paginated list."""
        response = client.get("/business-capabilities")

        assert response.status_code == 200
        data = response.json()

        assert "items" in data or isinstance(data, list)
        items = data.get("items", []) if isinstance(data, dict) else data

        if isinstance(data, dict):
            assert "page" in data
            assert "limit" in data
            assert "total" in data

        if isinstance(items, list) and len(items) > 0:
            cap = items[0]
            assert "id" in cap
            assert "name" in cap
            assert "created_at" in cap
            assert "updated_at" in cap

    def test_list_capabilities_with_pagination(self, client: APIClient):
        """GET /business-capabilities with page/limit params."""
        response = client.get("/business-capabilities", params={"page": 1, "limit": 5})

        assert response.status_code == 200
        data = response.json()

        assert isinstance(data, dict)
        assert "items" in data
        assert data.get("page") == 1
        assert data.get("limit") == 5
        assert isinstance(data.get("items", []), list)
        assert len(data["items"]) <= 5

    def test_create_capability(self, client: APIClient):
        """POST /business-capabilities should create a capability."""
        payload = {
            "name": "Payments",
            "parent_id": None,
        }
        response = client.post("/business-capabilities", json=payload)

        assert response.status_code in [200, 201]
        data = response.json()

        assert "id" in data
        assert data["name"] == payload["name"]

        cap_id = data.get("id")
        if cap_id:
            client.delete(f"/business-capabilities/{cap_id}")

    def test_create_capability_missing_required_field(self, client: APIClient):
        """POST /business-capabilities without required 'name' should return 400 or 422."""
        payload = {"parent_id": "cap-parent"}
        response = client.post("/business-capabilities", json=payload)

        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_capability(self, client: APIClient):
        """GET /business-capabilities/{id} should return 200."""
        create_resp = client.post(
            "/business-capabilities",
            json={"name": "Capability A"},
        )
        assert create_resp.status_code in [200, 201]
        cap_id = create_resp.json()["id"]

        get_resp = client.get(f"/business-capabilities/{cap_id}")
        assert get_resp.status_code == 200
        data = get_resp.json()
        assert data["id"] == cap_id
        assert data["name"]

        client.delete(f"/business-capabilities/{cap_id}")

    def test_get_nonexistent_capability(self, client: APIClient):
        """GET /business-capabilities/{id} for unknown id should return 404."""
        response = client.get("/business-capabilities/nonexistent-id")
        assert response.status_code == 404

    def test_update_capability(self, client: APIClient):
        """PATCH /business-capabilities/{id} should update fields."""
        create_resp = client.post(
            "/business-capabilities",
            json={"name": "Capability Old"},
        )
        assert create_resp.status_code in [200, 201]
        cap_id = create_resp.json()["id"]

        update_payload = {"name": "Capability New", "parent_id": None}
        update_resp = client.patch(f"/business-capabilities/{cap_id}", json=update_payload)

        assert update_resp.status_code in [200, 202]
        data = update_resp.json()
        assert data["id"] == cap_id
        assert data["name"] == update_payload["name"]

        client.delete(f"/business-capabilities/{cap_id}")

    def test_delete_capability(self, client: APIClient):
        """DELETE /business-capabilities/{id} should delete the capability."""
        create_resp = client.post(
            "/business-capabilities",
            json={"name": "Capability Temp"},
        )
        assert create_resp.status_code in [200, 201]
        cap_id = create_resp.json()["id"]

        delete_resp = client.delete(f"/business-capabilities/{cap_id}")
        assert delete_resp.status_code in [200, 202, 204]

        get_resp = client.get(f"/business-capabilities/{cap_id}")
        assert get_resp.status_code == 404
