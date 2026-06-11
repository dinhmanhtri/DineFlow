using DineFlow.Domain.Entities;

namespace DineFlow.Application.Interfaces.Repositories;

/// <summary>
/// Specific Repository Interface cho MenuItem
/// 
/// [KIẾN THỨC] Tại sao cần Specific Repository ngoài Generic?
/// 
/// IRepository<MenuItem> có GetAllAsync() — trả về tất cả MenuItem, không có Category
/// Nhưng thực tế cần: MenuItem kèm CategoryName để hiển thị → cần JOIN
/// 
/// Specific Repository thêm các query phức tạp, đặc thù cho từng entity:
/// - GetAllWithCategoryAsync(): MenuItem + eager load Category
/// - GetByCategoryAsync(): lọc theo category
/// - GetAvailableAsync(): chỉ lấy món còn phục vụ
/// 
/// Còn CRUD cơ bản vẫn kế thừa từ IRepository<MenuItem>
/// </summary>
public interface IMenuRepository : IRepository<MenuItem>
{
    /// <summary>
    /// Lấy tất cả menu kèm thông tin Category (Eager Loading)
    /// SQL: SELECT m.*, c.Name FROM MenuItems m JOIN Categories c ON m.CategoryId = c.Id
    /// </summary>
    Task<IEnumerable<MenuItem>> GetAllWithCategoryAsync();

    /// <summary>Lấy menu theo danh mục</summary>
    Task<IEnumerable<MenuItem>> GetByCategoryAsync(Guid categoryId);

    /// <summary>Chỉ lấy món đang available (IsAvailable = true)</summary>
    Task<IEnumerable<MenuItem>> GetAvailableAsync();

    /// <summary>Lấy MenuItem kèm Category theo Id</summary>
    Task<MenuItem?> GetByIdWithCategoryAsync(Guid id);
}
