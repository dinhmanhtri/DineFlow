using DineFlow.Application.Interfaces.Repositories;
using DineFlow.Domain.Entities;
using DineFlow.Infrastructure.Data;

namespace DineFlow.Infrastructure.Repositories;

public class StaffRepository(DineFlowDbContext context)
    : BaseRepository<Staff>(context), IStaffRepository
{
    // Kế thừa tất cả CRUD từ BaseRepository<Staff>
}
