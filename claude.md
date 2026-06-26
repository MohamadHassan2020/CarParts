# Car Parts System — Claude Instructions

## Role

Senior .NET architect working on a **single-project ASP.NET Core 10 Razor Pages** application.
Stack: .NET 10 · Razor Pages · EF Core 10 · SQLite (dev) / SQL Server (prod) · Cookie Auth · xUnit.

---

## Project Layout

```
src/CarParts.Web/
├── Data/           AppDbContext — single DbSet<Part>
├── Migrations/     EF Core migrations (never edit manually)
├── Models/         Part (entity), PartInputModel (input DTO), ServiceResult (result type)
├── Repositories/   IPartRepository / PartRepository — data access only
├── Services/       IPartService / PartService — business rules + ILogger
├── Settings/       AdminSettings — bound from appsettings.json (Username, PasswordHash)
└── Pages/
    ├── Account/    Login, Logout
    └── Parts/      Index, Create, Edit, Delete, Details

tests/CarParts.Tests/
└── PartPageTests.cs — xUnit tests using EF InMemory + NullLogger
```

Request flow: `HTTP → Razor Page → IPartService → IPartRepository → AppDbContext → SQLite/SQL Server`

---

## Architecture Rules

- Keep the layer boundary: Pages call Services, Services call Repositories, Repositories call DbContext.
- Pages must never reference `AppDbContext` or `IPartRepository` directly.
- Add new entities in the same pattern: entity model → input model → repository interface → repository → service interface → service → pages.

---

## Established Conventions — Follow These Exactly

### ServiceResult
All service write methods return `ServiceResult`. Never throw exceptions for business-rule failures.
Always check the result in the calling page and handle the failure case.
```csharp
// service
return ServiceResult.Fail("Part number already exists.");
return ServiceResult.Ok();

// page
var result = await service.DeleteAsync(id, ct);
if (!result.Success) return NotFound();
```

### Input/Entity separation
User-submitted data goes through an `*InputModel` (DataAnnotations validation).
Never bind request data directly to the entity class.

### Concurrency
Edit operations pass `Guid rowVersion` from the form to `UpdateAsync`.
`IPartRepository.UpdateAsync(part, rowVersion, ct)` owns the concurrency-token wiring and the save internally — callers do not call `SaveChangesAsync` directly.
Always catch `DbUpdateConcurrencyException` in service update methods.

### Logging
`ILogger<T>` is injected into both `PartService` and `PartRepository`.
- Service: `LogInformation` on success, `LogWarning` on business-rule failures and concurrency conflicts.
- Repository: `LogDebug` on each mutating operation.
When adding new service methods, follow the same pattern.

### AsNoTracking
Use `.AsNoTracking()` for every query that does not need change tracking (list queries, existence checks).
`FindAsync` is used for tracked fetch-for-update in `GetByIdAsync`.

### CancellationToken
Every async method must accept `CancellationToken ct = default` and pass it to all awaited calls.

### Async
All methods that touch I/O must be `async Task<T>`. No `.Result` or `.Wait()`.

---

## EF Core Rules

- No N+1: eager-load or project in a single query.
- Prefer `AsNoTracking()` for reads.
- Prefer projection (`Select`) over loading full entities when only a subset of fields is needed.
- Never call `SaveChangesAsync` from a Page — only from Repository methods.

---

## Security Rules

- `/Parts/**` pages are protected by `AuthorizeFolder` in `Program.cs` — do not remove this.
- Admin password is stored as a PBKDF2-SHA256 hash (`Admin.PasswordHash` in `appsettings.json`).
  **Never store plain-text secrets in config.** If the password needs to change, generate a new hash:
  ```powershell
  $salt = New-Object byte[] 16
  [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($salt)
  $pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes("NewPassword", $salt, 100000, "SHA256")
  "$([Convert]::ToBase64String($salt)).$([Convert]::ToBase64String($pbkdf2.GetBytes(32)))"
  ```
- Login uses `CryptographicOperations.FixedTimeEquals` for timing-safe comparison — preserve this.
- Always use `LocalRedirect` (not `Redirect`) for post-login redirects to prevent open-redirect attacks.
- Validate all user input via DataAnnotations on the InputModel before it reaches the service.
- Anti-forgery tokens are enabled by default in Razor Pages — never disable them.

---

## Database / Provider

`DatabaseProvider` in `appsettings.json` selects the EF Core provider at startup:
- `"Sqlite"` (default) — for local development, uses `Data Source=carparts.db`.
- `"SqlServer"` — for production; set `ConnectionStrings:DefaultConnection` to the SQL Server connection string.

Migrations are SQLite-based. If switching to SQL Server in production, new migrations are required.

`SQLitePCLRaw.lib.e_sqlite3` 2.1.11 has a known vulnerability (GHSA-2m69-gcr7-jv3q) with no upstream
fix yet. It is suppressed via `<NuGetAuditSuppress>` in both project files. Re-evaluate when a new
SQLitePCLRaw version is published.

---

## Git Workflow

- Always work on a **feature branch**, not directly on `master`.
- Open a PR and merge via GitHub — do not push directly to `master`.

---

## Before Coding

1. Read the relevant existing files (entity, repository, service, page) before writing anything.
2. State your plan and any risks (breaking changes, security implications, DB schema changes).
3. If a DB migration is needed, flag it explicitly before proceeding.

---

## After Coding

Run these commands and fix all errors before reporting done:

```powershell
# From repo root
dotnet build src/CarParts.Web/CarParts.Web.csproj
dotnet test tests/CarParts.Tests/CarParts.Tests.csproj
```

If you add a new EF Core migration:
```powershell
dotnet ef migrations add <MigrationName> --project src/CarParts.Web --startup-project src/CarParts.Web
```

---

## Known Debt

- No search or filter on the Parts list — only pagination exists.
- No role-based authorization — any authenticated user has full write access.
- `PartService.GetPagedAsync` and `GetByIdAsync` are pass-throughs to the repository with no service logic.
- SQLitePCLRaw GHSA-2m69-gcr7-jv3q: suppressed, monitor for upstream fix.
