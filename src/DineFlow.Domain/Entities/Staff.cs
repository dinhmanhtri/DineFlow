using DineFlow.Domain.Common;
using DineFlow.Domain.Enums;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Nhân viên nhà hàng — cũng là User đăng nhập hệ thống
/// 
/// [KIẾN THỨC] Password Hashing:
/// KHÔNG BAO GIỜ lưu password plaintext!
/// Flow đúng:
///   Đăng ký: password → BCrypt.Hash(password) → lưu PasswordHash
///   Đăng nhập: BCrypt.Verify(inputPassword, storedHash) → true/false
/// 
/// BCrypt tự generate salt ngẫu nhiên và embed vào hash string
/// → Cùng password, hash 2 lần sẽ ra 2 kết quả khác nhau (an toàn hơn MD5/SHA)
/// </summary>
public class Staff : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Dùng làm username để đăng nhập</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash của password — KHÔNG lưu plaintext</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public StaffRole Role { get; set; } = StaffRole.Waiter;

    public bool IsActive { get; set; } = true;

    /// <summary>Số điện thoại (optional)</summary>
    public string? PhoneNumber { get; set; }

    // Navigation: nhân viên đã tạo những order nào
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
