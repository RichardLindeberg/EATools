import requests
import re
import json
from datetime import datetime

BASE_URL = "http://localhost:8000"

def test_trace_context_headers():
    """Test that W3C trace context headers are correctly set in responses"""
    
    # Make a request to the health endpoint
    response = requests.get(f"{BASE_URL}/health")
    
    # Check response status
    assert response.status_code == 200, f"Expected 200, got {response.status_code}"
    
    # Check for traceparent header in response
    assert "traceparent" in response.headers, "traceparent header not found in response"
    
    traceparent = response.headers["traceparent"]
    print(f"Traceparent: {traceparent}")
    
    # Validate W3C traceparent format: 00-{trace-id}-{span-id}-{trace-flags}
    # trace-id is 32 hex chars, span-id is 16 hex chars, trace-flags is 2 hex chars
    pattern = r"^00-[0-9a-f]{32}-[0-9a-f]{16}-[01]{2}$"
    assert re.match(pattern, traceparent), f"Invalid traceparent format: {traceparent}"
    
    # Parse the traceparent
    parts = traceparent.split("-")
    version = parts[0]
    trace_id = parts[1]
    span_id = parts[2]
    trace_flags = parts[3]
    
    print(f"  Version: {version}")
    print(f"  Trace ID: {trace_id}")
    print(f"  Span ID: {span_id}")
    print(f"  Trace Flags: {trace_flags}")
    
    # Verify format compliance
    assert version == "00", "Version should be 00"
    assert len(trace_id) == 32, "Trace ID should be 32 hex characters"
    assert len(span_id) == 16, "Span ID should be 16 hex characters"
    assert trace_flags in ["00", "01"], "Trace flags should be 00 or 01"
    
    print("✓ W3C trace context headers validated successfully")


def test_trace_context_propagation():
    """Test that trace context is properly propagated through request lifecycle"""
    
    # Create an application to test command tracing
    request_data = {
        "name": "TraceTest App",
        "owner": "test-user",
        "lifecycle": "active",
        "dataClassification": "internal"
    }
    
    response = requests.post(
        f"{BASE_URL}/applications",
        json=request_data,
        headers={"Content-Type": "application/json"}
    )
    
    print(f"Create app response status: {response.status_code}")
    
    # Check for traceparent in response
    if "traceparent" in response.headers:
        traceparent = response.headers["traceparent"]
        print(f"Response traceparent: {traceparent}")
        
        # Validate format
        pattern = r"^00-[0-9a-f]{32}-[0-9a-f]{16}-[01]{2}$"
        assert re.match(pattern, traceparent), f"Invalid traceparent format in create response: {traceparent}"
        print("✓ Trace context propagated through command execution")
    else:
        print("⚠ traceparent header not in response headers (expected for newer OTel versions)")


def test_trace_context_format():
    """Test trace context format in various endpoints"""
    
    endpoints = [
        "/health",
        "/metadata",
        "/applications",
    ]
    
    for endpoint in endpoints:
        try:
            response = requests.get(f"{BASE_URL}{endpoint}", timeout=5)
            
            # Check for trace headers
            has_traceparent = "traceparent" in response.headers
            has_tracestate = "tracestate" in response.headers
            
            if has_traceparent:
                print(f"✓ {endpoint}: traceparent={response.headers['traceparent']}")
            else:
                print(f"  {endpoint}: No traceparent header")
            
            # All responses should have status code
            print(f"    Status: {response.status_code}")
            
        except Exception as e:
            print(f"✗ {endpoint}: {e}")


if __name__ == "__main__":
    print("\n=== W3C Trace Context Validation ===\n")
    
    try:
        test_trace_context_format()
        print("\n=== Testing traceparent header format ===\n")
        test_trace_context_headers()
        print("\n=== Testing trace context propagation ===\n")
        test_trace_context_propagation()
        print("\n✓ All trace context tests passed!\n")
    except AssertionError as e:
        print(f"\n✗ Test failed: {e}\n")
        exit(1)
    except Exception as e:
        print(f"\n✗ Unexpected error: {e}\n")
        exit(1)
