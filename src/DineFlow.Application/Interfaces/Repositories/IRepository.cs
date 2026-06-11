using System.Linq.Expressions;
using DineFlow.Domain.Common;

namespace DineFlow.Application.Interfaces.Repositories;

/// <summary>
/// Generic Repository Interface — áp dụng cho mọi Entity kế thừa BaseEntity
/// 
/// [KIẾN THỨC] Generic Repository Pattern:
/// Thay vì viết IMenuRepository, IOrderRepository... đều có CRUD giống nhau,
/// ta tạo 1 Generic Repository chứa common operations.
/// 
/// Specific repositories (IMenuRepository) kế thừa từ đây và thêm
/// các query đặc thù (GetByCategory, GetAvailable...)
/// 
/// where T : BaseEntity → Constraint: T phải là subtype của BaseEntity
/// → Đảm bảo T có Id, CreatedAt... để dùng trong GetByIdAsync, v.v.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    // ===== Basic CRUD =====

    Task<T?> GetByIdAsync(Guid id);

    Task<IEnumerable<T>> GetAllAsync();

    Task AddAsync(T entity);

    void Update(T entity);       // Synchronous — EF chỉ mark as Modified, không query DB ngay

    void Delete(T entity);       // Synchronous — EF chỉ mark as Deleted

    // ===== Query Helpers =====

    /// <summary>
    /// Tìm theo điều kiện tùy ý bằng LINQ Expression
    /// Ví dụ: FindAsync(x => x.IsActive && x.Price > 50000)
    /// 
    /// [KIẾN THỨC] Expression vs Func:
    /// - Func<T, bool>: delegate → chạy in-memory (LINQ to Objects)
    /// - Expression<Func<T, bool>>: expression tree → EF dịch sang SQL (LINQ to SQL)
    /// → Luôn dùng Expression cho Repository để EF tạo WHERE clause trong SQL
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
