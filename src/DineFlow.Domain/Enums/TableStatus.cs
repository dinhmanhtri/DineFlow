namespace DineFlow.Domain.Enums;

/// <summary>
/// Trạng thái bàn ăn
/// 
/// [KIẾN THỨC] Tại sao dùng Enum thay vì string?
/// - string "Available" → typo "Availble" compile vẫn pass, runtime mới lỗi
/// - Enum → compiler check ngay, IntelliSense gợi ý, switch exhaustive
/// - EF Core tự convert enum ↔ int trong DB (hoặc string nếu cấu hình)
/// </summary>
public enum TableStatus
{
    Available = 0,  // Trống
    Occupied  = 1,  // Đang có khách
    Reserved  = 2,  // Đã đặt trước
    Cleaning  = 3   // Đang dọn dẹp
}
