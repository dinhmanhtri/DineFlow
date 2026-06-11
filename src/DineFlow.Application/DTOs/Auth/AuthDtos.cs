using System.ComponentModel.DataAnnotations;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.DTOs.Auth;

/// <summary>Request đăng nhập</summary>
public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    string Password
);

/// <summary>
/// Response sau khi đăng nhập thành công
/// 
/// [KIẾN THỨC] JWT (JSON Web Token):
/// Header.Payload.Signature
/// - Header: algorithm (HS256)
/// - Payload: claims (userId, email, role, exp)
/// - Signature: HMAC-SHA256(Base64(header) + "." + Base64(payload), secretKey)
/// 
/// Server KHÔNG lưu token → stateless
/// Client gửi kèm trong header: Authorization: Bearer <token>
/// </summary>
public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string FullName,
    string Email,
    string Role
);

/// <summary>Request tạo tài khoản nhân viên mới (Admin only)</summary>
public record RegisterStaffRequest(
    [Required] [MaxLength(200)] string FullName,

    [Required] [EmailAddress] [MaxLength(200)]
    string Email,

    [Required] [MinLength(6)] [MaxLength(100)]
    string Password,

    [Required] StaffRole Role,

    [MaxLength(20)] string? PhoneNumber
);

/// <summary>DTO thông tin staff (không có password)</summary>
public record StaffDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt
);
