# @footballcatch/common

Librería **npm compartida** para todas las aplicaciones frontend.  
Implementa **clean architecture** (Domain → Application → Infrastructure) sin dependencias de framework.

---

## Qué contiene

### 1. Domain
- Entidades y value objects (ej. `Prediction`, `League`, `UserId`)
- Interfaces de repositorios y servicios (ports)
- Reglas de negocio puras, sin efectos secundarios

### 2. Application
- Use cases (ej. `SubmitPredictionUseCase`, `GetLeaderboardUseCase`)
- DTOs de entrada/salida
- Orchestración entre domain y puertos de infraestructura

### 3. Infrastructure
- Implementaciones concretas de los puertos: clientes HTTP hacia los microservicios REST
- Adaptadores de almacenamiento local (AsyncStorage, SecureStore)
- Solo se instancia en las apps, nunca en domain ni application

---

## Qué **NO** debe ir aquí

❌ Componentes React o React Native  
❌ Lógica de routing o navegación  
❌ Estilos o temas visuales  
❌ Variables de entorno específicas de una app  

---

## Organización

```
/frontend/common/
  src/
    domain/          # Entidades, value objects, interfaces (ports)
    application/     # Use cases y DTOs
    infrastructure/  # Clientes HTTP, adaptadores de storage
  package.json       # Publicado como @footballcatch/common
```

---

## Dependencias

- Consumido por `web`, `backoffice` y `app` vía npm workspace
- Versionado con SemVer; cambios breaking = major bump
- Sin dependencias de React, Next.js ni React Native en `domain` y `application`
