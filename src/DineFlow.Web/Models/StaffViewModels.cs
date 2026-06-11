namespace DineFlow.Web.Models;

// ===== DTOs (mirror từ Application layer) =====

public record StaffViewDto(
    Guid     Id,
    string   FullName,
    string   Email,
    string   Role,          // RoleDisplay
    bool     IsActive,
    string?  PhoneNumber,
    DateTime CreatedAt);

// ===== ViewModels =====

public class StaffIndexViewModel
{
    public IEnumerable<StaffViewDto> Staff       { get; set; } = [];
    public int                       TotalCount  { get; set; }
    public int                       ActiveCount { get; set; }
}

// ===== Dashboard ViewModel (nâng cấp với dữ liệu thực) =====

public class DashboardSummaryViewModel
{
    public decimal  TodayRevenue      { get; set; }
    public int      TodayOrders       { get; set; }
    public int      ActiveTables      { get; set; }
    public int      AvailableTables   { get; set; }
    public decimal  RevenueGrowth     { get; set; }
    public IEnumerable<TopMenuItemViewModel>    TopItems      { get; set; } = [];
    public IEnumerable<RevenueByHourViewModel>  HourlyRevenue { get; set; } = [];
}

public record TopMenuItemViewModel(string Name, int TotalQuantity, decimal TotalRevenue);
public record RevenueByHourViewModel(int Hour, decimal Amount);
