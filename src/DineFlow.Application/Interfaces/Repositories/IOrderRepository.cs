using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Interfaces.Repositories;

/// <summary>
/// Specific Repository cho Order — quản lý đơn hàng
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Lấy order kèm đầy đủ thông tin: Table, Staff, OrderItems (với MenuItem)
    /// Đây là "Full Load" — dùng khi cần hiển thị chi tiết order
    /// </summary>
    Task<Order?> GetByIdWithDetailsAsync(Guid id);

    /// <summary>Lấy tất cả orders kèm Table và Staff info</summary>
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();

    /// <summary>Lọc orders theo status</summary>
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);

    /// <summary>Lấy order hiện tại (chưa paid/cancelled) của bàn</summary>
    Task<Order?> GetActiveOrderByTableAsync(Guid tableId);

    /// <summary>
    /// Lấy orders trong khoảng thời gian — dùng cho báo cáo doanh thu
    /// </summary>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime from, DateTime to);
}
