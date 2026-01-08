import pytest
from conftest import APIClient


@pytest.mark.integration
class TestApplicationInterfaces:
    def test_create_and_get_interface(self, client: APIClient):
        payload = {
            "name": "Public API",
            "protocol": "rest",
            "exposed_by_app_id": "app-api",
            "serves_service_ids": ["svc-public"],
            "status": "active",
        }
        create_resp = client.post("/application-interfaces", json=payload)
        assert create_resp.status_code in [200, 201]
        iface = create_resp.json()
        iface_id = iface["id"]

        get_resp = client.get(f"/application-interfaces/{iface_id}")
        assert get_resp.status_code == 200
        body = get_resp.json()
        assert body["name"] == payload["name"]
        assert body.get("exposed_by_app_id") == payload["exposed_by_app_id"]

        client.post(f"/application-interfaces/{iface_id}/commands/delete")

    def test_update_interface_command(self, client: APIClient):
        create_resp = client.post(
            "/application-interfaces",
            json={
                "name": "Billing API",
                "protocol": "grpc",
                "exposed_by_app_id": "app-billing",
                "status": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        iface_id = create_resp.json()["id"]

        update_resp = client.post(
            f"/application-interfaces/{iface_id}/commands/update",
            json={
                "name": "Billing API v2",
                "protocol": "rest",
                "endpoint": "https://billing.example.com",
                "version": "v2",
                "tags": ["billing", "v2"],
            },
        )
        assert update_resp.status_code == 200
        data = update_resp.json()
        assert data["name"] == "Billing API v2"
        assert data.get("protocol") == "rest"
        assert data.get("version") == "v2"

        client.post(f"/application-interfaces/{iface_id}/commands/delete")

    def test_set_service_and_status_transitions(self, client: APIClient):
        create_resp = client.post(
            "/application-interfaces",
            json={
                "name": "Reporting API",
                "protocol": "rest",
                "exposed_by_app_id": "app-reporting",
                "status": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        iface_id = create_resp.json()["id"]

        set_service = client.post(
            f"/application-interfaces/{iface_id}/commands/set-service",
            json={"service_ids": ["svc-reporting"]},
        )
        assert set_service.status_code == 200
        assert "svc-reporting" in set_service.json().get("serves_service_ids", [])

        deprecate = client.post(f"/application-interfaces/{iface_id}/commands/deprecate")
        assert deprecate.status_code == 200
        assert deprecate.json().get("status") == "deprecated"

        retire = client.post(f"/application-interfaces/{iface_id}/commands/retire")
        assert retire.status_code == 200
        assert retire.json().get("status") == "retired"

        client.post(f"/application-interfaces/{iface_id}/commands/delete")

    def test_filter_by_application_and_status(self, client: APIClient):
        create_resp = client.post(
            "/application-interfaces",
            json={
                "name": "Inventory API",
                "protocol": "rest",
                "exposed_by_app_id": "app-inventory",
                "status": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        iface_id = create_resp.json()["id"]

        list_resp = client.get(
            "/application-interfaces",
            params={"application_id": "app-inventory", "status": "active"},
        )
        assert list_resp.status_code == 200
        items = list_resp.json().get("items", [])
        assert any(item.get("id") == iface_id for item in items)

        client.post(f"/application-interfaces/{iface_id}/commands/delete")

    def test_validation_missing_name(self, client: APIClient):
        resp = client.post(
            "/application-interfaces",
            json={"protocol": "rest", "exposed_by_app_id": "app-test", "status": "active"},
        )
        assert resp.status_code in [400, 422]

    def test_delete_interface(self, client: APIClient):
        create_resp = client.post(
            "/application-interfaces",
            json={
                "name": "Temp Interface",
                "protocol": "rest",
                "exposed_by_app_id": "app-temp",
                "status": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        iface_id = create_resp.json()["id"]

        delete_resp = client.post(f"/application-interfaces/{iface_id}/commands/delete")
        assert delete_resp.status_code in [200, 204]

        get_resp = client.get(f"/application-interfaces/{iface_id}")
        assert get_resp.status_code == 404
