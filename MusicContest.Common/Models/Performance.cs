namespace MusicContest.Common.Models;

public class Performance : IEntity
{
    // Делегат
    public delegate void ScoreChangedHandler(Performance performance, double oldScore, double newScore);

    // Подія
    public event ScoreChangedHandler? ScoreChanged;

    public Guid Id { get; set; }
    public string ParticipantStageName { get; set; } = string.Empty;
    public string SongTitle { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsCompleted { get; set; }

    // Конструктор
    public Performance()
    {
        Id = Guid.NewGuid();
    }

    // Конструктор з параметрами
    public Performance(string participantStageName, string songTitle, double score) : this()
    {
        ParticipantStageName = participantStageName;
        SongTitle = songTitle;
        Score = score;
        IsCompleted = false;
    }

    // Метод
    public void SetScore(double newScore)
    {
        if (newScore is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(newScore), "Оцінка має бути від 0 до 100.");
        }

        double oldScore = Score;
        Score = newScore;

        // Виклик події
        ScoreChanged?.Invoke(this, oldScore, newScore);
    }

    // Метод
    public void Complete()
    {
        IsCompleted = true;
    }
}
