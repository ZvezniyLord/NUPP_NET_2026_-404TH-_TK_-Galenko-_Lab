namespace MusicContest.Common.Models;

public class Song : IEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalArtist { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Key { get; set; } = string.Empty;

    // Конструктор
    public Song()
    {
        Id = Guid.NewGuid();
    }

    // Конструктор з параметрами
    public Song(string title, string originalArtist, int durationSeconds, string key) : this()
    {
        Title = title;
        OriginalArtist = originalArtist;
        DurationSeconds = durationSeconds;
        Key = key;
    }

    // Метод
    public string GetFormattedDuration()
    {
        TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds);
        return $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
    }

    // Статичний метод
    public static Song FromMinutes(string title, string originalArtist, double minutes, string key)
    {
        return new Song(title, originalArtist, (int)Math.Round(minutes * 60), key);
    }
}
