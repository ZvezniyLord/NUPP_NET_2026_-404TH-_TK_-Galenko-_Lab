using System.Collections.Concurrent;
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
// Чому тіло не блокує потік на await: всередині Parallel.For ми лише
// ЗАПУСКАЄМО асинхронні CreateAsync (повернений Task зберігаємо у
// ConcurrentBag<Task>), а чекаємо на них ОДНАЧАСНО через Task.WhenAll
// ПІСЛЯ join point. Блокування GetAwaiter().GetResult() всередині тіла
// могло б «з'їсти» потіки пулу (thread starvation — виснаження пулу),
// саме тому тут використовується await-зшивання (Task.WhenAll), а не блокуюче очікування.
public static class Parallell
{
    // Кількість об'єктів для демонстрації (за умови: не менше 1000).
    public const int DefaultObjectCount = 2000;

    /// <summary>
    /// Parallel.For паралельно створює count об'єктів Song (через
    /// Song.CreateNew()) і запускає їх додавання у CRUD-сервіс
    /// service.CreateAsync. Після join point усі додавання зшиваються
    /// через Task.WhenAll. Час вимірює Stopwatch; реальна кількість
    /// зчитується з колекції самого сервісу.
    /// </summary>
    public static async Task<ParallelCreateResult> RunAsync(
        CrudServiceAsync<Song> service,
        int count,
        TextWriter? console = null)
    {
        // ConcurrentBag<Task<bool>> — thread-safe збір запущених Tasks:
        // різні потоки пулу одночасно кладуть у нього завдання без lock.
        ConcurrentBag<Task<bool>> addTasks = new();

        // Stopwatch — секундомір: StartNew() запускає, Stop() зупиняє,
        // Elapsed — фактичний (real) проміжок часу створення + додавання.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Parallel.For(0, count, body): ітерації 0..count-1 виконуються
        // одночасно різними потоками пулу. Тіло:
        //   1) створює об'єкт фабричним CreateNew() (Random.Shared — thread-safe);
        //   2) запускає асинхронне додавання у CRUD-сервіс і зберігає Task.
        Parallel.For(0, count, i =>
        {
            Song song = Song.CreateNew();

            // CreateAsync не блокує потік: TryAdd у ConcurrentDictionary
            // (вбудована thread-safe колекція) виконується одразу, решта —
            // асинхронне «очікування», потік пулу залишається вільним.
            addTasks.Add(service.CreateAsync(song));
        });

        // JOIN POINT: Parallel.For завершив усі ітерації. Тепер одночасно
        // чекаємо на ВСІ запущені додавання (Task.WhenAll — зшивання задач).
        bool[] addedResults = await Task.WhenAll(addTasks);

        stopwatch.Stop();

        // Скільки CreateAsync повернули true (уся кількість має бути повною:
        // Guid.Id у CreateNew унікальний, duplicate неможливий).
        int created = addedResults.Count(added => added);

        // РЕАЛЬНА кількість — з колекції сервісу: перевіряємо, що жоден
        // елемент не загубився під конкурентним додаванням.
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
