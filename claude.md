# Car Parts System — Claude Instructions

## Role

Senior .NET architect working on a **single-project ASP.NET Core 10 Razor Pages** application.
Stack: .NET 10 · Razor Pages · EF Core 10 · SQLite · Cookie Auth · xUnit.

---

## Project Layout

```
src/CarParts.Web/
├── Data/           AppDbContext — single DbSet<Part>
├── Migrations/     EF Core migrations (never edit manually)
├── Models/         Part (entity), PartInputModel (input DTO), ServiceResult (result type)
├── Repositories/   IPartRepository / PartRepository — data access only
├── Services/       IPartService / PartService — business rules
├── Settings/       AdminSettings — bound from appsettings.json
└── Pages/
    ├── Account/    Login, Logout
    └── Parts/      Index, Create, Edit, Delete, Details

tests/CarParts.Tests/
└── PartPageTests.cs — xUnit tests using EF InMemory
```

Request flow: `HTTP → Razor Page → IPartService → IPartRepository → AppDbContext → SQLite`

---

## Architecture Rules

- Keep the layer boundary: Pages call Services, Services call Repositories, Repositories call DbContext.
- Pages must never reference `AppDbContext` or `IPartRepository` directly.
- Add new entities in the same pattern: entity model → input model → repository interface → repository → service interface → service → pages.

---

## Established Conventions — Follow These Exactly

### ServiceResult
All service write methods return `ServiceResult`. Never throw exceptions for business-rule failures.
```csharp
return ServiceResult.Fail("Part number already exists.");
return ServiceResult.Ok();
```

### Input/Entity separation
User-submitted data goes through an `*InputModel` (DataAnnotations validation).
Never bind request data directly to the entity class.

### Concurrency
Edit operations pass `Guid rowVersion` from the form to `UpdateAsync`.
`PartRepository.SetConcurrencyToken` wires the original value before save.
Always catch `DbUpdateConcurrencyException` in service update methods.

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
- Never call `SaveChangesAsync` from a Page — only from Repository methods or `CommitAsync`.

---

## Security Rules

- `/Parts/**` pages are protected by `AuthorizeFolder` in `Program.cs` — do not remove this.
- Login uses `CryptographicOperations.FixedTimeEquals` to prevent timing attacks — preserve this.
- `AdminSettings.Password` is plain text in config; **do not store new secrets in plain text**. Flag this as a known debt item if touched.
- Always use `LocalRedirect` (not `Redirect`) for post-login redirects to prevent open-redirect attacks.
- Validate and sanitise all user input via DataAnnotations on the InputModel before it reaches the service.
- Anti-forgery tokens are enabled by default in Razor Pages — never disable them.

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

## Known Debt (do not re-introduce, fix if you touch the area)

- `AdminSettings.Password` is plain text — should be a PBKDF2/BCrypt hash stored in the config.
- `IPartRepository.CommitAsync` leaks Unit-of-Work concern — prefer self-contained repository methods.
- `DeleteModel.OnPostAsync` does not handle a failed `ServiceResult` — always check the result.
- No structured logging (`ILogger<T>`) in services or repositories.
- SQLite is dev-only — production should target SQL Server or PostgreSQL.
