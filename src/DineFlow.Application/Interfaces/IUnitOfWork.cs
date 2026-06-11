using DineFlow.Application.Interfaces.Repositories;

namespace DineFlow.Application.Interfaces;

/// <summary>
/// Unit of Work Interface — gom tất cả Repositories vào 1 transaction
/// 
/// [KIẾN THỨC] Tại sao cần Unit of Work?
/// 
/// Vấn đề nếu không có UoW:
///   await menuRepo.AddAsync(item);
///   await menuRepo.SaveAsync();  // commit 1
///   await orderRepo.AddAsync(order);
///   await orderRepo.SaveAsync(); // commit 2 — nếu lỗi ở đây, item đã commit rồi!
///   → Inconsistent data!
/// 
/// Giải pháp với UoW:
///   await uow.Menus.AddAsync(item);
///   await uow.Orders.AddAsync(order);
///   await uow.SaveChangesAsync();  // 1 transaction duy nhất — all or nothing!
///   → Atomicity đảm bảo
/// 
/// IDisposable: khi dùng xong (scope kết thúc), DbContext tự đóng connection
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ===== Repositories =====
    IMenuRepository Menus { get; }
    IOrderRepository Orders { get; }
    ITableRepository Tables { get; }

    // ===== Transaction =====

    /// <summary>
    /// Commit tất cả changes trong current transaction vào DB.
    /// Trả về số records bị ảnh hưởng.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin explicit transaction — dùng cho operations phức tạp cần manual control
    /// Ví dụ: create order + update table status phải atomic
    /// </summary>
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}
