```yaml id="sec9r1"
name: aspnetcore-security
description: Review ASP.NET Core apps for security issues and best practices
version: 1.0
argument-hint: Controllers, Razor Pages, Middleware, Auth, Authorization, Program.cs
disabled-model-invocation: false
tags: [aspnetcore, security, dotnet]

# ASP.NET Core Security Review

Input:
$ARGUMENTS

Analyze:
- Authentication (Identity/JWT, passwords, sessions)
- Authorization (roles, policies, access control)
- Vulnerabilities (XSS, CSRF, SQLi, redirects, data exposure)
- Security Headers (HTTPS, HSTS, cookies)

Output:
- Risks found
- Severity (High/Medium/Low)
- Mitigations

Rules:
- Be concise
- Focus on critical issues
- Explain risks clearly
- Do not modify code unless requested
```
