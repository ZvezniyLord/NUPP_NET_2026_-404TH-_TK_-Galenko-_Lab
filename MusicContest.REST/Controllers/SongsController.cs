using Microsoft.AspNetCore.Mvc;
using MusicContest.Common.Services;
using MusicContest.Infrastructure.Models;
using MusicContest.REST.Models;

namespace MusicContest.REST.Controllers;

[ApiController]
[Route("api/songs")]
public sealed class SongsController(ICrudServiceAsync<SongModel> service) : ControllerBase
{
    private static SongResponse ToResponse(SongModel song) =>
        new(song.Id, song.Title, song.OriginalArtist, song.DurationSeconds, song.Key);

    private static void Apply(SongModel song, SongRequest input)
    {
        song.Title = input.Title;
        song.OriginalArtist = input.OriginalArtist;
        song.DurationSeconds = input.DurationSeconds;
        song.Key = input.Key;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SongResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int amount = 20)
    {
        if (page < 1 || amount is < 1 or > 100 || (long)(page - 1) * amount > int.MaxValue)
            return BadRequest("page має бути не менше 1, amount від 1 до 100.");
        var songs = await service.ReadAllAsync(page, amount);
        return Ok(songs.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SongResponse>> GetById(Guid id)
    {
        try { return Ok(ToResponse(await service.ReadAsync(id))); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost]
    public async Task<ActionResult<SongResponse>> Create(SongRequest input)
    {
        var song = new SongModel();
        Apply(song, input);
        if (!await service.CreateAsync(song)) return Conflict();
        return CreatedAtAction(nameof(GetById), new { id = song.Id }, ToResponse(song));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SongRequest input)
    {
        SongModel song;
        try { song = await service.ReadAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        Apply(song, input);
        return await service.UpdateAsync(song) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        SongModel song;
        try { song = await service.ReadAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        return await service.RemoveAsync(song) ? NoContent() : NotFound();
    }
}
