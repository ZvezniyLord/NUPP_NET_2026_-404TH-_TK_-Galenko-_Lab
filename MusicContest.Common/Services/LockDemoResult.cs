namespace MusicContest.Common.Services;

// Результат lock-демонстрації: «планове» vs «фактичне» значення лічильника.
// struct (структура) — дані живуть на стеку (stack) і копіюються
// дешево; readonly — поля не можна змінити після створення об'єкта.
public readonly record struct LockDemoResult(
    // Планова кількість зростань: threadCount × incrementsPerThread
    // (число з коментаря-завдання, а НЕ з лічильника).
    int Expected,
    // ФАКТИЧНЕ значення лічильника після завершення всіх потоків
    // (з лічильника, а не з константи).
    int Actual,
    // Чи збігується факт із планом (match — збіг).
    bool Match);
