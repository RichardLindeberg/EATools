import pytest
from conftest import APIClient


@pytest.mark.integration
class TestRelationValidation:
    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "app-b",
                "source_type": "application",
                "target_type": "application",
                "relation_type": rel,
            }
            for rel in [
                "depends_on",
                "communicates_with",
                "calls",
            ]
        ],
    )
    def test_allowed_application_to_application(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "svc-a",
                "source_type": "application",
                "target_type": "application_service",
                "relation_type": rel,
            }
            for rel in ["realizes", "uses"]
        ],
    )
    def test_allowed_application_to_service(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "intf-a",
                "source_type": "application",
                "target_type": "application_interface",
                "relation_type": "exposes",
            },
            {
                "source_id": "intf-a",
                "target_id": "svc-a",
                "source_type": "application_interface",
                "target_type": "application_service",
                "relation_type": "serves",
            },
        ],
    )
    def test_allowed_interface_pairs(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "srv-a",
                "source_type": "application",
                "target_type": "server",
                "relation_type": rel,
            }
            for rel in ["deployed_on", "stores_data_on"]
        ]
        + [
            {
                "source_id": "srv-a",
                "target_id": "srv-b",
                "source_type": "server",
                "target_type": "server",
                "relation_type": "connected_to",
            }
        ],
    )
    def test_allowed_infrastructure(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "data-a",
                "source_type": "application",
                "target_type": "data_entity",
                "relation_type": rel,
            }
            for rel in ["reads", "writes"]
        ],
    )
    def test_allowed_data_access(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "cap-a",
                "source_type": "application",
                "target_type": "business_capability",
                "relation_type": "supports",
            }
        ]
        + [
            {
                "source_id": "svc-a",
                "target_id": "cap-a",
                "source_type": "application_service",
                "target_type": "business_capability",
                "relation_type": rel,
            }
            for rel in ["realizes", "supports"]
        ],
    )
    def test_allowed_business_support(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "org-a",
                "target_id": "app-a",
                "source_type": "organization",
                "target_type": "application",
                "relation_type": "owns",
            },
            {
                "source_id": "org-a",
                "target_id": "srv-a",
                "source_type": "organization",
                "target_type": "server",
                "relation_type": "owns",
            },
        ],
    )
    def test_allowed_ownership(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (200, 201)
        rel_id = resp.json().get("id")
        if rel_id:
            client.post(f"/relations/{rel_id}/commands/delete", json={})

    @pytest.mark.parametrize(
        "payload",
        [
            {
                "source_id": "app-a",
                "target_id": "srv-a",
                "source_type": "application",
                "target_type": "server",
                "relation_type": "reads",
            },
            {
                "source_id": "server-a",
                "target_id": "app-a",
                "source_type": "server",
                "target_type": "application",
                "relation_type": "depends_on",
            },
            {
                "source_id": "data-a",
                "target_id": "app-a",
                "source_type": "data_entity",
                "target_type": "application",
                "relation_type": "writes",
            },
            {
                "source_id": "application",
                "target_id": "application",
                "source_type": "application_interface",
                "target_type": "application_interface",
                "relation_type": "serves",
            },
        ],
    )
    def test_invalid_combinations(self, client: APIClient, payload):
        resp = client.post("/relations", json=payload)
        assert resp.status_code in (400, 422)
        body = resp.json()
        assert "validation_error" in str(body).lower()
