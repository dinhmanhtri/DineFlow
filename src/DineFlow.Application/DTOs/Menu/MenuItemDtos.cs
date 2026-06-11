using System.ComponentModel.DataAnnotations;

namespace DineFlow.Application.DTOs.Menu;

/// <summary>Output DTO — trả về cho client</summary>
public record MenuItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    Guid CategoryId,
    string CategoryName,    // Flattened từ Category navigation
    DateTime CreatedAt
);

/// <summary>Summary DTO — dùng cho danh sách (ít data hơn)</summary>
public record MenuItemSummaryDto(
    Guid Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    string CategoryName
);

// ===== Request DTOs =====

public record CreateMenuItemRequest(
    [Required(ErrorMessage = "Tên món không được để trống")]
    [MaxLength(200)]
    string Name,

    [MaxLength(1000)] string? Description,

    [Required]
    [Range(0, 9_999_999, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
    decimal Price,

    [Required(ErrorMessage = "Danh mục không được để trống")]
    Guid CategoryId,

    [MaxLength(500)] string? ImageUrl
);

public record UpdateMenuItemRequest(
    [Required] [MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9_999_999)] decimal Price,
    [Required] Guid CategoryId,
    [MaxLength(500)] string? ImageUrl,
    bool IsAvailable
);

/// <summary>
/// [KIẾN THỨC] Tại sao tách CreateMenuItemRequest và UpdateMenuItemRequest?
/// 
/// Create: không cần Id (server tự gen), IsAvailable mặc định true
/// Update: cần tất cả fields để user quyết định giữ hay đổi
/// → PATCH (partial update) phức tạp hơn, dùng PUT (full replace) cho đơn giản
/// </summary>
