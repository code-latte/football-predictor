# Observability

## Logging
- **Serilog** structured logging.  
- Correlation IDs for tracing across services.  
- Log levels: Info, Warning, Error.  

## Metrics
- Each service exposes `/metrics` endpoint.  
- Default metrics: HTTP requests, DB queries, RabbitMQ consumers.  
- Custom metrics: domain-specific (predictions submitted, leagues created).  

## Tracing
- Correlation IDs propagated through HTTP headers and RabbitMQ message properties.  
- Optional OpenTelemetry integration in the future.  

## Dashboards
- Grafana dashboards for:  
  - Service health.  
  - Requests/sec.  
  - Event throughput.  
  - DB performance.  

---
