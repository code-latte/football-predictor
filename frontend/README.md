# FootballCatch Frontend

This is where the three client applications and the shared library live.

---

## Project conventions

Each application must comply with:

- **Folder structure**

  ```
  /src/      # Application code
  /tests/    # Unit and integration tests
  package.json
  ```

- **Technologies**

  - **web** and **backoffice**: Next.js 14+ (App Router)
  - **app**: React Native (Expo)
  - **common**: Pure TypeScript, no framework dependencies in domain and application

- **Testing**: Jest + React Testing Library; E2E with Playwright (web/backoffice) or Detox (app)
- **Quality**: ESLint + Prettier with project rules
- **Architecture**: Business logic and data access in `common`; apps only consume exposed use cases

---

## Applications

- `web` → Next.js web app (predictions, rankings, profile)
- `backoffice` → Next.js admin (competitions, rules, moderation)
- `app` → React Native mobile (predictions and push notifications)
- `common` → Shared npm library (`@footballcatch/common`) with clean architecture
