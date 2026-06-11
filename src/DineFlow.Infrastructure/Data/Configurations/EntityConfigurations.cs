using DineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DineFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Fluent API Configuration cho MenuItem
/// 
/// [KIẾN THỨC] IEntityTypeConfiguration<T>:
/// Mỗi entity có 1 class configuration riêng → Single Responsibility
/// Tất cả được scan và apply tự động qua ApplyConfigurationsFromAssembly()
/// 
/// Fluent API ưu tiên hơn DataAnnotations vì:
/// 1. Domain entity sạch, không có [Attribute]
/// 2. Cấu hình phức tạp hơn (composite FK, owned types...)
/// 3. Override convention dễ dàng
/// </summary>
public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Quan trọng: decimal cần specify precision để tránh warning và đảm bảo accuracy
        // decimal(18, 2) → tối đa 18 chữ số, 2 chữ số sau dấu phẩy
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        // Relationship: MenuItem (N) → Category (1)
        builder.HasOne(x => x.Category)
            .WithMany(x => x.MenuItems)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Không xóa Category nếu còn MenuItem
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        // Index để query theo DisplayOrder nhanh hơn
        builder.HasIndex(x => x.DisplayOrder);
    }
}

public class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
{
    public void Configure(EntityTypeBuilder<DiningTable> builder)
    {
        builder.ToTable("DiningTables");
        builder.HasKey(x => x.Id);

        // Unique constraint: không được có 2 bàn cùng số
        builder.HasIndex(x => x.TableNumber).IsUnique();

        // Lưu enum dưới dạng string trong DB thay vì int
        // → Dễ đọc trong DB, không bị lỗi khi thêm enum value mới
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staffs");
        builder.HasKey(x => x.Id);

        // Email phải unique — dùng làm username đăng nhập
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

        // Order → Table (nhiều orders trên 1 bàn theo thời gian)
        builder.HasOne(x => x.Table)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order → Staff
        builder.HasOne(x => x.Staff)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index để query orders theo CreatedAt (báo cáo doanh thu)
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Status);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Note).HasMaxLength(300);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Order → xóa luôn OrderItems

        builder.HasOne(x => x.MenuItem)
            .WithMany()
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // SubTotal là computed property — Ignore, không tạo cột trong DB
        builder.Ignore(x => x.SubTotal);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        // 1-1 relationship: Invoice ↔ Order
        // OrderId là Unique FK
        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Invoice)
            .HasForeignKey<Invoice>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cashier)
            .WithMany()
            .HasForeignKey(x => x.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
