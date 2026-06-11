using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Infrastructure.Data;

/// <summary>
/// DbContext — cầu nối giữa C# entities và SQL Server database
/// 
/// [KIẾN THỨC] DbContext lifecycle:
/// - Mỗi HTTP request nên có 1 DbContext riêng (Scoped lifetime trong DI)
/// - DbContext track tất cả entities được query từ nó (Change Tracking)
/// - Khi SaveChangesAsync() → EF tự generate INSERT/UPDATE/DELETE SQL
/// 
/// [KIẾN THỨC] Fluent API vs Data Annotations:
/// - Data Annotations: [Required], [MaxLength(100)] ngay trên property — đơn giản nhưng
///   trộn lẫn với Domain entities (vi phạm SRP)
/// - Fluent API: cấu hình tách riêng trong OnModelCreating → sạch hơn, linh hoạt hơn
///   → Dùng Fluent API cho project này
/// </summary>
public class DineFlowDbContext : DbContext
{
    public DineFlowDbContext(DbContextOptions<DineFlowDbContext> options) : base(options) { }

    // ===== DbSets — mỗi DbSet tương ứng 1 bảng trong DB =====
    public DbSet<Category>    Categories  => Set<Category>();
    public DbSet<MenuItem>    MenuItems   => Set<MenuItem>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<Staff>       Staffs      => Set<Staff>();
    public DbSet<Order>       Orders      => Set<Order>();
    public DbSet<OrderItem>   OrderItems  => Set<OrderItem>();
    public DbSet<Invoice>     Invoices    => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scan tất cả IEntityTypeConfiguration trong assembly này
        // → Tự động apply tất cả configurations, không cần gọi từng cái
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DineFlowDbContext).Assembly);

        // Seed data mặc định
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Override SaveChangesAsync để tự động set UpdatedAt
    /// 
    /// [KIẾN THỨC] Override SaveChanges để inject cross-cutting behavior:
    /// Thay vì mỗi chỗ phải tự set entity.UpdatedAt = DateTime.UtcNow,
    /// ta hook vào SaveChanges → tự động cho tất cả entities
    /// → AOP (Aspect-Oriented Programming) nhẹ
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tìm tất cả entities đang được Modified
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in modifiedEntries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // ===== Seed Categories =====
        var categories = new[]
        {
            new Category { Id = new Guid("11111111-0000-0000-0000-000000000001"), Name = "Khai vị",    DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = new Guid("11111111-0000-0000-0000-000000000002"), Name = "Món chính",  DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = new Guid("11111111-0000-0000-0000-000000000003"), Name = "Tráng miệng",DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = new Guid("11111111-0000-0000-0000-000000000004"), Name = "Đồ uống",   DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
        };
        modelBuilder.Entity<Category>().HasData(categories);

        // ===== Seed Menu Items =====
        var menuItems = new[]
        {
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000001"), Name = "Gỏi cuốn tôm thịt", CategoryId = new Guid("11111111-0000-0000-0000-000000000001"), Price = 45000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000002"), Name = "Bò lúc lắc",        CategoryId = new Guid("11111111-0000-0000-0000-000000000002"), Price = 185000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000003"), Name = "Cơm tấm sườn bì",   CategoryId = new Guid("11111111-0000-0000-0000-000000000002"), Price = 65000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000004"), Name = "Chè ba màu",         CategoryId = new Guid("11111111-0000-0000-0000-000000000003"), Price = 35000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000005"), Name = "Nước ép cam",        CategoryId = new Guid("11111111-0000-0000-0000-000000000004"), Price = 30000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new MenuItem { Id = new Guid("22222222-0000-0000-0000-000000000006"), Name = "Trà đá",             CategoryId = new Guid("11111111-0000-0000-0000-000000000004"), Price = 10000, IsAvailable = true, CreatedAt = DateTime.UtcNow },
        };
        modelBuilder.Entity<MenuItem>().HasData(menuItems);

        // ===== Seed DiningTables =====
        var tables = Enumerable.Range(1, 10).Select(i => new DiningTable
        {
            Id          = new Guid($"33333333-0000-0000-0000-{i:D12}"),
            TableNumber = i,
            FloorNumber = i <= 5 ? 1 : 2,
            Capacity    = i % 3 == 0 ? 6 : 4,
            Status      = TableStatus.Available,
            CreatedAt   = DateTime.UtcNow
        }).ToArray();
        modelBuilder.Entity<DiningTable>().HasData(tables);

        // ===== Seed Admin Staff =====
        // BCrypt hash của "Admin@123"
        modelBuilder.Entity<Staff>().HasData(new Staff
        {
            Id           = new Guid("44444444-0000-0000-0000-000000000001"),
            FullName     = "System Admin",
            Email        = "admin@dineflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role         = StaffRole.Admin,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });
    }
}
