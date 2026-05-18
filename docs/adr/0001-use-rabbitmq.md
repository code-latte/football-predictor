# ADR 0001: Use RabbitMQ as Event Bus

## Status
Superseded by ADR-0006 (in-process dispatcher)

## Context
We need asynchronous communication between microservices. Options considered: RabbitMQ, Kafka, Azure Service Bus.

## Decision
We will use **RabbitMQ** as the event bus because:  
- Lightweight and easy to run on a VPS.  
- Excellent .NET client support.  
- Matches our needs (message durability, pub/sub, topic exchanges).  
- Lower operational complexity than Kafka.

## Consequences
- All services must include RabbitMQ client.  
- Need to manage message versioning and idempotency.  
- Future migration to Kafka is possible if event throughput grows significantly.
