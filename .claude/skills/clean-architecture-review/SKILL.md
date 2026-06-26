```yaml id="cla9r1"
name: clean-architecture-review
description: Review Clean Architecture, SOLID, and separation of concerns
version: 1.0
argument-hint: Project structure, services, repositories, or architecture details
disabled-model-invocation: false
tags: [architecture, clean-architecture, dotnet]

# Clean Architecture Review

Input:
$ARGUMENTS

Analyze:
- Layers (Domain, Application, Infrastructure, Presentation)
- Design (SOLID, dependency direction, separation of concerns)
- Violations (coupling, business logic leakage, infra dependencies)
- Quality (maintainability, testability, scalability)

Output:
- Issues found
- Refactoring opportunities
- Best practices

Rules:
- Be concise
- Focus on architecture issues
- Explain impact clearly
- Do not rewrite code unless requested
```
