# Code Style Guidelines

We enforce consistent coding standards across all languages.

## C# (.NET)
- Use **PascalCase** for classes, methods, and properties.  
- Use **camelCase** for local variables and parameters.  
- Use `record struct` for strongly typed IDs.  
- Organize projects in layers: Domain, Application, Infrastructure, API.  
- Unit tests must follow `[UnitOfWork_StateUnderTest_ExpectedBehavior]` naming convention.  

## TypeScript / Next.js (web & backoffice)
- Use ESLint + Prettier with project rules.  
- App Router (Next.js 14+): folder structure follows the `app/` convention.  
- Prefer **Server Components** by default; only use `'use client'` when strictly necessary (interactivity, browser APIs).  
- Folder by feature inside `app/` and `components/`.  
- Use hooks (`useSomething`) instead of HOCs where possible.  
- Use functional components only.  
- Keep components small and composable.

## TypeScript / React Native (app)
- Use ESLint + Prettier with project rules.  
- Expo managed workflow.  
- Folder by feature inside `screens/` and `components/`.  
- Use functional components and hooks only.

## TypeScript / Common library (`@footballcatch/common`)
- **Domain and Application layers**: zero dependencies on React, Next.js, or React Native.  
- **Infrastructure layer**: may depend on fetch/axios for HTTP and platform-agnostic storage interfaces.  
- Export everything from a single entry point (`src/index.ts`).  
- Versioned with SemVer; breaking changes require a major bump.

## Testing
- NUnit for C#.  
- Jest + React Testing Library for Next.js and React Native.  
- Playwright for E2E on web/backoffice; Detox for E2E on mobile.  
- Tests colocated in `/tests` folder with clear naming.

