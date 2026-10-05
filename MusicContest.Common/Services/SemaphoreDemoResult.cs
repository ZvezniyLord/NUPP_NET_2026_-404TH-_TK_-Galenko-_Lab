namespace MusicContest.Common.Services;

// Результат SemaphoreSlim-демонстрації.
// readonly record struct — незмінний (immutable) результат,
// який передається всім потокам безпосередньо.
public readonly record struct SemaphoreDemoResult(
    // Скільки «ліцензій» (maxConcurrency) ЗАЯВЛЕНО у завданні —
    // число з параметра, а не з вимірювання.
    int MaxConcurrencySetting,
    // ФАКТ (з атомарного лічильника): скільки worker'ів реально
    // було одночасно всередині обмеженої ділянки під час запуску.
    int MeasuredMaxConcurrency);
