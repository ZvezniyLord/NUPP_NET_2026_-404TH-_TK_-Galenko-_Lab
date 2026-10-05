namespace MusicContest.Common.Services;

// Результат SemaphoreSlim-демонстрації.
public readonly record struct SemaphoreDemoResult(
    int MaxConcurrencySetting,
    int MeasuredMaxConcurrency);
