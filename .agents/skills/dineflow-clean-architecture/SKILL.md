---
name: dineflow-clean-architecture
description: Enforces Clean Architecture conventions for the DineFlow ASP.NET Core project. Use when adding new features, creating new files, deciding where code belongs, or reviewing layer dependencies. Applies to all code in src/ folder.
---

# DineFlow Clean Architecture Skill

This skill enforces the **architectural rules and coding conventions** for DineFlow. Read this before adding any new class, service, or file.

## Layer Rules (The Dependency Rule)

```
Domain ← Application ← Infrastructure ← API
                                       ← Web
```

**Direction of allowed references:**
- `Domain` → imports NOTHING (pure C#, no framework dependencies)
- `Application` → imports only `Domain`
- `Infrastructure` → imports `Application` (implements its interfaces)
- `API` → imports `Infrastructure` (for DI registration only)
- `Web` → imports `Application` (calls services via DI)

**❌ NEVER:**
- Import `Infrastructure` from `Domain` or `Application`
- Import `API` or `Web` from any other layer
- Reference `Microsoft.EntityFrameworkCore` in `Application` or `Domain`

## Where Does Each Thing Go?

### Domain (`src/DineFlow.Domain/`)
| What | Where |
|---|---|
| Entity classes | `Entities/` |
| Enum types | `Enums/` |
| Custom exceptions | `Exceptions/` |
| Value objects | `ValueObjects/` |
| Abstract base classes | `Common/` |

**Rules:**
- Entities inherit `BaseEntity` (Guid Id, CreatedAt, UpdatedAt)
- Entities may contain business logic methods (Rich Domain Model)
- Use `decimal` for all monetary values (never `double`/`float`)
- Use `null!` for required navigation properties (suppress CS8618)
- Navigation properties: collections initialize with `new List<T>()`

### Application (`src/DineFlow.Application/`)
| What | Where |
|---|---|
| Repository interfaces | `Interfaces/Repositories/` |
| Service interfaces | `Interfaces/Services/` |
| Unit of Work interface | `Interfaces/IUnitOfWork.cs` |
| Service implementations | `Services/` |
| DTO classes | `DTOs/<Feature>/` |
| AutoMapper profiles | `Mappings/` |

**Rules:**
- Interfaces only — no EF Core, no Redis, no HTTP
- Services receive `IUnitOfWork` and `ICacheService` via constructor DI
- Services throw domain exceptions (`NotFoundException`, not `Exception`)
- DTOs use `record` types (immutable, value equality)
- Input DTOs: `CreateXxxRequest`, `UpdateXxxRequest`
- Output DTOs: `XxxDto`, `XxxSummaryDto`

### Infrastructure (`src/DineFlow.Infrastructure/`)
| What | Where |
|---|---|
| DbContext | `Data/DineFlowDbContext.cs` |
| Fluent API configs | `Data/Configurations/` |
| Repository implementations | `Repositories/` |
| Unit of Work impl | `UnitOfWork/UnitOfWork.cs` |
| External service impls | `Services/` |
| DI registration | `InfrastructureServiceExtensions.cs` |

**Rules:**
- Always use `AsNoTracking()` for read-only queries
- Always use `Include()` (Eager Loading) — never rely on Lazy Loading
- `decimal` columns need `HasColumnType("decimal(18,2)")`
- Enum stored as `string`: `.HasConversion<string>().HasMaxLength(20)`
- Unique constraints: `.HasIndex(x => x.Field).IsUnique()`

### API (`src/DineFlow.API/`)
| What | Where |
|---|---|
| REST Controllers | `Controllers/` |
| JWT Auth setup | `Program.cs` |
| Middleware | `Middleware/` |
| Swagger config | `Program.cs` |

**Controller conventions:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    // GET    → Ok(data) 200
    // POST   → CreatedAtAction() 201
    // PUT    → NoContent() 204
    // DELETE → NoContent() 204
    // Not Found → throw NotFoundException (mapped to 404 by middleware)
}
```

## Naming Conventions

| Type | Convention | Example |
|---|---|---|
| Entity | PascalCase, singular | `MenuItem`, `DiningTable` |
| Interface | `I` prefix | `IMenuRepository`, `ICacheService` |
| Repository | `<Entity>Repository` | `MenuRepository` |
| Service | `<Feature>Service` | `MenuService`, `OrderService` |
| Controller | `<Resource>Controller` (plural) | `MenuController`, `TablesController` |
| DTO (output) | `<Entity>Dto` | `MenuItemDto` |
| DTO (input) | `Create/Update<Entity>Request` | `CreateMenuItemRequest` |
| AutoMapper | `<Feature>MappingProfile` | `MenuMappingProfile` |

## Cache Keys Convention

```
menu:all                   → all menu items
menu:category:{id}         → items by category
menu:item:{id}             → single item
table:status:all           → all tables with status
dashboard:summary:{date}   → daily KPI summary
```

**TTL guidelines:**
- Menu data → 10 minutes
- Table status → 30 seconds (near-realtime)
- Dashboard → 5 minutes
- Auth tokens → match JWT expiry

## Decision Tree: Where to Put New Code?

```
Is it a pure business rule or domain object?
  YES → Domain/Entities or Domain/Exceptions
  NO  ↓
Is it an interface (contract without implementation)?
  YES → Application/Interfaces
  NO  ↓
Is it business logic (orchestrating repos, applying rules)?
  YES → Application/Services
  NO  ↓
Is it data access or external service (EF, Redis, BCrypt)?
  YES → Infrastructure/Repositories or Infrastructure/Services
  NO  ↓
Is it HTTP handling (request/response, routing)?
  YES → API/Controllers or Web/Controllers
```

## Code Quality Checklist

Before committing any new code, verify:

- [ ] No cross-layer violations (check using project references)
- [ ] `decimal` used for all money fields (not `double`)
- [ ] Navigation properties use `null!` or initialized collection
- [ ] Repository methods use `AsNoTracking()` for reads
- [ ] Service throws domain exceptions (not raw `Exception`)
- [ ] New entities registered in `DineFlowDbContext.DbSet<>`
- [ ] Fluent API configuration added for new entities
- [ ] DI registered in `InfrastructureServiceExtensions.cs`
