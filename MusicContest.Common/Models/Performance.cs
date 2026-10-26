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

    // СТАТИЧНИЙ ФАБРИЧНИЙ МЕТОД CreateNew("створити новий").
    // Фабрика — метод, який повертає готовий об'єкт із рандомними,
    // але реалістичними значеннями.
    // Random.Shared — спільна thread-safe копія Random: метод можна
    // викликати з безлічі потоків одночасно.
    public static Performance CreateNew()
    {
        // Random.Shared: безпечний для потоку instance (копія) Random.
        Random random = Random.Shared;

        return new Performance(
            $"Artist-{random.Next(1, 1000)}",    // ParticipantStageName
            $"Song-{Guid.NewGuid():N}",           // SongTitle
            Math.Round(random.NextDouble() * 100, 1) // Score: 0.0-100.0 (з округленням)
        );
    }
}
