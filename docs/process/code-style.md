# Code Style Guidelines

We enforce consistent coding standards across all languages.

## C# (.NET)
- Use **PascalCase** for classes, methods, and properties.  
- Use **camelCase** for local variables and parameters.  
- Use `record struct` for strongly typed IDs.  
- Organize projects in layers: Domain, Application, Infrastructure, API.  
- Unit tests must follow `[UnitOfWork_StateUnderTest_ExpectedBehavior]` naming convention.  

## TypeScript / React
- Use ESLint + Prettier with project rules.  
- Folder by feature.  
- Use hooks (`useSomething`) instead of HOCs where possible.  
- Use functional components only.  
- Keep components small and composable.

## Testing
- NUnit for C#.  
- Jest/React Testing Library for TS/React.  
- Tests colocated in `/tests` folder with clear naming.

