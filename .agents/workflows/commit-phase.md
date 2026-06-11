# Commit Phase

**Description**: Verify build, stage đúng files, commit với message chuẩn và push lên GitHub cho phase hiện tại.

Invoke với: `/commit-phase`

---

## Step 1 — Xác định Phase Number

Hỏi user hoặc đọc `task.md` để biết phase nào vừa hoàn thành.

## Step 2 — Build Verification

```powershell
dotnet build
```

- Nếu **0 errors, 0 warnings** → tiếp tục Step 3
- Nếu có **errors** → dừng, fix lỗi, build lại
- Nếu có **warnings** → hỏi user có muốn fix trước không

## Step 3 — Review Unstaged Changes

```powershell
git status --short
git diff --stat HEAD
```

Liệt kê tất cả files changed/untracked và xác định:
- Files thuộc phase hiện tại → sẽ stage
- Files thuộc phase khác → KHÔNG stage

## Step 4 — Stage Files

Stage TỪNG PATH cụ thể, không `git add .`:

```powershell
# Ví dụ Phase 3:
git add src/DineFlow.Application/Services/
git add src/DineFlow.Application/DTOs/
git add src/DineFlow.Application/Mappings/
git add src/DineFlow.Application/DineFlow.Application.csproj
```

Verify staged files:
```powershell
git diff --staged --stat
```

## Step 5 — Tạo Commit Message

Format chuẩn:
```
feat: Phase N - <short title>

<nhóm 1: tên nhóm files>
- file1.cs: mô tả ngắn
- file2.cs: mô tả ngắn

<nhóm 2: patterns áp dụng>
- Pattern 1
- Pattern 2

NuGet packages added (nếu có):
- PackageName vVersion
```

## Step 6 — Commit

```powershell
git commit -m "<commit message từ Step 5>"
```

## Step 7 — Push

```powershell
git push
```

Verify output: `main -> main` hoặc branch hiện tại.

## Step 8 — Confirm

Hiển thị:
```
✅ Phase N committed và pushed thành công!
📎 https://github.com/dinhmanhtri/DineFlow/commits/main
```

Hiển thị `git log --oneline -5` để xem lịch sử gần nhất.
