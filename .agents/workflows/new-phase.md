# Implement New Phase

**Description**: Thực hiện đầy đủ một phase mới trong DineFlow — từ setup đến build verify và commit.

Invoke với: `/new-phase`

---

## Step 1 — Xác định Phase

Đọc `implementation_plan.md` để xác định:
- Phase number và tên
- Danh sách files cần tạo
- Kiến thức/patterns cần áp dụng

Nếu implementation_plan chưa có phase đó, hỏi user trước khi tiếp tục.

## Step 2 — Đọc Skills

Đọc các skill files liên quan:
- `.agents/skills/dineflow-clean-architecture/SKILL.md` — layer rules
- `.agents/skills/dineflow-phase-commit/SKILL.md` — commit conventions

## Step 3 — Tạo Code Files

Tạo từng file theo thứ tự dependency (interfaces trước, implementations sau):

1. Interfaces (Application layer) → nếu phase cần
2. DTOs / Request models → nếu phase cần
3. Service implementations (Application layer) → nếu phase cần
4. Infrastructure implementations → nếu phase cần
5. Controller / View code → nếu phase cần

**Với mỗi file:**
- Thêm comment giải thích kiến thức (`// [KIẾN THỨC] ...`)
- Tuân thủ naming conventions từ `coding-standards.md`
- Không vi phạm dependency rule

## Step 4 — Register DI (nếu có service mới)

Cập nhật `InfrastructureServiceExtensions.cs` hoặc `Program.cs` với các registrations mới.

## Step 5 — Build Verify

```powershell
dotnet build
```

Nếu có lỗi → tự fix, không hỏi user, build lại cho đến khi pass.

## Step 6 — Update Task Tracker

Cập nhật `task.md`:
- Mark các tasks của phase này là `[x]`
- Thêm phase tiếp theo vào task list nếu chưa có

## Step 7 — Commit Phase

Gọi workflow `/commit-phase` hoặc thực hiện:

```powershell
# Stage chỉ files của phase này
git add <phase-specific-paths>

# Commit với message đúng format
git commit -m "feat: Phase N - <title>`n`n<body>"

# Push
git push
```

## Step 8 — Update README

Cập nhật bảng Phase Status trong `README.md`:
- Phase vừa xong: `🔄 In Progress` → `✅ Done`
- Phase tiếp theo: `⬜ Todo` → `🔄 In Progress`

```powershell
git add README.md
git commit -m "docs: Update phase status (Phase N complete)"
git push
```

## Step 9 — Summary

Hiển thị tóm tắt bảng:
- Files đã tạo
- Patterns/concepts đã áp dụng
- Link commit trên GitHub
- Hướng dẫn gõ phase tiếp theo
