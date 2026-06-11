using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Domain.Entities;
using DineFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Infrastructure.Repositories;

/// <summary>
/// MenuRepository — kế thừa BaseRepository, thêm queries đặc thù
/// 
/// [KIẾN THỨC] Eager Loading với Include():
/// EF Core mặc định KHÔNG load navigation properties (lazy loading OFF by default)
/// → Phải explicitly Include() những gì cần
/// 
/// Cách load:
/// 1. Eager Loading (Include): load cùng lúc với entity chính (1 query với JOIN)
/// 2. Explicit Loading (Load): load riêng sau (2 queries)
/// 3. Lazy Loading: load tự động khi access property (cần virtual + proxy)
///    → Nguy hiểm: N+1 query problem!
/// 
/// → Luôn dùng Eager Loading (Include) trong Repository
/// </summary>
public class MenuRepository : BaseRepository<MenuItem>, IMenuRepository
{
    public MenuRepository(DineFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<MenuItem>> GetAllWithCategoryAsync()
        => await _dbSet
            .AsNoTracking()
            .Include(x => x.Category)   // Eager load: JOIN với Categories table
            .OrderBy(x => x.Category.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync();

    public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(Guid categoryId)
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .Include(x => x.Category)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<IEnumerable<MenuItem>> GetAvailableAsync()
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.IsAvailable)
            .Include(x => x.Category)
            .OrderBy(x => x.Category.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync();

    public async Task<MenuItem?> GetByIdWithCategoryAsync(Guid id)
        => await _dbSet
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
}
