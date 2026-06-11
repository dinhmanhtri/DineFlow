using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Infrastructure.Repositories;

public class TableRepository : BaseRepository<DiningTable>, ITableRepository
{
    public TableRepository(DineFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<DiningTable>> GetByFloorAsync(int floor)
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.FloorNumber == floor)
            .OrderBy(x => x.TableNumber)
            .ToListAsync();

    public async Task<IEnumerable<DiningTable>> GetByStatusAsync(TableStatus status)
        => await _dbSet
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderBy(x => x.TableNumber)
            .ToListAsync();

    public async Task<bool> TableNumberExistsAsync(int tableNumber, Guid? excludeId = null)
        => await _dbSet.AnyAsync(x =>
            x.TableNumber == tableNumber &&
            (excludeId == null || x.Id != excludeId));
}
