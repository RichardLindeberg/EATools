import pytest
from conftest import APIClient


@pytest.mark.integration


class TestServers:
    def test_list_servers(self, client: APIClient):
        """GET /servers should return 200 with paginated list."""
        response = client.get("/servers")

        assert response.status_code == 200
        data = response.json()

        assert "items" in data or isinstance(data, list)
        items = data.get("items", []) if isinstance(data, dict) else data

        if isinstance(data, dict):
            assert "page" in data
            assert "limit" in data
            assert "total" in data

        if isinstance(items, list) and len(items) > 0:
            srv = items[0]
            assert "id" in srv
            assert "hostname" in srv
            assert "created_at" in srv
            assert "updated_at" in srv

    def test_list_servers_with_pagination(self, client: APIClient):
        """GET /servers with page/limit params."""
        response = client.get("/servers", params={"page": 1, "limit": 5})

        assert response.status_code == 200
        data = response.json()

        assert isinstance(data, dict)
        assert "items" in data
        assert data.get("page") == 1
        assert data.get("limit") == 5
        assert isinstance(data.get("items", []), list)
        assert len(data["items"]) <= 5

    def test_list_servers_with_filters(self, client: APIClient):
        """GET /servers with environment/region filters should still succeed."""
        response = client.get("/servers", params={"environment": "prod", "region": "eu"})

        assert response.status_code == 200
        data = response.json()
        assert "items" in data or isinstance(data, list)

    def test_create_server(self, client: APIClient):
        """POST /servers should create a server."""
        payload = {
            "hostname": "srv-test-01",
            "environment": "staging",
            "region": "eu-west-1",
            "platform": "linux",
            "criticality": "medium",
            "owning_team": "platform",
            "tags": ["platform", "staging"],
        }
        response = client.post("/servers", json=payload)

        assert response.status_code in [200, 201]
        data = response.json()

        assert "id" in data
        assert data["hostname"] == payload["hostname"]

        srv_id = data.get("id")
        if srv_id:
            client.delete(f"/servers/{srv_id}")

    def test_create_server_missing_required_field(self, client: APIClient):
        """POST /servers without required 'hostname' should return 400 or 422."""
        payload = {"environment": "staging"}
        response = client.post("/servers", json=payload)

        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_server(self, client: APIClient):
        """GET /servers/{id} should return 200."""
        create_resp = client.post(
            "/servers",
            json={
                "hostname": "srv-get-01",
                "environment": "dev",
            },
        )
        assert create_resp.status_code in [200, 201]
        srv_id = create_resp.json()["id"]

        get_resp = client.get(f"/servers/{srv_id}")
        assert get_resp.status_code == 200
        data = get_resp.json()
        assert data["id"] == srv_id
        assert data["hostname"]

        client.delete(f"/servers/{srv_id}")

    def test_get_nonexistent_server(self, client: APIClient):
        """GET /servers/{id} for unknown id should return 404."""
        response = client.get("/servers/nonexistent-id")
        assert response.status_code == 404

    def test_update_server(self, client: APIClient):
        """PATCH /servers/{id} should update fields."""
        create_resp = client.post(
            "/servers",
            json={
                "hostname": "srv-update-01",
                "environment": "dev",
            },
        )
        assert create_resp.status_code in [200, 201]
        srv_id = create_resp.json()["id"]

        update_payload = {
            "hostname": "srv-update-02",
            "environment": "prod",
            "region": "us-east-1",
            "owning_team": "sre",
        }
        update_resp = client.patch(f"/servers/{srv_id}", json=update_payload)

        assert update_resp.status_code in [200, 202]
        data = update_resp.json()
        assert data["id"] == srv_id
        assert data["hostname"] == update_payload["hostname"]
        assert data.get("environment") == update_payload["environment"]
        assert data.get("region") == update_payload["region"]

        client.delete(f"/servers/{srv_id}")

    def test_delete_server(self, client: APIClient):
        """DELETE /servers/{id} should delete the server."""
        create_resp = client.post(
            "/servers",
            json={
                "hostname": "srv-delete-01",
                "environment": "dev",
            },
        )
        assert create_resp.status_code in [200, 201]
        srv_id = create_resp.json()["id"]

        delete_resp = client.delete(f"/servers/{srv_id}")
        assert delete_resp.status_code in [200, 202, 204]

        get_resp = client.get(f"/servers/{srv_id}")
        assert get_resp.status_code == 404
