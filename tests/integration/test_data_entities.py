import pytest
from conftest import APIClient


@pytest.mark.integration
class TestDataEntities:
    def test_list_data_entities(self, client: APIClient):
        """GET /data-entities should return 200 with paginated list."""
        response = client.get("/data-entities")

        assert response.status_code == 200
        data = response.json()

        assert "items" in data or isinstance(data, list)
        items = data.get("items", []) if isinstance(data, dict) else data

        if isinstance(data, dict):
            assert "page" in data
            assert "limit" in data
            assert "total" in data

        if isinstance(items, list) and len(items) > 0:
            entity = items[0]
            assert "id" in entity
            assert "name" in entity
            assert "classification" in entity
            assert "created_at" in entity
            assert "updated_at" in entity

    def test_list_data_entities_with_pagination(self, client: APIClient):
        """GET /data-entities with page/limit params."""
        response = client.get("/data-entities", params={"page": 1, "limit": 3})

        assert response.status_code == 200
        data = response.json()

        assert isinstance(data, dict)
        assert "items" in data
        assert data.get("page") == 1
        assert data.get("limit") == 3
        assert isinstance(data.get("items", []), list)
        assert len(data["items"]) <= 3

    def test_create_data_entity(self, client: APIClient):
        """POST /data-entities should create a data entity."""
        payload = {
            "name": "Customer",
            "classification": "internal",
            "domain": "sales",
            "pii_flag": True,
            "glossary_terms": ["customer"],
            "lineage": [],
        }
        response = client.post("/data-entities", json=payload)

        assert response.status_code in [200, 201]
        data = response.json()

        assert "id" in data
        assert data["name"] == payload["name"]
        assert data["classification"] == payload["classification"]

        entity_id = data.get("id")
        if entity_id:
            client.delete(f"/data-entities/{entity_id}")

    def test_create_data_entity_missing_required_field(self, client: APIClient):
        """POST /data-entities without required 'name' should return 400 or 422."""
        payload = {"classification": "internal"}
        response = client.post("/data-entities", json=payload)

        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_data_entity(self, client: APIClient):
        """GET /data-entities/{id} should return 200."""
        create_resp = client.post(
            "/data-entities",
            json={"name": "Order", "classification": "internal"},
        )
        assert create_resp.status_code in [200, 201]
        entity_id = create_resp.json()["id"]

        get_resp = client.get(f"/data-entities/{entity_id}")
        assert get_resp.status_code == 200
        data = get_resp.json()
        assert data["id"] == entity_id
        assert data["name"]

        client.delete(f"/data-entities/{entity_id}")

    def test_get_nonexistent_data_entity(self, client: APIClient):
        """GET /data-entities/{id} for unknown id should return 404."""
        response = client.get("/data-entities/nonexistent-id")
        assert response.status_code == 404

    def test_update_data_entity(self, client: APIClient):
        """PATCH /data-entities/{id} should update fields."""
        create_resp = client.post(
            "/data-entities",
            json={"name": "Legacy", "classification": "public"},
        )
        assert create_resp.status_code in [200, 201]
        entity_id = create_resp.json()["id"]

        update_payload = {
            "name": "Modern",
            "classification": "confidential",
            "pii_flag": False,
        }
        update_resp = client.patch(f"/data-entities/{entity_id}", json=update_payload)

        assert update_resp.status_code in [200, 202]
        data = update_resp.json()
        assert data["id"] == entity_id
        assert data["name"] == update_payload["name"]
        assert data["classification"] == update_payload["classification"]

        client.delete(f"/data-entities/{entity_id}")

    def test_delete_data_entity(self, client: APIClient):
        """DELETE /data-entities/{id} should delete the data entity."""
        create_resp = client.post(
            "/data-entities",
            json={"name": "Temp Data", "classification": "internal"},
        )
        assert create_resp.status_code in [200, 201]
        entity_id = create_resp.json()["id"]

        delete_resp = client.delete(f"/data-entities/{entity_id}")
        assert delete_resp.status_code in [200, 202, 204]

        get_resp = client.get(f"/data-entities/{entity_id}")
        assert get_resp.status_code == 404
