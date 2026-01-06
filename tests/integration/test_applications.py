import pytest
from conftest import APIClient


@pytest.mark.integration


class TestApplications:
    def test_list_applications(self, client: APIClient):
        """GET /applications should return 200 with paginated list."""
        response = client.get("/applications")

        assert response.status_code == 200
        data = response.json()

        assert "items" in data or isinstance(data, list)
        items = data.get("items", []) if isinstance(data, dict) else data

        if isinstance(data, dict):
            assert "page" in data
            assert "limit" in data
            assert "total" in data

        if isinstance(items, list) and len(items) > 0:
            app = items[0]
            assert "id" in app
            assert "name" in app
            assert "lifecycle" in app
            assert "created_at" in app
            assert "updated_at" in app

    def test_list_applications_with_pagination(self, client: APIClient):
        """GET /applications with page/limit params."""
        response = client.get("/applications", params={"page": 1, "limit": 10})

        assert response.status_code == 200
        data = response.json()

        assert isinstance(data, dict)
        assert "items" in data
        assert "page" in data and data["page"] == 1
        assert "limit" in data and data["limit"] == 10
        assert isinstance(data.get("items", []), list)
        assert len(data["items"]) <= 10

    def test_list_applications_with_search(self, client: APIClient):
        """GET /applications with search filter should still succeed."""
        response = client.get("/applications", params={"search": "test"})

        assert response.status_code == 200
        data = response.json()
        assert "items" in data or isinstance(data, list)

    def test_create_application(self, client: APIClient):
        """POST /applications should create an application."""
        payload = {
            "name": "Payments Service",
            "lifecycle": "active",
            "owner": "payments-team",
            "tags": ["payments", "pci"],
        }
        response = client.post("/applications", json=payload)

        assert response.status_code in [200, 201]
        data = response.json()

        assert "id" in data
        assert data["name"] == payload["name"]
        assert data["lifecycle"] == payload["lifecycle"]

        # cleanup if supported
        app_id = data.get("id")
        if app_id:
            client.delete(f"/applications/{app_id}")

    def test_create_application_missing_required_field(self, client: APIClient):
        """POST /applications without required 'name' should return 400 or 422."""
        payload = {
            "lifecycle": "active",
        }
        response = client.post("/applications", json=payload)

        assert response.status_code in [400, 422]
        data = response.json()
        assert "code" in data or "error" in data or "errors" in data

    def test_get_application(self, client: APIClient):
        """GET /applications/{id} should return 200."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "Catalog Service",
                "lifecycle": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        get_resp = client.get(f"/applications/{app_id}")
        assert get_resp.status_code == 200
        data = get_resp.json()
        assert data["id"] == app_id
        assert data["name"]
        assert data["lifecycle"]

        client.delete(f"/applications/{app_id}")

    def test_get_nonexistent_application(self, client: APIClient):
        """GET /applications/{id} for unknown id should return 404."""
        response = client.get("/applications/nonexistent-id")
        assert response.status_code == 404

    def test_update_application(self, client: APIClient):
        """PATCH /applications/{id} should update fields."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "Legacy Billing",
                "lifecycle": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        update_payload = {
            "name": "Modern Billing",
            "lifecycle": "sunset",
            "owner": "finance-team",
        }
        update_resp = client.patch(f"/applications/{app_id}", json=update_payload)

        assert update_resp.status_code in [200, 202]
        data = update_resp.json()
        assert data["id"] == app_id
        assert data["name"] == update_payload["name"]
        assert data["lifecycle"] == update_payload["lifecycle"]
        assert data.get("owner") == update_payload["owner"]

        client.delete(f"/applications/{app_id}")

    def test_delete_application(self, client: APIClient):
        """DELETE /applications/{id} should require approval_id and reason."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "Temp App",
                "lifecycle": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        # Test delete without approval_id and reason should fail
        delete_resp = client.delete(f"/applications/{app_id}")
        assert delete_resp.status_code == 400

        # Test delete with approval_id and reason should succeed
        delete_resp = client.delete(
            f"/applications/{app_id}?approval_id=APPR-12345&reason=End+of+life"
        )
        assert delete_resp.status_code in [200, 202, 204]

        # verify gone
        get_resp = client.get(f"/applications/{app_id}")
        assert get_resp.status_code == 404

    def test_set_classification_command(self, client: APIClient):
        """POST /applications/{id}/commands/set-classification should update data classification."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "Customer Portal",
                "lifecycle": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        try:
            # Set classification with reason
            cmd_resp = client.post(
                f"/applications/{app_id}/commands/set-classification",
                json={
                    "classification": "confidential",
                    "reason": "Contains customer PII",
                },
            )
            assert cmd_resp.status_code == 200
            data = cmd_resp.json()
            assert data["data_classification"] == "confidential"

            # Test invalid classification
            invalid_resp = client.post(
                f"/applications/{app_id}/commands/set-classification",
                json={
                    "classification": "invalid",
                    "reason": "Testing",
                },
            )
            assert invalid_resp.status_code == 400

            # Test missing reason
            no_reason_resp = client.post(
                f"/applications/{app_id}/commands/set-classification",
                json={
                    "classification": "public",
                },
            )
            assert no_reason_resp.status_code == 400
        finally:
            client.delete(f"/applications/{app_id}?approval_id=TEST&reason=cleanup")

    def test_transition_lifecycle_command(self, client: APIClient):
        """POST /applications/{id}/commands/transition-lifecycle should validate state machine."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "New Service",
                "lifecycle": "planned",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        try:
            # Valid transition: planned → active
            cmd_resp = client.post(
                f"/applications/{app_id}/commands/transition-lifecycle",
                json={
                    "target_lifecycle": "active",
                },
            )
            assert cmd_resp.status_code == 200
            data = cmd_resp.json()
            assert data["lifecycle"] == "active"

            # Valid transition: active → deprecated
            cmd_resp = client.post(
                f"/applications/{app_id}/commands/transition-lifecycle",
                json={
                    "target_lifecycle": "deprecated",
                    "sunset_date": "2025-12-31",
                },
            )
            assert cmd_resp.status_code == 200
            data = cmd_resp.json()
            assert data["lifecycle"] == "deprecated"

            # Invalid transition: deprecated → planned should fail
            invalid_resp = client.post(
                f"/applications/{app_id}/commands/transition-lifecycle",
                json={
                    "target_lifecycle": "planned",
                },
            )
            assert invalid_resp.status_code == 400
            error_data = invalid_resp.json()
            assert "error" in error_data
        finally:
            client.delete(f"/applications/{app_id}?approval_id=TEST&reason=cleanup")

    def test_set_owner_command(self, client: APIClient):
        """POST /applications/{id}/commands/set-owner should update owner."""
        create_resp = client.post(
            "/applications",
            json={
                "name": "Team App",
                "lifecycle": "active",
            },
        )
        assert create_resp.status_code in [200, 201]
        app_id = create_resp.json()["id"]

        try:
            # Set owner
            cmd_resp = client.post(
                f"/applications/{app_id}/commands/set-owner",
                json={
                    "owner": "platform-team",
                    "reason": "Team restructuring",
                },
            )
            assert cmd_resp.status_code == 200
            data = cmd_resp.json()
            assert data.get("owner") == "platform-team"

            # Set owner without reason (optional)
            cmd_resp = client.post(
                f"/applications/{app_id}/commands/set-owner",
                json={
                    "owner": "devops-team",
                },
            )
            assert cmd_resp.status_code == 200
            data = cmd_resp.json()
            assert data.get("owner") == "devops-team"
        finally:
            client.delete(f"/applications/{app_id}?approval_id=TEST&reason=cleanup")

