namespace DineFlow.Domain.Enums;

/// <summary>
/// Vai trò nhân viên — dùng cho Role-based Authorization trong Phase 4
/// 
/// [KIẾN THỨC] [Flags] attribute:
/// Nếu muốn 1 user có nhiều role, dùng [Flags] và gán giá trị là power of 2:
///   Admin = 1, Waiter = 2, Chef = 4, Cashier = 8
///   Một user vừa là Admin vừa là Waiter: role = Admin | Waiter = 3
/// 
/// Hiện tại không dùng [Flags] vì mỗi staff chỉ có 1 role cố định.
/// </summary>
public enum StaffRole
{
    Admin   = 0,  // Quản lý — toàn quyền
    Waiter  = 1,  // Phục vụ — tạo/sửa order
    Chef    = 2,  // Bếp — xem và cập nhật trạng thái order
    Cashier = 3   // Thu ngân — tạo hóa đơn, thanh toán
}
