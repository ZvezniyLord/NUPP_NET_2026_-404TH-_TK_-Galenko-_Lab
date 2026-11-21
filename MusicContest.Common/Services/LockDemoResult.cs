namespace MusicContest.Common.Services;

// Результат демонстрації lock. Містить очікуване й фактичне значення лічильника.
// readonly record struct є незмінним типом значення.
public readonly record struct LockDemoResult(
    // Кількість збільшень, яку задають параметри запуску.
    int Expected,
    // Значення лічильника після завершення всіх робочих завдань.
    int Actual,
    // True, якщо фактичне значення відповідає очікуваному.
    bool Match);
