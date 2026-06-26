```yaml id="efc9r1"
name: efcore-review
description: Review EF Core code for performance, security, and best practices
version: 1.0
argument-hint: DbContext, entities, queries, migrations, or EF Core code
disabled-model-invocation: false
tags: [efcore, dotnet, review]

# EF Core Review

Input:
$ARGUMENTS

Analyze:
- Architecture (entities, DbContext, relationships)
- Performance (N+1, tracking, includes, queries)
- Security (SQL injection, data exposure, validation)
- Code Quality (naming, maintainability, SOLID)

Output:
- Issues found
- Recommendations
- Best practices

Rules:
- Be concise
- Prioritize critical issues
- Explain clearly
- Do not rewrite code unless requested
```
