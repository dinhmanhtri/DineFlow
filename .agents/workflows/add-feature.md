# Add Feature

**Description**: Thêm một tính năng mới vào DineFlow theo đúng Clean Architecture — từ Domain đến API endpoint, đầy đủ các layer.

Invoke với: `/add-feature`

---

## Step 1 — Thu thập Yêu Cầu

Hỏi user (nếu chưa rõ):
1. Tên tính năng là gì? (ví dụ: "Quản lý khuyến mãi", "Đặt bàn trước")
2. Tính năng này cần entity mới không?
3. Cần API endpoints gì? (CRUD hay chỉ một số)
4. Ai có quyền truy cập? (All / Admin only / Waiter only...)

## Step 2 — Đọc Architecture Rules

Đọc `.agents/skills/dineflow-clean-architecture/SKILL.md` để xác định layer placement.

## Step 3 — Domain Layer (nếu cần entity mới)

**3a. Tạo Entity**
```
src/DineFlow.Domain/Entities/<FeatureName>.cs
```
- Kế thừa `BaseEntity`
- Thêm navigation properties
- Thêm domain logic methods nếu cần

**3b. Thêm Enums (nếu cần)**
```
src/DineFlow.Domain/Enums/<FeatureStatus>.cs
```

**3c. Đăng ký trong DbContext**
Thêm `DbSet<NewEntity>` vào `DineFlowDbContext`

**3d. Tạo Fluent API Configuration**
```
src/DineFlow.Infrastructure/Data/Configurations/EntityConfigurations.cs
```
Thêm class mới implement `IEntityTypeConfiguration<NewEntity>`

## Step 4 — Application Layer

**4a. Repository Interface**
```
src/DineFlow.Application/Interfaces/Repositories/I<Feature>Repository.cs
```

**4b. DTOs**
```
src/DineFlow.Application/DTOs/<Feature>/
  <Feature>Dto.cs         ← output
  Create<Feature>Request.cs ← input
  Update<Feature>Request.cs ← input (nếu cần)
```

**4c. Service Interface + Implementation**
```
src/DineFlow.Application/Interfaces/Services/I<Feature>Service.cs
src/DineFlow.Application/Services/<Feature>Service.cs
```

**4d. AutoMapper Profile**
```
src/DineFlow.Application/Mappings/<Feature>MappingProfile.cs
```

## Step 5 — Infrastructure Layer

**5a. Repository Implementation**
```
src/DineFlow.Infrastructure/Repositories/<Feature>Repository.cs
```
Kế thừa `BaseRepository<Entity>`, implement `I<Feature>Repository`

**5b. Thêm vào UnitOfWork**
- Interface `IUnitOfWork`: thêm property `I<Feature>Repository <Feature>s`
- Implementation `UnitOfWork.cs`: thêm lazy-init property

**5c. Register DI**
Thêm vào `InfrastructureServiceExtensions.cs`:
```csharp
services.AddScoped<I<Feature>Service, <Feature>Service>();
```

## Step 6 — Migration

```powershell
dotnet ef migrations add Add<FeatureName>Table `
  --project src/DineFlow.Infrastructure `
  --startup-project src/DineFlow.API
```

Review migration file trước khi apply.

## Step 7 — API Layer

```
src/DineFlow.API/Controllers/<Feature>Controller.cs
```

Tạo controller với:
- `[ApiController]`, `[Route("api/[controller]")]`
- `[Authorize]` class-level
- Standard HTTP verbs + status codes
- Inject `I<Feature>Service`

## Step 8 — Build & Test

```powershell
dotnet build
dotnet test tests/DineFlow.UnitTests/
```

## Step 9 — Commit

```powershell
git add <tất cả files của feature này>
git commit -m "feat: Add <FeatureName> feature (Domain → API)

- Entity: <FeatureName> with ...
- Repository: I<Feature>Repository with ...
- Service: <Feature>Service with ...
- API: <Feature>Controller (GET, POST, PUT, DELETE)"

git push
```
