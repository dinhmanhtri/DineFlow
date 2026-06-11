# DineFlow — New Feature Rule

> **Activation**: Glob — `src/**/*.cs`
> Áp dụng khi tạo file `.cs` mới trong `src/`.

## Trước khi tạo file mới, xác định layer

Chạy qua decision tree này:

```
Là business rule / domain object thuần?
  ✅ → src/DineFlow.Domain/Entities/ hoặc /Enums/ hoặc /Exceptions/

Là interface (contract)?
  ✅ → src/DineFlow.Application/Interfaces/

Là business logic (orchestrate repos, apply rules)?
  ✅ → src/DineFlow.Application/Services/

Là DTO (data transfer)?
  ✅ → src/DineFlow.Application/DTOs/<FeatureName>/

Là data access hoặc external service?
  ✅ → src/DineFlow.Infrastructure/Repositories/ hoặc /Services/

Là HTTP endpoint?
  ✅ → src/DineFlow.API/Controllers/

Là UI / Razor View?
  ✅ → src/DineFlow.Web/
```

## Checklist khi thêm Entity mới

1. Tạo Entity kế thừa `BaseEntity` trong `Domain/Entities/`
2. Thêm Enum liên quan vào `Domain/Enums/` (nếu cần)
3. Thêm `DbSet<NewEntity>` vào `DineFlowDbContext`
4. Tạo `IEntityTypeConfiguration<NewEntity>` trong `Data/Configurations/`
5. Tạo `INewEntityRepository` trong `Application/Interfaces/Repositories/`
6. Implement `NewEntityRepository : BaseRepository<NewEntity>` trong `Infrastructure/`
7. Thêm property vào `IUnitOfWork` và `UnitOfWork`
8. Tạo DTOs trong `Application/DTOs/NewEntity/`
9. Tạo `NewEntityService` trong `Application/Services/`
10. Đăng ký Service vào DI container
11. Tạo migration: `dotnet ef migrations add Add<NewEntity>Table`

## Checklist khi thêm API Endpoint mới

1. Tạo hoặc thêm vào Controller trong `API/Controllers/`
2. Inject Service interface (không inject Repository trực tiếp)
3. Validate input với DataAnnotations trên Request DTO
4. Trả đúng HTTP status code
5. Domain exceptions tự động map sang HTTP codes qua GlobalErrorHandler
6. Thêm `[Authorize]` / `[Authorize(Roles = "Admin")]` đúng chỗ
7. Swagger tự gen từ attributes — kiểm tra Swagger UI sau khi thêm
