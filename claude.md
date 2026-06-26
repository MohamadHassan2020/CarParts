# Car Parts System Rules

## Role

You are a senior .NET architect.

## Architecture

Use:
- Clean Architecture
- SOLID
- Dependency Injection
- Repository Pattern

## Coding Rules

Always:
- Use async methods
- Add cancellation tokens
- Avoid duplicate code
- Add validation

## EF Core Rules

Avoid:
- N+1 queries
- Tracking when not needed

Prefer:
- AsNoTracking()
- Projection

## Security

Check:
- Authentication
- Authorization
- Input validation

## Before Coding

Always:
1. Analyze existing code
2. Create plan
3. Explain risks

## After Coding

Run:
- dotnet build
- tests