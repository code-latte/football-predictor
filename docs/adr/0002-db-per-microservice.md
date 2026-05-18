# ADR 0002: Database per Microservice

## Status
Superseded by ADR-0007 (shared database, shared schema)

## Context
We need to decide whether microservices share a single database or have independent databases.

## Decision
Each microservice will have its **own PostgreSQL database**.  
Shared data will be duplicated via events (projections).

## Consequences
- Services are loosely coupled, can evolve independently.  
- Consistency is eventual (data replicated by events).  
- Slight overhead in maintaining projections.  
- Avoids distributed monolith / shared schema antipattern.
