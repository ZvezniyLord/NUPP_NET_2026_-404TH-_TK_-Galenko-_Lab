using System.Collections.Concurrent;
using System.Diagnostics;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Клас Parallell демонструє паралельне створення об'єктів і їх додавання
// до асинхронного CRUD-сервісу в межах ЛР2.
//
// Parallel.For розподіляє ітерації між доступними потоками виконання.
// Метод повертається після завершення всіх ітерацій.
//
// Усередині циклу викликається CreateAsync, а отримані Task<bool>
// збираються в ConcurrentBag. Після циклу Task.WhenAll очікує завершення
// всіх операцій. Це очікування не блокує потік, який викликав RunAsync.
//
// У цій реалізації CreateAsync змінює ConcurrentDictionary синхронно,
// а Task.Yield планує асинхронне продовження. Файлове асинхронне
// введення-виведення реалізовано окремо в SaveAsync.
public static class Parallell
{
    // Кількість об'єктів для демонстрації (за умови: не менше 1000).
    public const int DefaultObjectCount = 2000;

    /// <summary>
    /// Створює count об'єктів Song через Song.CreateNew(), запускає їх
    /// додавання до сервісу та очікує завершення всіх операцій через
    /// Task.WhenAll. Час вимірюється Stopwatch, кількість перевіряється
    /// за результатом ReadAllAsync().
    /// </summary>
    public static async Task<ParallelCreateResult> RunAsync(
        CrudServiceAsync<Song> service,
        int count,
        TextWriter? console = null)
    {
        // ConcurrentBag<Task<bool>> збирає завдання з різних ітерацій.
        // Колекція підтримує конкурентне додавання без окремого lock.
        ConcurrentBag<Task<bool>> addTasks = new();

        // Stopwatch вимірює час створення об'єктів і запуску операцій додавання.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Ітерації Parallel.For можуть виконуватися паралельно.
        // Кожна з них створює пісню та запускає її додавання до сервісу.
        Parallel.For(0, count, i =>
        {
            Song song = Song.CreateNew();

            // TryAdd виконується під час виклику CreateAsync.
            // Task.Yield() у сервісі планує продовження асинхронно.
            addTasks.Add(service.CreateAsync(song));
        });

        // Після завершення всіх ітерацій очікуємо створені завдання.
        // Task.WhenAll завершується, коли завершено кожне з них.
        bool[] addedResults = await Task.WhenAll(addTasks);

        stopwatch.Stop();

        // Перевіряємо результати додавання перед підрахунком елементів.
        if (addedResults.Any(added => !added))
        {
            throw new InvalidOperationException("Не всі об'єкти вдалося додати до CRUD-сервісу.");
        }

        // Окремо читаємо кількість елементів із самого сервісу.
        int actualCount = (await service.ReadAllAsync()).Count();

        ParallelCreateResult result = new(actualCount, count, stopwatch.Elapsed);

        if (console is not null)
        {
            console.WriteLine($"Об'єктів створено: {result.ActualCount} із {result.RequestedCount} планових");
            console.WriteLine($"Фактичний час: {result.Elapsed.TotalMilliseconds:F3} ms");
        }

        return result;
    }
}

// Результати демонстрації паралельного створення.
public sealed class ParallelCreateResult
{
    // Кількість елементів, яку повернув CRUD-сервіс.
    public int ActualCount { get; }

    // Кількість об'єктів, яку передали для створення.
    public int RequestedCount { get; }

    // Час виконання, виміряний за допомогою Stopwatch.
    public TimeSpan Elapsed { get; }

    public ParallelCreateResult(int actualCount, int requestedCount, TimeSpan elapsed)
    {
        ActualCount = actualCount;
        RequestedCount = requestedCount;
        Elapsed = elapsed;
    }
}
