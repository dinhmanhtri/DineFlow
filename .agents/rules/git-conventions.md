# DineFlow — Git & Phase Commit Conventions

> **Activation**: Model Decision — Áp dụng khi người dùng đề cập đến commit, push, git, phase, hoặc GitHub.

## Branch Strategy

```
main          ← production-ready, mỗi phase đã complete
develop       ← integration branch (tùy chọn cho team lớn)
feat/phase-N  ← feature branch cho từng phase (tùy chọn)
```

Với project solo, commit thẳng vào `main` theo từng phase.

## Commit Message Format (Conventional Commits)

```
<type>: Phase N - <short title (≤ 72 chars)>

<body: what changed, organized by file group>

<footer: breaking changes hoặc issue references nếu có>
```

### Commit Types

| Type | Dùng khi |
|---|---|
| `feat` | Thêm tính năng mới (phase mới, module mới) |
| `fix` | Sửa bug trong code |
| `refactor` | Tái cấu trúc không thay đổi behavior |
| `chore` | Cấu hình, tooling, scripts |
| `docs` | Chỉ thay đổi documentation |
| `test` | Thêm hoặc sửa tests |
| `ci` | GitHub Actions workflows |

### Ví dụ commit messages hợp lệ

```
feat: Phase 3 - Application services, DTOs, AutoMapper

feat: Phase 4 - Web API with JWT auth and Swagger

fix: Phase 2 - Fix ambiguous JsonSerializer overload in RedisCacheService

docs: Update README phase status table after Phase 3 completion

chore: Add agent skills and rules for DineFlow development

ci: Add GitHub Actions CI workflow for build and test
```

## Staged Files Rules

Khi commit phase N, chỉ stage files thuộc phase đó:

```powershell
# ĐÚNG: stage theo path cụ thể
git add src/DineFlow.Application/Services/
git add src/DineFlow.Application/DTOs/

# SAI: không git add . (có thể lẫn lộn nhiều phase)
git add .
```

## Pre-Commit Checklist

Trước mỗi commit, xác nhận:
- [ ] `dotnet build` — 0 errors, 0 warnings
- [ ] `git status` — không có file nhạy cảm (appsettings.*.json, .env)
- [ ] `git diff --staged` — review nội dung thực sự được commit
- [ ] Commit message đúng format

## Remote & Push

```powershell
# Push sau mỗi phase commit
git push

# Xem lịch sử commits
git log --oneline --graph
```

Repo: https://github.com/dinhmanhtri/DineFlow
