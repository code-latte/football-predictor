# Frontend de FootballCatch

Aquí viven las tres aplicaciones cliente y la librería compartida.

---

## Convenciones de proyecto

Cada aplicación debe cumplir:

- **Estructura de carpetas**

  ```
  /src/      # Código de la aplicación
  /tests/    # Tests unitarios y de integración
  package.json
  ```

- **Tecnologías**

  - **web** y **backoffice**: Next.js 14+ (App Router)
  - **app**: React Native (Expo)
  - **common**: TypeScript puro, sin dependencias de framework en domain y application

- **Testing**: Jest + React Testing Library; E2E con Playwright (web/backoffice) o Detox (app)
- **Calidad**: ESLint + Prettier con reglas del proyecto
- **Arquitectura**: Lógica de negocio y acceso a datos en `common`; las apps solo consumen los use cases expuestos

---

## Aplicaciones

- `web` → Next.js web app (predicciones, rankings, perfil)
- `backoffice` → Next.js admin (competiciones, reglas, moderación)
- `app` → React Native mobile (predicciones y notificaciones push)
- `common` → Librería npm compartida (`@footballcatch/common`) con clean architecture
