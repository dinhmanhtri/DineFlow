using System.ComponentModel.DataAnnotations;

namespace DineFlow.Application.DTOs.Menu;

/// <summary>
/// [KIẾN THỨC] record vs class cho DTO:
/// - record: value-based equality, immutable by default, init-only properties
/// - record { get; init; } → không thể đổi sau khi tạo → thread-safe, predictable
/// - Compiler tự gen Equals(), GetHashCode(), ToString() so sánh theo value
/// 
/// DTO (Data Transfer Object):
/// - Tách biệt Domain Entity với API contract
/// - Entity thay đổi (thêm field nội bộ) không ảnh hưởng DTO
/// - DTO thay đổi (đổi tên field cho FE) không ảnh hưởng Domain
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    int DisplayOrder,
    bool IsActive,
    int MenuItemCount  // Computed từ navigation
);

// ===== Category Request DTOs =====

public record CreateCategoryRequest(
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
    string Name,

    [Range(1, 99, ErrorMessage = "Thứ tự hiển thị phải từ 1 đến 99")]
    int DisplayOrder = 1
);

public record UpdateCategoryRequest(
    [Required] [MaxLength(100)] string Name,
    [Range(1, 99)] int DisplayOrder,
    bool IsActive
);
