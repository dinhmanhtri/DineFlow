using System.Linq.Expressions;
using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Domain.Common;
using DineFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Infrastructure.Repositories;

/// <summary>
/// Generic Repository Implementation — cung cấp CRUD chuẩn cho mọi Entity
/// 
/// [KIẾN THỨC] Generic Class với Constraint:
/// class BaseRepository<T> where T : BaseEntity
/// → T phải là subtype của BaseEntity
/// → Cho phép dùng T.Id, T.CreatedAt... mà compiler không complain
/// 
/// [KIẾN THỨC] protected readonly:
/// - protected: subclass (MenuRepository) có thể dùng _context, _dbSet
/// - readonly: chỉ gán 1 lần trong constructor → immutable, thread-safe hơn
/// 
/// [KIẾN THỨC] DbSet<T> vs IQueryable<T>:
/// - DbSet<T>: EF-specific, có thêm Add/Remove...
/// - IQueryable<T>: standard LINQ interface, lazy evaluation (không query DB ngay)
///   → Query chỉ thực sự execute khi gọi ToListAsync(), FirstOrDefaultAsync()...
/// </summary>
public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DineFlowDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(DineFlowDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();  // EF tìm DbSet tương ứng với T
    }

    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);
    // FindAsync → tìm trong cache (ChangeTracker) trước, không thấy mới query DB
    // → Tốt cho lookup by PK

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();
    // AsNoTracking(): EF không track entities → nhanh hơn cho read-only queries
    // Dùng khi chỉ đọc dữ liệu, không cần update

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
    // Chưa insert vào DB — chỉ add vào ChangeTracker với state Added
    // DB insert thực sự xảy ra khi SaveChangesAsync()

    public void Update(T entity)
        => _dbSet.Update(entity);
    // Mark entity là Modified → EF sẽ UPDATE tất cả columns khi SaveChanges
    // (không chỉ columns thay đổi — đây là full update)

    public void Delete(T entity)
        => _dbSet.Remove(entity);
    // Mark entity là Deleted → EF sẽ DELETE khi SaveChanges

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);
    // SELECT TOP 1 1 ... — chỉ check tồn tại, không load data → rất nhanh

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
}
