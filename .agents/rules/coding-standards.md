# DineFlow — Coding Standards

> **Activation**: Always On
> Áp dụng cho toàn bộ code trong project DineFlow.

## C# Style

- Dùng `var` khi type rõ ràng từ RHS: `var items = new List<MenuItem>()`
- Dùng explicit type khi return type không rõ: `IEnumerable<MenuItem> items = ...`
- File-scoped namespace: `namespace DineFlow.Domain.Entities;` (không dùng block `{}`)
- Primary constructor cho simple classes, record cho DTOs
- Null-forgiving `null!` chỉ cho EF Core navigation properties
- Dùng `string.Empty` thay `""` cho default string property
- `DateTime.UtcNow` (không bao giờ `DateTime.Now`) để tránh timezone issues
- `decimal` cho mọi giá trị tiền tệ — KHÔNG dùng `double` hay `float`

## Naming

| Element | Convention | Ví dụ |
|---|---|---|
| Private field | `_camelCase` | `_context`, `_menuService` |
| Const | `PascalCase` | `DefaultPageSize` |
| Async method | Suffix `Async` | `GetAllAsync()` |
| Boolean property | `Is/Has/Can` prefix | `IsAvailable`, `CanBeCancelled` |
| Interface | `I` prefix | `IMenuRepository` |

## Clean Architecture — Layer Rules

**KHÔNG được vi phạm dependency rule:**
```
Domain ← Application ← Infrastructure ← API/Web
```

- `Domain`: không import bất cứ NuGet package nào
- `Application`: không import EF Core, Redis, BCrypt
- `Infrastructure`: implement interfaces từ Application
- Controller nhận Service interface, không nhận Repository trực tiếp

## EF Core Conventions

- Luôn `AsNoTracking()` cho read-only queries
- Luôn `Include()` explicit — không dùng lazy loading
- `decimal` columns: `HasColumnType("decimal(18,2)")`
- Enum columns: `.HasConversion<string>().HasMaxLength(20)`
- Navigation properties required: dùng `null!`

## API Conventions

- HTTP status codes chuẩn: 200/201/204/400/401/403/404/409/500
- Controller action không chứa business logic — delegate cho Service
- Validate với `[Required]`, `[Range]` trên Request DTOs
- Global error handler xử lý domain exceptions → HTTP codes

## Comments & Documentation

- XML doc (`/// <summary>`) cho public methods trong Domain và Application
- Inline comment giải thích "tại sao" không phải "cái gì"
- Comments bằng tiếng Việt OK cho domain-specific logic
- Xóa TODO comments trước khi commit phase
