using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicContest.Common.Services;
using MusicContest.Infrastructure.Data;
using MusicContest.Infrastructure.Models;
using MusicContest.REST.Models;

namespace MusicContest.REST.Controllers;

[ApiController]
[Route("api/performances")]
public sealed class PerformancesController(
    ICrudServiceAsync<PerformanceModel> service,
    MusicContestContext db) : ControllerBase
{
    private static PerformanceResponse ToResponse(PerformanceModel p) =>
        new(p.Id, p.ParticipantDatabaseId, p.SongDatabaseId, p.Score, p.IsCompleted);

    private static void Apply(PerformanceModel p, PerformanceRequest input)
    {
        p.ParticipantDatabaseId = input.ParticipantDatabaseId;
        p.SongDatabaseId = input.SongDatabaseId;
        p.Score = input.Score;
        p.IsCompleted = input.IsCompleted;
    }

    private async Task<bool> ReferencesExist(PerformanceRequest input) =>
        await db.Participants.AnyAsync(p => p.DatabaseId == input.ParticipantDatabaseId)
        && await db.Songs.AnyAsync(s => s.DatabaseId == input.SongDatabaseId);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PerformanceResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int amount = 20)
    {
        if (page < 1 || amount is < 1 or > 100 || (long)(page - 1) * amount > int.MaxValue)
            return BadRequest("page має бути не менше 1, amount від 1 до 100.");
        var items = await service.ReadAllAsync(page, amount);
        return Ok(items.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PerformanceResponse>> GetById(Guid id)
    {
        try { return Ok(ToResponse(await service.ReadAsync(id))); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost]
    public async Task<ActionResult<PerformanceResponse>> Create(PerformanceRequest input)
    {
        if (!await ReferencesExist(input))
            return BadRequest("Не знайдено учасника або пісню із заданим DatabaseId.");
        var performance = new PerformanceModel();
        Apply(performance, input);
        if (!await service.CreateAsync(performance)) return Conflict();
        return CreatedAtAction(nameof(GetById), new { id = performance.Id }, ToResponse(performance));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PerformanceRequest input)
    {
        if (!await ReferencesExist(input))
            return BadRequest("Не знайдено учасника або пісню із заданим DatabaseId.");
        PerformanceModel performance;
        try { performance = await service.ReadAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        Apply(performance, input);
        return await service.UpdateAsync(performance) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        PerformanceModel performance;
        try { performance = await service.ReadAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        return await service.RemoveAsync(performance) ? NoContent() : NotFound();
    }
}
