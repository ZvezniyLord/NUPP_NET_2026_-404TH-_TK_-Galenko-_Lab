using System.Diagnostics;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Parallell — клас для паралельного створення великої кількості об'єктів
// через асинхронний CRUD-сервіс (ЛР2).
//
// Parallel (System.Threading.Tasks.Parallel) — статичний клас .NET, який
// автоматично розбиває велику задачу на менші частини та виконує їх
// ОДНОЧАСНО на багатьох потоках пулу (thread pool).
//
// Parallel.For(from, to, body) — паралельний for: тіло виконується різними
// потоками одночасно; код після Parallel.For біжить лише після завершення
// ВСІХ ітерацій (join point — точка зшивання).
//
// Parallel.ForEachAsync — паралельний перебір з АСИНХРОННИМ тілом: кожен
// елемент обробляється в await-методі, тому потоки пулу не блокуюються
// під час CreateAsync. Саме цей варіант використовується тут, бо CRUD-сервіс
// асинхронний.
public static class Parallell
{
    // Кількість об'єктів для демонстрації (за умови: не менше 1000).
    public const int DefaultObjectCount = 2000;

    /// <summary>
    /// Паралельно створює count об'єктів Song (через Song.CreateNew()) і
    /// додає їх у CRUD-сервіс service.CreateAsync. Фактичний час вимірюється
    /// Stopwatch; реальна кількість рахується з самого сервісу.
    /// </summary>
    public static async Task<ParallelCreateResult> RunAsync(
        CrudServiceAsync<Song> service,
        int count,
        TextWriter? console = null)
    {
        // Атомарний лічильник успішних створень: Interlocked.Increment —
        // безпечне зростання під конкурентними потоками (без lock).
        int created = 0;

        // Stopwatch — секундомір: StartNew() запускає, Stop() зупиняє,
        // Elapsed — фактичний (real) проміжок часу.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Parallel.ForEachAsync: ітерації 0..count-1 обробляються одночасно
        // різними потоками пулу; тіло — async (await CreateAsync).
        await Parallel.ForEachAsync(
            Enumerable.Range(0, count),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (i, cancellationToken) =>
            {
                // Новий об'єкт створюється РІЗНИМ потоком через фабричний
                // метод CreateNew() (Random.Shared — thread-safe генератор).
                Song song = Song.CreateNew();

                // Додавання в CRUD-сервіс: ConcurrentDictionary всередині
                // сервісу вже thread-safe, тому паралельні CreateAsync
                // не гублять елементи.
                bool added = await service.CreateAsync(song);

                if (added)
                {
                    Interlocked.Increment(ref created);
                }
            });

        stopwatch.Stop();

        // РЕАЛЬНА кількість — з колекції сервісу, а не з лічильника:
        // перевіряємо, що CRUD-сервіс справді зберіг усі об'єкти.
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

// Результати паралельного створення.
public sealed class ParallelCreateResult
{
    // ФАКТИЧНА кількість (з колекції CRUD-сервісу, а не з константи).
    public int ActualCount { get; }

    // Планова (з завдання) кількість.
    public int RequestedCount { get; }

    // Фактичний час, виміряний Stopwatch.
    public TimeSpan Elapsed { get; }

    public ParallelCreateResult(int actualCount, int requestedCount, TimeSpan elapsed)
    {
        ActualCount = actualCount;
        RequestedCount = requestedCount;
        Elapsed = elapsed;
    }
}
