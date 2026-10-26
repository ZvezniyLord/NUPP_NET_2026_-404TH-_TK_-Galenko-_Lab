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
    // Фабрика — метод, який повертає готовий об'єкт із рандомними,
    // але реалістичними значеннями.
    // Random.Shared — спільна thread-safe копія Random, яку безпечно
    // використовують паралельні потоки.
    public static Song CreateNew()
    {
        // Random.Shared: безпечний для потоку instance (копія) Random.
        Random random = Random.Shared;

        return new Song(
            $"Song-{Guid.NewGuid():N}",             // унікальна назва
            $"Artist-{random.Next(1, 1000)}",        // оригінальний виконавець
            random.Next(120, 360),                   // тривалість: 2-6 хвилин
            new[] { "C major", "F major", "G major", "A minor", "D minor" }
                [random.Next(0, 5)]);                // тональність
    }
}
