using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MusicContest.Common.Models;

namespace MusicContest.Nosql.Models;

// Один документ MongoDB може містити вкладену інформацію без JOIN.
public sealed class SongDocument : IEntity
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int DatabaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalArtist { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Key { get; set; } = string.Empty;
    public SongStatsDocument Stats { get; set; } = new();
}

public sealed class SongStatsDocument
{
    public int ListenerVotes { get; set; }
    public List<string> Tags { get; set; } = new();
}
