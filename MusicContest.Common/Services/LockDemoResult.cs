namespace MusicContest.Common.Services;

// Результат lock-демонстрації.
public readonly record struct LockDemoResult(
    int Expected,
    int Actual,
    bool Match);
