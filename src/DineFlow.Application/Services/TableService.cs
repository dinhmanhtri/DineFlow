using AutoMapper;
using DineFlow.Application.DTOs.Table;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Domain.Exceptions;

namespace DineFlow.Application.Services;

public class TableService(
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper) : ITableService
{
    private const string CacheKeyAll = "table:status:all";

    public async Task<IEnumerable<DiningTableDto>> GetAllTablesAsync()
    {
        // Table status thay đổi liên tục → TTL ngắn (30 giây)
        var cached = await cache.GetAsync<List<DiningTableDto>>(CacheKeyAll);
        if (cached is not null) return cached;

        var tables = await unitOfWork.Tables.GetAllAsync();
        var dtos   = mapper.Map<List<DiningTableDto>>(tables);

        await cache.SetAsync(CacheKeyAll, dtos, TimeSpan.FromSeconds(30));
        return dtos;
    }

    public async Task<IEnumerable<DiningTableDto>> GetTablesByFloorAsync(int floor)
    {
        var tables = await unitOfWork.Tables.GetByFloorAsync(floor);
        return mapper.Map<IEnumerable<DiningTableDto>>(tables);
    }

    public async Task<IEnumerable<DiningTableDto>> GetTablesByStatusAsync(TableStatus status)
    {
        var tables = await unitOfWork.Tables.GetByStatusAsync(status);
        return mapper.Map<IEnumerable<DiningTableDto>>(tables);
    }

    public async Task<DiningTableDto> GetTableByIdAsync(Guid id)
    {
        var table = await unitOfWork.Tables.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(DiningTable), id);

        return mapper.Map<DiningTableDto>(table);
    }

    public async Task<DiningTableDto> CreateTableAsync(CreateTableRequest request)
    {
        // Kiểm tra số bàn đã tồn tại chưa
        if (await unitOfWork.Tables.TableNumberExistsAsync(request.TableNumber))
            throw new ConflictException($"Bàn số {request.TableNumber} đã tồn tại.");

        var table = mapper.Map<DiningTable>(request);

        await unitOfWork.Tables.AddAsync(table);
        await unitOfWork.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeyAll);
        return mapper.Map<DiningTableDto>(table);
    }

    public async Task<DiningTableDto> UpdateTableStatusAsync(Guid id, UpdateTableStatusRequest request)
    {
        var table = await unitOfWork.Tables.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(DiningTable), id);

        table.Status = request.Status;
        unitOfWork.Tables.Update(table);
        await unitOfWork.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeyAll);
        return mapper.Map<DiningTableDto>(table);
    }

    public async Task DeleteTableAsync(Guid id)
    {
        var table = await unitOfWork.Tables.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(DiningTable), id);

        // Không xóa bàn đang có khách
        if (table.Status == TableStatus.Occupied)
            throw new ValidationException("Status", "Không thể xóa bàn đang có khách.");

        unitOfWork.Tables.Delete(table);
        await unitOfWork.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeyAll);
    }
}
