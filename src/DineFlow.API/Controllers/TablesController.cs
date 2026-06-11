using DineFlow.Application.DTOs.Table;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.API.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController(ITableService tableService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DiningTableDto>), 200)]
    public async Task<IActionResult> GetTables(
        [FromQuery] int? floor,
        [FromQuery] TableStatus? status)
    {
        if (floor.HasValue)
            return Ok(await tableService.GetTablesByFloorAsync(floor.Value));

        if (status.HasValue)
            return Ok(await tableService.GetTablesByStatusAsync(status.Value));

        return Ok(await tableService.GetAllTablesAsync());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DiningTableDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTable(Guid id)
    {
        var table = await tableService.GetTableByIdAsync(id);
        return Ok(table);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DiningTableDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
    {
        var table = await tableService.CreateTableAsync(request);
        return CreatedAtAction(nameof(GetTable), new { id = table.Id }, table);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(DiningTableDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTableStatus(Guid id, [FromBody] UpdateTableStatusRequest request)
    {
        var table = await tableService.UpdateTableStatusAsync(id, request);
        return Ok(table);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteTable(Guid id)
    {
        await tableService.DeleteTableAsync(id);
        return NoContent();
    }
}
