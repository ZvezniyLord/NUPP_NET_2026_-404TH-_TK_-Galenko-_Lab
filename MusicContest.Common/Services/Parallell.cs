using System.Collections.Concurrent;
using System.Diagnostics;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Parallell — клас для паралельного створення великої кількості об'єктів.
//
// Parallel (System.Linq.Parallel) — статичний клас бібліотеки .NET,
// який автоматично розбиває велику задачу на менші частини та виконує їх
// ОДНОЧАСНО на багатьох потоках пулу (thread pool).
//
// Parallel.For(int from, int to, Action) — паралельний варіант for-циклу:
// тіло (body) виконується одночасно різними потоками.
// Код після Parallel.For виконується лише після завершення ВСІХ
// ітерацій (join point — точка зшивання).
public static class Parallell
{
    // Кількість об'єктів для демонстрації (не менше 1000).
    public const int DefaultObjectCount = 2000;

    /// <summary>
    /// Створює count об'єктів Song паралельно (Parallel.For) і вимірює
    /// фактичний час через Stopwatch.
    /// </summary>
    public static ParallelCreateResult Run(int count, TextWriter? console = null)
    {
        // ConcurrentBag<Song> — thread-safe (поточно-безпечна) колекція:
        // безліч потоків одночасно викликають Add без lock і без втрат даних.
        // Bag (кошик) — без підписки, без сортування: лише Add та Enumerate.
        ConcurrentBag<Song> createdSongs = new();

        // Stopwatch — секундомір: StartNew() запускає, Stop() зупиняє,
        // Elapsed — фактичний (real) проміжок часу.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Parallel.For(0, count, i => { ... }):
        // ітерації 0..count-1 виконуються одночасно різними потоками пулу.
        Parallel.For(0, count, i =>
        {
            // Random.Shared — статичний instance (копія) Random,
            // thread-safe: багато потоків одночасно читають його
            // без lock і без втрат швидкості.
            Random random = Random.Shared;

            // Новий об'єкт створюється РІЗНИМ потоком.
            Song song = new(
                $"Song-{i}",
                $"Artist-{random.Next(1, 1000)}",
                random.Next(120, 360),                            // DurationSeconds: 2-6 хвилин
                new[] { "C major", "F major", "G major", "A minor", "D minor" }
                    [random.Next(0, 5)]);                          // Key: тональність

            // Add у ConcurrentBag — безпосередньо з потоку, без lock
            // (колекція вже thread-safe).
            createdSongs.Add(song);
        });

        stopwatch.Stop();

        ParallelCreateResult result = new(
            createdSongs.Count,
            count,
            stopwatch.Elapsed,
            createdSongs);

        if (console is not null)
        {
            console.WriteLine($"Об'єктів створено: {result.ActualCount} із {result.RequestedCount} планових");
            console.WriteLine($"Фактичний час: {result.Elapsed.TotalMilliseconds:F3} ms");
        }

        return result;
    }
}

// Результати паралельного створення.
// Зберігаємо РЕАЛЬНО створені об'єкти, щоб Program.cs (та тести)
// мали до них доступ без повторного запуску.
public sealed class ParallelCreateResult
{
    // ФАКТИЧНА (з колекції, а не з константи) кількість створених.
    public int ActualCount { get; }

    // Планова (з завдання) кількість.
    public int RequestedCount { get; }

    // Фактичний час, виміряний Stopwatch.
    public TimeSpan Elapsed { get; }

    // РЕАЛЬНО створені об'єкти (незалежна копія).
    public IReadOnlyList<Song> Songs { get; }

    public ParallelCreateResult(
        int actualCount,
        int requestedCount,
        TimeSpan elapsed,
        IEnumerable<Song> songs)
    {
        ActualCount = actualCount;
        RequestedCount = requestedCount;
        Elapsed = elapsed;
        Songs = songs.ToList();
    }
}
