# Item-002: Add Integration Tests for Organization Hierarchy

**Status:** 🔴 Blocked  
**Priority:** P0 - CRITICAL  
**Effort:** 2-3 hours  
**Created:** 2026-01-06  
**Owner:** TBD

---

## Problem Statement

No test coverage for the documented hierarchical organization feature. Tests would currently fail because the feature is not implemented (Item-001).

---

## Affected Files

- [tests/integration/test_organizations.py](../../tests/integration/test_organizations.py)

---

## Detailed Tasks

- [ ] Test creating organization without parent (root)
- [ ] Test creating organization with valid parent_id
- [ ] Test creating multi-level hierarchy (grandparent → parent → child)
- [ ] Test updating parent_id (moving org in hierarchy)
- [ ] Test querying organizations by parent_id
- [ ] Test filtering for root organizations (parent_id=null)
- [ ] Test circular reference prevention (child cannot be its own ancestor)
- [ ] Test ancestor validation (prevent circular chains)
- [ ] Test non-existent parent rejection
- [ ] Test deleting parent organization (cascade behavior or constraint)
- [ ] Test orphan handling

---

## Acceptance Criteria

- [ ] All hierarchy tests pass
- [ ] Edge cases covered (null parent, circular refs, orphans)
- [ ] Test data cleanup works correctly
- [ ] Tests cover all validation rules
- [ ] Tests document expected behavior

---

## Testing Strategy

**Test File:** tests/integration/test_organizations.py

**Test Cases:**

```python
def test_create_root_organization():
    """Create organization without parent."""
    payload = {"name": "Root Org", "parent_id": None}
    response = client.post("/organizations", json=payload)
    assert response.status_code == 201

def test_create_child_organization():
    """Create organization with parent."""
    # Create parent first
    parent = client.post("/organizations", json={"name": "Parent"}).json()
    # Create child
    child_payload = {"name": "Child", "parent_id": parent["id"]}
    response = client.post("/organizations", json=child_payload)
    assert response.status_code == 201
    assert response.json()["parent_id"] == parent["id"]

def test_prevent_circular_reference():
    """Cannot make child its own ancestor."""
    org = client.post("/organizations", json={"name": "Org"}).json()
    payload = {"parent_id": org["id"]}
    response = client.patch(f"/organizations/{org['id']}", json=payload)
    assert response.status_code == 422

def test_query_by_parent_id():
    """Filter organizations by parent_id."""
    parent = client.post("/organizations", json={"name": "Parent"}).json()
    client.post("/organizations", json={"name": "Child", "parent_id": parent["id"]})
    
    response = client.get("/organizations", params={"parent_id": parent["id"]})
    assert response.status_code == 200
    children = response.json()["items"]
    assert len(children) >= 1
    assert all(c["parent_id"] == parent["id"] for c in children)
```

---

## Dependencies

**Blocks:** None

**Depends On:** 
- Item-001: Organization parent_id implementation

---

## Related Items

- [Item-001-Prio-P0.md](Item-001-Prio-P0.md) - Must complete first
- [tests/integration/test_organizations.py](../../tests/integration/test_organizations.py)

---

## Definition of Done

- [x] All test cases implemented
- [x] Tests pass with Item-001 implemented
- [x] Edge cases covered
- [x] Test cleanup works
- [x] Code reviewed

---

## Notes

- Currently test file has basic organization tests but no hierarchy tests
- Should follow existing test patterns in test_organizations.py
- Use client fixture from conftest.py
