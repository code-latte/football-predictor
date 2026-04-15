# @footballcatch/common

**Shared npm library** for all frontend applications.  
Implements **clean architecture** (Domain → Application → Infrastructure) with no framework dependencies.

---

## What it contains

### 1. Domain
- Entities and value objects (e.g. `Prediction`, `League`, `UserId`)
- Repository and service interfaces (ports)
- Pure business rules, no side effects

### 2. Application
- Use cases (e.g. `SubmitPredictionUseCase`, `GetLeaderboardUseCase`)
- Input/output DTOs
- Orchestration between domain and infrastructure ports

### 3. Infrastructure
- Concrete implementations of the ports: HTTP clients towards REST microservices
- Local storage adapters (AsyncStorage, SecureStore)
- Only instantiated in the apps, never in domain or application

---

## What should **NOT** go here

❌ React or React Native components  
❌ Routing or navigation logic  
❌ Visual styles or themes  
❌ App-specific environment variables  

---

## Organisation

```
/frontend/common/
  src/
    domain/          # Entities, value objects, interfaces (ports)
    application/     # Use cases and DTOs
    infrastructure/  # HTTP clients, storage adapters
  package.json       # Published as @footballcatch/common
```

---

## Dependencies

- Consumed by `web`, `backoffice`, and `app` via npm workspace
- Versioned with SemVer; breaking changes = major bump
- No React, Next.js, or React Native dependencies in `domain` and `application`
