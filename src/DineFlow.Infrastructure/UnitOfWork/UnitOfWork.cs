using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Infrastructure.Data;
using DineFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DineFlow.Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work Implementation
/// 
/// [KIẾN THỨC] Lazy Initialization với ??= operator:
/// private IMenuRepository? _menus;
/// public IMenuRepository Menus => _menus ??= new MenuRepository(_context);
/// 
/// Nghĩa là: nếu _menus chưa được khởi tạo (null), tạo mới và gán
/// → Lazy: chỉ tạo Repository khi thực sự cần dùng (tiết kiệm memory)
/// → Tất cả Repositories dùng chung 1 DbContext instance → cùng transaction
/// 
/// [KIẾN THỨC] IDbContextTransaction:
/// Khi cần nhiều operations phải atomic (tất cả thành công hoặc tất cả fail),
/// dùng explicit transaction:
///   await uow.BeginTransactionAsync();
///   try {
///     // operations...
///     await uow.CommitTransactionAsync();
///   } catch {
///     await uow.RollbackTransactionAsync();
///   }
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DineFlowDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Lazy initialization: chỉ tạo khi cần
    private IMenuRepository? _menus;
    private IOrderRepository? _orders;
    private ITableRepository? _tables;
    private IStaffRepository? _staff;

    public UnitOfWork(DineFlowDbContext context)
    {
        _context = context;
    }

    // ===== Repository Properties (Lazy) =====
    public IMenuRepository  Menus  => _menus  ??= new MenuRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public ITableRepository Tables => _tables ??= new TableRepository(_context);
    public IStaffRepository Staff  => _staff  ??= new StaffRepository(_context);

    // ===== Transaction Management =====

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction is not null) return; // Không nest transaction
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("Không có transaction nào đang active.");

        try
        {
            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction is null) return;

        try { await _currentTransaction.RollbackAsync(); }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // ===== IDisposable =====

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
