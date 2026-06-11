using System.ComponentModel.DataAnnotations;

namespace DineFlow.Web.Models;

// ===== DTOs (mirror từ Application layer — không reference trực tiếp) =====

public record CategoryDto(
    Guid   Id,
    string Name,
    int    DisplayOrder,
    bool   IsActive,
    int    ItemCount);

public record MenuItemDto(
    Guid     Id,
    string   Name,
    string?  Description,
    decimal  Price,
    Guid     CategoryId,
    string   CategoryName,
    string?  ImageUrl,
    bool     IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ===== Form ViewModels (dùng cho Create/Edit forms) =====

public class MenuItemFormViewModel
{
    [Required(ErrorMessage = "Tên món ăn không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
    [Display(Name = "Tên món ăn")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Giá không được để trống")]
    [Range(0.01, 99_999_999, ErrorMessage = "Giá phải lớn hơn 0")]
    [Display(Name = "Giá (VND)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Display(Name = "Danh mục")]
    public Guid CategoryId { get; set; }

    [Display(Name = "URL hình ảnh")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Đang bán")]
    public bool IsAvailable { get; set; } = true;

    // Danh sách categories để hiển thị dropdown
    public IEnumerable<CategoryDto> Categories { get; set; } = [];
}

// ===== Index ViewModel =====

public class MenuIndexViewModel
{
    public IEnumerable<MenuItemDto> Items      { get; set; } = [];
    public string?                  SearchTerm { get; set; }
    public Guid?                    CategoryId { get; set; }
    public IEnumerable<CategoryDto> Categories { get; set; } = [];
    public int                      TotalCount { get; set; }
}

// ===== Dashboard ViewModel =====

public class DashboardViewModel
{
    public int     TotalMenuItems     { get; set; }
    public int     AvailableItems     { get; set; }
    public int     UnavailableItems   { get; set; }
}

// ===== Auth ViewModels =====

public class LoginViewModel
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;
}
