namespace DineFlow.Domain.Enums;

/// <summary>
/// Trạng thái đơn hàng — theo workflow thực tế của nhà hàng:
/// 
/// Pending → Preparing → Served → Paid
///    ↓                              ↓
/// Cancelled ←←←←←←←←←←←←←←←←←←←←←
/// 
/// [KIẾN THỨC] State Machine:
/// Đây là ví dụ điển hình của State Pattern — object chuyển trạng thái theo quy tắc
/// Trong business logic, cần validate: không thể chuyển từ Paid → Pending
/// </summary>
public enum OrderStatus
{
    Pending    = 0,  // Vừa tạo, chờ bếp xác nhận
    Preparing  = 1,  // Bếp đang làm
    Served     = 2,  // Đã mang ra bàn
    Paid       = 3,  // Đã thanh toán
    Cancelled  = 4   // Đã hủy
}
