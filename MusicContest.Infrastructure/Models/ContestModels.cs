using MusicContest.Common.Models;

namespace MusicContest.Infrastructure.Models;

// EF-моделі відокремлені від доменних класів ЛР1.
// DatabaseId використовується як PK або FK. Guid Id зберігає сумісність із CRUD-контрактом ЛР2.
public abstract class ContestParticipantModel : IEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StageName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Age { get; set; }

    public ParticipantProfileModel? Profile { get; set; }        // 1:1
    public ICollection<PerformanceModel> Performances { get; set; } = new List<PerformanceModel>(); // 1:N
    public ICollection<SongModel> Songs { get; set; } = new List<SongModel>(); // M:N
}

public sealed class SoloSingerModel : ContestParticipantModel
{
    public string VoiceType { get; set; } = string.Empty;
    public string VocalRange { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
}

public sealed class VocalGroupModel : ContestParticipantModel
{
    public int MemberCount { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool HasBackingVocals { get; set; }
}

public sealed class ParticipantProfileModel : IEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ParticipantDatabaseId { get; set; }
    public string Biography { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public ContestParticipantModel Participant { get; set; } = null!;
}

public sealed class SongModel : IEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string OriginalArtist { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Key { get; set; } = string.Empty;
    public ICollection<ContestParticipantModel> Participants { get; set; } = new List<ContestParticipantModel>();
    public ICollection<PerformanceModel> Performances { get; set; } = new List<PerformanceModel>();
}

public sealed class PerformanceModel : IEntity
{
    public int DatabaseId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ParticipantDatabaseId { get; set; }
    public int SongDatabaseId { get; set; }
    public double Score { get; set; }
    public bool IsCompleted { get; set; }
    public ContestParticipantModel Participant { get; set; } = null!;
    public SongModel Song { get; set; } = null!;
}
