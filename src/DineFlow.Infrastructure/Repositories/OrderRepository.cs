using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Infrastructure.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(DineFlowDbContext context) : base(context) { }

    /// <summary>
    /// Load Order kèm ThenInclude — Load nested navigation properties
    /// 
    /// [KIẾN THỨC] ThenInclude():
    /// Include(x => x.OrderItems)                  → load OrderItems
    /// .ThenInclude(oi => oi.MenuItem)              → load MenuItem của từng OrderItem
    /// .ThenInclude(m => m.Category)                → load Category của từng MenuItem
    /// 
    /// SQL tương đương:
    /// SELECT o.*, oi.*, m.*, c.*
    /// FROM Orders o
    /// LEFT JOIN OrderItems oi ON oi.OrderId = o.Id
    /// LEFT JOIN MenuItems m ON m.Id = oi.MenuItemId
    /// LEFT JOIN Categories c ON c.Id = m.CategoryId
    /// WHERE o.Id = @id
    /// </summary>
    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet
            .AsNoTracking()
            .Include(x => x.Table)
            .Include(x => x.Staff)
            .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                    .ThenInclude(m => m.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
        => await _dbSet
            .AsNoTracking()
            .Include(x => x.Table)
            .Include(x => x.Staff)
            .Include(x => x.OrderItems)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.Status == status)
            .Include(x => x.Table)
            .Include(x => x.Staff)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public async Task<Order?> GetActiveOrderByTableAsync(Guid tableId)
        => await _dbSet
            .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(x =>
                x.TableId == tableId &&
                x.Status != OrderStatus.Paid &&
                x.Status != OrderStatus.Cancelled);

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
            .Include(x => x.Invoice)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
}
