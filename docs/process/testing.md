# Testing Strategy

We apply a layered testing approach.

## Unit Tests
- NUnit for .NET domain logic.  
- At least 70% coverage in domain layer.  

## Integration Tests
- Database integration with Testcontainers (PostgreSQL).  
- Event-driven integration with RabbitMQ test exchanges.  

## Contract Tests
- Validate events published/consumed match agreed Contracts.  
- Run in CI against Common Contracts package.  

## End-to-End Tests
- Minimal but critical paths covered (submit prediction → scoring → leaderboard update).  
- Tools: Playwright or Cypress for frontend.  

## Continuous Integration
- All tests run in GitHub Actions.  
- A PR cannot be merged if tests fail.
