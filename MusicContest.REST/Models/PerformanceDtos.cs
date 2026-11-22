using System.ComponentModel.DataAnnotations;

namespace MusicContest.REST.Models;

public sealed record PerformanceResponse(Guid Id, int ParticipantDatabaseId, int SongDatabaseId, double Score, bool IsCompleted);

public sealed class PerformanceRequest
{
    [Range(1, int.MaxValue)]
    public int ParticipantDatabaseId { get; init; }

    [Range(1, int.MaxValue)]
    public int SongDatabaseId { get; init; }

    [Range(0, 100)]
    public double Score { get; init; }

    public bool IsCompleted { get; init; }
}
