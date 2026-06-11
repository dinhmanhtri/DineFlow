# 🍽️ DineFlow — Restaurant Management System

> Fullstack web application quản lý nhà hàng, xây dựng theo **Clean Architecture** với ASP.NET Core 10.

[![CI](https://github.com/dinhmanhtri/DineFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/dinhmanhtri/DineFlow/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| **Backend API** | ASP.NET Core 8 Web API, C# |
| **Web Frontend** | ASP.NET Core 8 MVC, Razor Views |
| **Database** | SQL Server (EF Core 8, Code First) |
| **Caching** | Redis (StackExchange.Redis) |
| **Auth** | JWT Bearer Token |
| **Docs** | Swagger / OpenAPI |
| **Container** | Docker, Docker Compose |
| **CI/CD** | GitHub Actions → Docker Hub → Railway |

---

## 🏗️ Architecture

```
DineFlow/
├── src/
│   ├── DineFlow.Domain/          # Entities, Enums, Exceptions (no dependencies)
│   ├── DineFlow.Application/     # Business Logic, DTOs, Interfaces
│   ├── DineFlow.Infrastructure/  # EF Core, Redis, Repositories
│   ├── DineFlow.API/             # ASP.NET Core Web API
│   └── DineFlow.Web/             # ASP.NET Core MVC (Razor Views)
└── tests/
    └── DineFlow.UnitTests/
```

**Dependency Rule (Clean Architecture):**
```
Domain ← Application ← Infrastructure ← API
                                       ← Web
```

---

## 📦 Modules

- 🍜 **Menu Management** — CRUD món ăn, danh mục
- 🪑 **Table Management** — Sơ đồ bàn theo tầng, trạng thái realtime
- 📋 **Order Management** — Tạo/quản lý order, thêm/xóa món
- 🧾 **Invoice & Payment** — Xuất hóa đơn, áp dụng VAT & discount
- 👥 **Staff Management** — Phân quyền Admin/Waiter/Chef/Cashier
- 📊 **Dashboard** — KPI cards, biểu đồ doanh thu

---

## ⚡ Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- SQL Server (hoặc dùng Docker)

### Run với Docker Compose
```bash
git clone https://github.com/dinhmanhtri/DineFlow.git
cd DineFlow
docker-compose up --build
```

- API: http://localhost:5000/swagger
- Web: http://localhost:3000

### Run locally (Development)

```bash
# 1. Start SQL Server + Redis bằng Docker
docker compose up -d sqlserver redis

# 2. Restore frontend libraries
cd src/DineFlow.Web && libman restore && cd ../..

# 3. Chạy API
dotnet run --project src/DineFlow.API

# 4. Chạy MVC Web
dotnet run --project src/DineFlow.Web
```

### API Endpoints

| Endpoint | Mô tả |
|---|---|
| `GET /swagger` | Swagger UI (Development only) |
| `GET /health` | Tổng hợp trạng thái SQL + Redis |
| `GET /health/ready` | Readiness probe (Kubernetes) |
| `GET /health/live` | Liveness probe (Kubernetes) |

---

## 🔑 Default Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@dineflow.com | Admin@123 |

---

## 📝 Development Phases

| Phase | Status | Nội dung |
|---|---|---|
| Phase 1 | ✅ Done | Solution Setup + Domain Layer |
| Phase 2 | ✅ Done | Infrastructure: EF Core, Repository, UnitOfWork |
| Phase 3 | ✅ Done | Application Services + DTOs + AutoMapper |
| Phase 4 | ✅ Done | Web API + JWT + Swagger + Global Error Handler |
| Phase 5 | ✅ Done | Redis Cache + Docker Compose + Health Checks |
| Phase 6 | ✅ Done | MVC Razor Views — Menu Management UI (Login, Dashboard, CRUD) |
| Phase 7 | ✅ Done | Dockerfile + CI/CD GitHub Actions (ghcr.io) |

---

## 📄 License

MIT © [dinhmanhtri](https://github.com/dinhmanhtri)
