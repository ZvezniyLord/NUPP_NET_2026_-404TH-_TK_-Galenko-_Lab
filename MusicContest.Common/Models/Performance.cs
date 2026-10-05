namespace MusicContest.Common.Models;

// Performance = виступ учасника.
public class Performance : IEntity
{
    // Делегат — тип, який описує сигнатуру методу.
    // У нашому випадку метод-обробник повинен отримати:
    // сам виступ, стару оцінку та нову оцінку.
    public delegate void ScoreChangedHandler(Performance performance, double oldScore, double newScore);

    // Подія (event) створена на основі делегата.
    // Інші частини програми можуть підписатися на неї через +=
    // і отримати повідомлення, коли оцінка зміниться.
    public event ScoreChangedHandler? ScoreChanged;

    // ParticipantStageName = сценічне ім'я учасника;
    // SongTitle = назва пісні;
    // Score = оцінка;
    // IsCompleted = чи завершено виступ.
    public Guid Id { get; set; }
    public string ParticipantStageName { get; set; } = string.Empty;
    public string SongTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsCompleted { get; set; }

    // Конструктор без параметрів генерує Id.
    public Performance()
    {
        Id = Guid.NewGuid();
    }

    // Конструктор з параметрами.
    // : this() гарантує, що Id буде створено й тут.
    public Performance(string participantStageName, string songTitle, double score) : this()
    {
        ParticipantStageName = participantStageName;
        SongTitle = songTitle;
        Score = score;
        IsCompleted = false;
    }

    // SetScore = встановити оцінку.
    // Окремий метод дає змогу перевірити допустимий діапазон і викликати подію.
    public void SetScore(double newScore)
    {
        // pattern "is < 0 or > 100" означає: менше 0 АБО більше 100.
        if (newScore is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(newScore), "Оцінка має бути від 0 до 100.");
        }

        double oldScore = Score;
        Score = newScore;

        // ?.Invoke означає: викликати подію тільки якщо на неї хтось підписаний.
        // this = поточний об'єкт Performance.
        ScoreChanged?.Invoke(this, oldScore, newScore);
    }

    // Complete = завершити виступ.
    public void Complete()
    {
        IsCompleted = true;
    }

    // Статичний фабричний метод CreateNew = "створити новий" із згенерованими даними.
    //
    // Random = генератор псевдовипадкових чисел.
    // Random.Shared = спільний (shared) екземпляр Random у сучасному .NET,
    // який потокобезпечний (thread-safe) і зручний для паралельного створення
    // об'єктів у різних потоках.
    //
    // Рандомні логічні значення:
    //   ParticipantStageName = сценічне ім'я (з випадкового списку);
    //   SongTitle            = назва пісні (з випадкового списку);
    //   Score                = оцінка: 0-100;
    //   IsCompleted          = false (виступ ще не завершений).
    public static Performance CreateNew()
    {
        Random rng = Random.Shared;

        string[] performers = { "Astra", "Nordwind", "Sirena", "Borealis", "Eklipsa",
            "Halcyon", "Lyra", "Melodia", "Vestra", "Zorya" };
        string[] songs = { "Gravity", "Midnight", "Horizon", "Breeze", "Firefly",
            "Aurora", "Delta", "Echo", "Harmony", "Iris" };

        // Score = оцінка: 0-100. NextDouble() -> [0,1), помножили на 100 -> [0,100).
        double score = rng.NextDouble() * 100;

        return new Performance(
            performers[rng.Next(performers.Length)],
            songs[rng.Next(songs.Length)],
            score);
    }
}
