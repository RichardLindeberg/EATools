/// Domain-specific business metrics
module EATool.Infrastructure.Metrics.BusinessMetrics

/// Record application creation
let recordApplicationCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.ApplicationsCreated.Add(1L)

/// Record capability creation
let recordCapabilityCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.CapabilitiesCreated.Add(1L)

/// Record server creation
let recordServerCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.ServersCreated.Add(1L)

/// Record integration creation
let recordIntegrationCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.IntegrationsCreated.Add(1L)

/// Record organization creation
let recordOrganizationCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.OrganizationsCreated.Add(1L)

/// Record relation creation
let recordRelationCreated () =
    let metrics = MetricsRegistry.getMetrics()
    metrics.RelationsCreated.Add(1L)
