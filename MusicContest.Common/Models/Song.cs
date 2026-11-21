using MusicContest.Common.Data;

namespace MusicContest.Common.Models;

// Song = пісня.
// Клас реалізує IEntity, тому його також можна зберігати у CrudService<Song>.
public class Song : IEntity
{
    // Title = назва пісні;
    // OriginalArtist = оригінальний виконавець;
    // DurationSeconds = тривалість у секундах;
    // Key = музична тональність.
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalArtist { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Key { get; set; } = string.Empty;

    // Конструктор без параметрів одразу генерує унікальний Id.
    public Song()
    {
        Id = Guid.NewGuid();
    }

    // : this() спочатку викликає попередній конструктор,
    // а потім заповнює інші властивості.
    public Song(string title, string originalArtist, int durationSeconds, string key) : this()
    {
        Title = title;
        OriginalArtist = originalArtist;
        DurationSeconds = durationSeconds;
        Key = key;
    }

    // Метод переводить тривалість із секунд у формат "хвилини:секунди".
    public string GetFormattedDuration()
    {
        // TimeSpan = проміжок часу.
        TimeSpan duration = TimeSpan.FromSeconds(DurationSeconds);

        // :00 змушує виводити секунди двома цифрами, наприклад 3:05.
        return $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
    }

    // Статичний фабричний метод.
    // FromMinutes = "створити з хвилин".
    // Метод приймає хвилини, переводить їх у секунди і повертає новий Song.
    public static Song FromMinutes(string title, string originalArtist, double minutes, string key)
    {
        return new Song(title, originalArtist, (int)Math.Round(minutes * 60), key);
    }

    // СТАТИЧНИЙ ФАБРИЧНИЙ МЕТОД CreateNew("створити новий").
    // Фабричний метод повертає готовий об'єкт із випадково вибраними,
    // але реалістичними значеннями.
    // Random.Shared є спільним потокобезпечним генератором випадкових чисел, який безпечно
    // використовують паралельні потоки.
    public static Song CreateNew()
    {
        var seeds = UkrainianMusicData.Songs;
        var selected = seeds[Random.Shared.Next(seeds.Length)];
        // Тривалість генерується незалежно для перевірки LINQ Min/Max/Average.
        return new Song(selected.Title, selected.OriginalArtist,
            Random.Shared.Next(120, 360), selected.Key);
    }
}
