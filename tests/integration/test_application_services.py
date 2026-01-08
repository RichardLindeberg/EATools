import pytest
from conftest import APIClient


@pytest.mark.integration
class TestApplicationServices:
    def test_create_and_get_service(self, client: APIClient):
        payload = {
            "name": "Payments Service",
            "description": "Handles payment processing",
            "business_capability_id": "cap-payments",
            "sla": "99.9",
            "exposed_by_app_ids": ["app-core"],
            "tags": ["payments", "core"],
        }
        create_resp = client.post("/application-services", json=payload)
        assert create_resp.status_code in [200, 201]
        svc = create_resp.json()
        assert "id" in svc
        svc_id = svc["id"]

        get_resp = client.get(f"/application-services/{svc_id}")
        assert get_resp.status_code == 200
        body = get_resp.json()
        assert body["name"] == payload["name"]
        assert body.get("business_capability_id") == payload["business_capability_id"]

        client.post(f"/application-services/{svc_id}/commands/delete")

    def test_update_command_flow(self, client: APIClient):
        create_resp = client.post(
            "/application-services",
            json={
                "name": "Catalog Service",
                "business_capability_id": "cap-catalog",
                "tags": ["catalog"],
            },
        )
        assert create_resp.status_code in [200, 201]
        svc_id = create_resp.json()["id"]

        update_resp = client.post(
            f"/application-services/{svc_id}/commands/update",
            json={
                "name": "Catalog Service v2",
                "description": "Updated description",
                "sla": "99.5",
                "tags": ["catalog", "v2"],
            },
        )
        assert update_resp.status_code == 200
        data = update_resp.json()
        assert data["name"] == "Catalog Service v2"
        assert data.get("description") == "Updated description"
        assert "v2" in data.get("tags", [])

        client.post(f"/application-services/{svc_id}/commands/delete")

    def test_set_business_capability_and_filter(self, client: APIClient):
        create_resp = client.post(
            "/application-services",
            json={"name": "Reporting Service", "business_capability_id": None},
        )
        assert create_resp.status_code in [200, 201]
        svc_id = create_resp.json()["id"]

        cmd_resp = client.post(
            f"/application-services/{svc_id}/commands/set-business-capability",
            json={"business_capability_id": "cap-reporting"},
        )
        assert cmd_resp.status_code == 200

        list_resp = client.get(
            "/application-services",
            params={"business_capability_id": "cap-reporting", "limit": 10},
        )
        assert list_resp.status_code == 200
        items = list_resp.json().get("items", [])
        assert any(item.get("id") == svc_id for item in items)

        client.post(f"/application-services/{svc_id}/commands/delete")

    def test_add_consumer_command(self, client: APIClient):
        create_resp = client.post(
            "/application-services",
            json={"name": "Audit Service", "exposed_by_app_ids": ["app-audit"]},
        )
        assert create_resp.status_code in [200, 201]
        svc_id = create_resp.json()["id"]

        cmd_resp = client.post(
            f"/application-services/{svc_id}/commands/add-consumer",
            json={"app_id": "app-consumer"},
        )
        assert cmd_resp.status_code == 200
        data = cmd_resp.json()
        assert "app-consumer" in data.get("consumers", [])

        client.post(f"/application-services/{svc_id}/commands/delete")

    def test_validation_missing_name(self, client: APIClient):
        resp = client.post(
            "/application-services",
            json={"description": "no name"},
        )
        assert resp.status_code in [400, 422]

    def test_delete_service(self, client: APIClient):
        create_resp = client.post(
            "/application-services",
            json={"name": "Temp Service"},
        )
        assert create_resp.status_code in [200, 201]
        svc_id = create_resp.json()["id"]

        delete_resp = client.post(f"/application-services/{svc_id}/commands/delete")
        assert delete_resp.status_code in [200, 204]

        get_resp = client.get(f"/application-services/{svc_id}")
        assert get_resp.status_code == 404
