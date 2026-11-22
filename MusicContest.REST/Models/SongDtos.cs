using System.ComponentModel.DataAnnotations;

namespace MusicContest.REST.Models;

public sealed record SongResponse(Guid Id, string Title, string OriginalArtist, int DurationSeconds, string Key);

public sealed class SongRequest
{
    [Required, StringLength(160, MinimumLength = 1)]
    public required string Title { get; init; }

    [StringLength(160)]
    public string OriginalArtist { get; init; } = string.Empty;

    [Range(1, 7200)]
    public int DurationSeconds { get; init; }

    [StringLength(40)]
    public string Key { get; init; } = string.Empty;
}
