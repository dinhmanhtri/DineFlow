using DineFlow.Application.DTOs.Table;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Interfaces.Services;

public interface ITableService
{
    Task<IEnumerable<DiningTableDto>> GetAllTablesAsync();
    Task<IEnumerable<DiningTableDto>> GetTablesByFloorAsync(int floor);
    Task<IEnumerable<DiningTableDto>> GetTablesByStatusAsync(TableStatus status);
    Task<DiningTableDto> GetTableByIdAsync(Guid id);
    Task<DiningTableDto> CreateTableAsync(CreateTableRequest request);
    Task<DiningTableDto> UpdateTableStatusAsync(Guid id, UpdateTableStatusRequest request);
    Task DeleteTableAsync(Guid id);
}
