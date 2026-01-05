import pytest
from conftest import APIClient


@pytest.mark.integration


class TestIntegrations:
    def test_list_integrations(self, client: APIClient):
        """GET /integrations should return 200 with paginated list."""
        response = client.get("/integrations")

        assert response.status_code == 200
        data = response.json()

        assert "items" in data or isinstance(data, list)
        items = data.get("items", []) if isinstance(data, dict) else data

        if isinstance(data, dict):
            assert "page" in data
            assert "limit" in data
            assert "total" in data

        if isinstance(items, list) and len(items) > 0:
            integ = items[0]
            assert "id" in integ
            assert "source_app_id" in integ
            assert "target_app_id" in integ
            assert "created_at" in integ
            assert "updated_at" in integ

    def test_list_integrations_with_pagination(self, client: APIClient):
        """GET /integrations with page/limit params."""
        response = client.get("/integrations", params={"page": 1, "limit": 5})

        assert response.status_code == 200
        data = response.json()

        assert isinstance(data, dict)
        assert "items" in data
        assert data.get("page") == 1
        assert data.get("limit") == 5
        assert isinstance(data.get("items", []), list)
        assert len(data["items"]) <= 5

    def test_list_integrations_with_filters(self, client: APIClient):
        """GET /integrations with source/target filters should still succeed."""
        response = client.get("/integrations", params={"source_app_id": "app-1", "target_app_id": "app-2"})

        assert response.status_code == 200
        data = response.json()
        assert "items" in data or isinstance(data, list)

    def test_create_integration(self, client: APIClient):
        """POST /integrations should create an integration."""
        payload = {
            "source_app_id": "app-source",
            "target_app_id": "app-target",
            "protocol": "https",
            "data_contract": "json",
            "frequency": "daily",
            "tags": ["sync"],
        }
        response = client.post("/integrations", json=payload)

        assert response.status_code in [200, 201]
        data = response.json()

        assert "id" in data
        assert data["source_app_id"] == payload["source_app_id"]
        assert data["target_app_id"] == payload["target_app_id"]

        integ_id = data.get("id")
        if integ_id:
            client.delete(f"/integrations/{integ_id}")

    def test_create_integration_missing_required_field(self, client: APIClient):
        """POST /integrations without required fields should return 400 or 422."""
        payload = {"source_app_id": "only-source"}
        response = client.post("/integrations", json=payload)

        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_integration(self, client: APIClient):
        """GET /integrations/{id} should return 200."""
        create_resp = client.post(
            "/integrations",
            json={
                "source_app_id": "app-get-src",
                "target_app_id": "app-get-tgt",
            },
        )
        assert create_resp.status_code in [200, 201]
        integ_id = create_resp.json()["id"]

        get_resp = client.get(f"/integrations/{integ_id}")
        assert get_resp.status_code == 200
        data = get_resp.json()
        assert data["id"] == integ_id
        assert data["source_app_id"]
        assert data["target_app_id"]

        client.delete(f"/integrations/{integ_id}")

    def test_get_nonexistent_integration(self, client: APIClient):
        """GET /integrations/{id} for unknown id should return 404."""
        response = client.get("/integrations/nonexistent-id")
        assert response.status_code == 404

    def test_update_integration(self, client: APIClient):
        """PATCH /integrations/{id} should update fields."""
        create_resp = client.post(
            "/integrations",
            json={
                "source_app_id": "app-update-src",
                "target_app_id": "app-update-tgt",
            },
        )
        assert create_resp.status_code in [200, 201]
        integ_id = create_resp.json()["id"]

        update_payload = {
            "source_app_id": "app-update-src-2",
            "target_app_id": "app-update-tgt-2",
            "protocol": "kafka",
            "frequency": "hourly",
            "tags": ["batch"],
        }
        update_resp = client.patch(f"/integrations/{integ_id}", json=update_payload)

        assert update_resp.status_code in [200, 202]
        data = update_resp.json()
        assert data["id"] == integ_id
        assert data["source_app_id"] == update_payload["source_app_id"]
        assert data["target_app_id"] == update_payload["target_app_id"]

        client.delete(f"/integrations/{integ_id}")

    def test_delete_integration(self, client: APIClient):
        """DELETE /integrations/{id} should delete the integration."""
        create_resp = client.post(
            "/integrations",
            json={
                "source_app_id": "app-del-src",
                "target_app_id": "app-del-tgt",
            },
        )
        assert create_resp.status_code in [200, 201]
        integ_id = create_resp.json()["id"]

        delete_resp = client.delete(f"/integrations/{integ_id}")
        assert delete_resp.status_code in [200, 202, 204]

        get_resp = client.get(f"/integrations/{integ_id}")
        assert get_resp.status_code == 404
