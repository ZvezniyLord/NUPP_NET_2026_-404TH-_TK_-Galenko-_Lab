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

    // Статичний фабричний метод CreateNew = "створити новий" із згенерованими даними.
    //
    // Random = генератор псевдовипадкових чисел.
    // Random.Shared = спільний (shared) екземпляр Random у сучасному .NET,
    // який потокобезпечний (thread-safe) і зручний для паралельного створення
    // об'єктів у різних потоках: не треба створювати власний Random для
    // кожного потоку (це класична помилка).
    //
    // Рандомні логічні значення:
    //   Title         = назва пісні (з випадкового списку);
    //   OriginalArtist = виконавець;
    //   DurationSeconds = тривалість у секундах: 120-360;
    //   Key           = музична тональність (з випадкового списку).
    public static Song CreateNew()
    {
        Random rng = Random.Shared;

        string[] titles = { "Gravity", "Midnight", "Horizon", "Breeze", "Firefly",
            "Aurora", "Delta", "Echo", "Harmony", "Iris" };
        string[] artists = { "Sara", "Mila", "Vika", "Ost", "Polo", "Rina",
            "Sofia", "Tara", "Uliana", "Vera" };
        string[] keys = { "C major", "F major", "G major", "A minor", "B minor",
            "D minor", "E major", "G minor" };

        int durationSeconds = 120 + rng.Next(241); // 120..360

        return new Song(
            titles[rng.Next(titles.Length)],
            artists[rng.Next(artists.Length)],
            durationSeconds,
            keys[rng.Next(keys.Length)]);
    }
}
