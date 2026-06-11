---
name: dineflow-phase-commit
description: Manages the phased development workflow for the DineFlow project. Use this skill when committing code, starting a new phase, or when the user says "Phase N" or asks to commit/push changes in DineFlow.
---

# DineFlow Phase Commit Skill

This skill governs the **git workflow** and **phase-based development** for the DineFlow restaurant management system. Follow these instructions precisely when working on any phase.

## Project Context

- **Repo**: https://github.com/dinhmanhtri/DineFlow
- **Branch**: `main`
- **Architecture**: Clean Architecture (.NET 10, ASP.NET Core 8)
- **Git remote**: origin (already configured with PAT)

## Phase Definitions

| Phase | Name | Key Files |
|---|---|---|
| 1 | Solution Setup & Domain Layer | `src/DineFlow.Domain/**`, `.gitignore`, `README.md`, `*.slnx` |
| 2 | Infrastructure (EF Core, Repository, Redis) | `src/DineFlow.Application/Interfaces/**`, `src/DineFlow.Infrastructure/**` |
| 3 | Application Services & DTOs | `src/DineFlow.Application/Services/**`, `src/DineFlow.Application/DTOs/**`, `src/DineFlow.Application/Mappings/**` |
| 4 | Web API (Controllers, JWT, Swagger, Middleware) | `src/DineFlow.API/**` |
| 5 | MVC Frontend (Razor Views, CSS, JS) | `src/DineFlow.Web/**` |
| 6 | Docker & Docker Compose | `docker/`, `docker-compose.yml`, `*.Dockerfile` |
| 7 | CI/CD (GitHub Actions) | `.github/workflows/**` |

## Commit Message Format

Always use **Conventional Commits** format:

```
feat: Phase N - <short title>

<What was added, categorized by file group>

Key patterns applied:
- <pattern 1>
- <pattern 2>
```

**Examples:**
```
feat: Phase 3 - Application services & DTOs (AutoMapper, CQRS-lite)
feat: Phase 4 - Web API with JWT auth, Swagger, global error handling
fix: Phase 2 - Fix ambiguous JsonSerializer overload in RedisCacheService
```

## Workflow Steps

When the user says "Phase N" or asks to commit a phase:

### Step 1 — Verify Build
```powershell
dotnet build
```
**STOP** if there are errors. Fix them before committing.

### Step 2 — Check What's Staged
```powershell
git status --short
```
Review which files belong to the current phase.

### Step 3 — Stage Phase-Specific Files
Stage **only files belonging to the current phase** (not unrelated files from other phases):
```powershell
git add <phase-specific-paths>
```

Do NOT `git add .` — always be selective by phase.

### Step 4 — Commit with Descriptive Message
```powershell
git commit -m "feat: Phase N - <title>`n`n<detailed body>"
```

### Step 5 — Push
```powershell
git push
```

### Step 6 — Verify on GitHub
After push, confirm the commit appears at:
`https://github.com/dinhmanhtri/DineFlow/commits/main`

## Rules

- **Never commit** `appsettings.Development.json`, `appsettings.Production.json`, `bin/`, `obj/`
- **Always build first** before committing
- **One commit per phase** — if a phase has fixes, use `fix: Phase N - <description>`
- **Separate commits** for unrelated changes (e.g., don't mix Phase 3 code with Phase 4)
- Keep `obj/` and `bin/` out of git (already in `.gitignore`)

## Task Tracking

After each phase commit, update the README.md Phase status table:
- `⬜ Todo` → `🔄 In Progress` → `✅ Done`

Then commit the README update as:
```
docs: Update phase status in README (Phase N complete)
```
