namespace MusicContest.Common.Services;

// Результат вимірювання паралельності в демонстрації SemaphoreSlim.
public readonly record struct SemaphoreDemoResult(
    // Максимальна кількість одночасних завдань, задана під час запуску.
    int MaxConcurrencySetting,
    // Найбільша кількість завдань, одночасно зафіксована лічильником.
    int MeasuredMaxConcurrency);
