using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Interfaces.Repositories;

/// <summary>
/// Specific Repository cho DiningTable
/// </summary>
public interface ITableRepository : IRepository<DiningTable>
{
    /// <summary>Lấy bàn theo tầng</summary>
    Task<IEnumerable<DiningTable>> GetByFloorAsync(int floor);

    /// <summary>Lấy tất cả bàn theo trạng thái (Available, Occupied...)</summary>
    Task<IEnumerable<DiningTable>> GetByStatusAsync(TableStatus status);

    /// <summary>Kiểm tra số bàn đã tồn tại chưa (tránh duplicate)</summary>
    Task<bool> TableNumberExistsAsync(int tableNumber, Guid? excludeId = null);
}
