using System.ComponentModel.DataAnnotations;

namespace DineFlow.Web.Models;

// ===== DTOs (mirror từ Application layer) =====

public record DiningTableDto(
    Guid   Id,
    int    TableNumber,
    int    FloorNumber,
    int    Capacity,
    int    Status,           // enum int value
    string StatusDisplay);   // "Trống", "Đang có khách"...

// ===== ViewModels =====

public class TableIndexViewModel
{
    public IEnumerable<DiningTableDto> Tables     { get; set; } = [];
    public IEnumerable<int>            Floors     { get; set; } = [];
    public int?                        FilterFloor { get; set; }
    public int  TotalTables     { get; set; }
    public int  AvailableTables { get; set; }
    public int  OccupiedTables  { get; set; }
}

public class TableFormViewModel
{
    [Required(ErrorMessage = "Số bàn không được để trống")]
    [Range(1, 999, ErrorMessage = "Số bàn phải từ 1 đến 999")]
    [Display(Name = "Số bàn")]
    public int TableNumber { get; set; }

    [Required(ErrorMessage = "Tầng không được để trống")]
    [Range(1, 10, ErrorMessage = "Tầng phải từ 1 đến 10")]
    [Display(Name = "Tầng")]
    public int FloorNumber { get; set; } = 1;

    [Required(ErrorMessage = "Sức chứa không được để trống")]
    [Range(2, 20, ErrorMessage = "Sức chứa phải từ 2 đến 20")]
    [Display(Name = "Sức chứa (người)")]
    public int Capacity { get; set; } = 4;
}
