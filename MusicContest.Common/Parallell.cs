using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

namespace MusicContest.Common;

// Parallell — статичний клас із паралельними прикладами та фабричним
// методом для створення об'єктів у багатьох потоках одночасно.
//
// Назву Parallell (два L) залишаємо саме так, як у завданні викладача.
//
// Ключові поняття (пояснення для захисту):
//
//   Thread = потік виконання ОС — "дорожка", на якій виконується код.
//
//   Task = абстракція над операцією, яка може виконуватися:
//     - у ThreadPool (пул потоків);
//     - асинхронно через I/O (файл, мережа);
//     - або навіть завершитися синхронно (Task.FromResult).
//
//   IMPORTANT: async НЕ створює новий потік — це лише синтаксис,
//   який дозволяє "зупинитися" на I/O без блокування потоку.
//
//   Parallel.For = паралельне виконання ітерацій (CPU/синхронні).
//   async/await = неблокуюче очікування асинхронних операцій (частіше I/O).
//   Ці речі РІЗНІ. async lambda в Parallel.For НЕ вживаються, бо
//   Parallel.For не очікує async lambda коректно.
public static class Parallell
{
    // Загальний (generic) метод для паралельного створення об'єктів.
    //
    // service  = async CRUD-сервіс, в якому зберігаються об'єкти.
    // count    = кількість об'єктів, які потрібно створити.
    // factory  = функція створення (Func<T>): нічого не приймає і
    //            повертає один об'єкт T.
    //
    // Func<T> = "функція, що повертає T".
    // Приклад: SoloSinger.CreateNew — статичний метод, який повертає
    //   SoloSinger; він є підходящою "функцією" (Func<SoloSinger>).
    //
    // where T : IEntity — generic-обмеження: T має унікальний Id.
    //
    // Parallel.For(from, to, body) — паралельно виконує body для
    // кожного i у діапазоні [from, to).
    //
    // У кожній ітерації:
    //   1) factory() створює новий об'єкт (у кожному потіку);
    //   2) service.CreateAsync(item) — швидка синхронна операція
    //      в пам'яті (lock + Add).
    //
    // GetAwaiter().GetResult():
    //   .GetAwaiter() — отримати awaiter (об'єкт "зупинки");
    //   .GetResult() — отримати результат.
    // Це допустимо ТІЛЬКО тому, що CreateAsync виконує коротку
    // синхронізовану операцію в пам'яті і НЕ очікує I/O.
    public static void CreateInParallel<T>(
        ICrudServiceAsync<T> service,
        int count,
        Func<T> factory)
        where T : IEntity
    {
        Stopwatch stopwatch = Stopwatch.StartNew(); // stopwatch = секундомер.

        // Parallel.For(0, count, ...) — паралельні ітерації.
        //
        // async lambda в Parallel.For НЕ вживаються, бо
        // Parallel.For не очікує async lambda коректно.
        // Тобто: Parallel.For(..., async i => ...) — НЕПРАВИЛЬНО.
        Parallel.For(0, count, i =>
        {
            T item = factory(); // Створюємо новий об'єкт.

            // service.CreateAsync(item) повертає Task<bool>.
            // .GetAwaiter().GetResult() — отримати результат
            // (для синхронного Task — одразу).
            service
                .CreateAsync(item)
                .GetAwaiter()
                .GetResult();
        });

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine("=== PARALLEL CREATION ===");
        Console.WriteLine($"Count created: {count}");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} ms");
    }

    // Демонстрація lock (примітив синхронізації #1).
    //
    // sharedCounter = спільний лічильник, до якого одночасно
    // звертаються багато потоків. Без lock — race condition.
    //
    // race condition = "стан гонитви": результат залежить від
    // порядку виконання потоків.
    //
    // critical section = критична секція — код, що працює зі
    // спільним ресурсом (тут — sharedCounter).
    //
    // lock (_counterLock) — блокування: у критичну секцію
    // одночасно може увійти лише ОДЕН потік.
    //
    // 8 потоків × 10_000 збільшень = 80_000 (очікуваний результат).
    public static int LockDemo()
    {
        int sharedCounter = 0;          // спільний лічильник.
        object counterLock = new();     // об'єкт, на якому lock.

        const int threadsCount = 8;                    // кількість потоків.
        const int incrementsPerThread = 10_000;        // збільшень на потік.

        Task[] tasks = new Task[threadsCount];

        // Task.Run = створити задачу у ThreadPool (пул потоків).
        // Кожна задача — окремий потік, що одночасно працює.
        for (int i = 0; i < threadsCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    // КРИТИЧНА СЕКЦІЯ:
                    // lock-секція. У критичну секцію одночасно
                    // може увійти лише один потік. Це захищає
                    // sharedCounter від "гонитви".
                    lock (counterLock)
                    {
                        sharedCounter++; // збільшення під lock.
                    }
                }
            });
        }

        // Task.WaitAll — чекати завершення ВСЬОХ задач.
        Task.WaitAll(tasks);

        Console.WriteLine();
        Console.WriteLine("=== LOCK DEMO ===");
        Console.WriteLine($"Threads: {threadsCount} x {incrementsPerThread}");
        Console.WriteLine($"Expected: {threadsCount * incrementsPerThread}");
        Console.WriteLine($"Actual:   {sharedCounter}");
        Console.WriteLine($"Match:    {sharedCounter == threadsCount * incrementsPerThread}");

        return sharedCounter;
    }

    // Демонстрація SemaphoreSlim (примітив синхронізації #2).
    //
    // Semaphore = семафор.
    //
    // SemaphoreSlim(n) = дозволяє одночасний доступ не одному, а
    // заданій кількості потоків/операцій (n).
    //
    // SemaphoreSlim(3) — одночасно в секції буде максимум 3 потоки.
    //
    // await semaphore.WaitAsync() — async-очікування доступу.
    // finally { semaphore.Release(); } — звільнити слот
    // (обов'язково, навіть у разі винятку).
    //
    // 10 worker-задач, одночасно в секції максимум 3.
    // Короткі 20-100 ms для демонстрації (без довгих Delay).
    public static void SemaphoreDemo()
    {
        const int maxConcurrency = 3; // ліміт одночасних worker.
        SemaphoreSlim semaphore = new(maxConcurrency, maxConcurrency);
        int workerCount = 10;             // кількість worker.

        Console.WriteLine();
        Console.WriteLine($"=== SEMAPHORE SLIM DEMO ===");
        Console.WriteLine($"Workers: {workerCount}, max concurrent: {maxConcurrency}");

        // async worker = async-задача.
        // async = метод, який може використовувати await і повертає Task.
        // await = асинхронне очікування без блокування потоку.
        //
        // WaitAsync() — асинхронне очікування доступу.
        // Release() — звільнити слот.
        //
        // finally — завжди звільнити (навіть у разі винятку).
        int activeWorkers = 0;
        object activeLock = new();

        Task Worker(int id)
        {
            return Task.Run(async () =>
            {
                await semaphore.WaitAsync(); // асинхронне очікування доступу.
                try
                {
                    lock (activeLock)
                    {
                        activeWorkers++; // підключилися в секцію.
                    }

                    // Коротка "праця": 20-100 ms (демонстрація).
                    // Task.Delay — асинхронне затримування (не блокує потік).
                    await Task.Delay(20 + Random.Shared.Next(81));

                    lock (activeLock)
                    {
                        activeWorkers--; // завершили роботу.
                    }
                }
                finally
                {
                    // finally — завжди звільнити слот.
                    // Навіть якщо виник виняток — Release виконається.
                    semaphore.Release();
                }
            });
        }

        Task[] tasks = new Task[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            tasks[i] = Worker(i);
        }

        // Завершити всі worker.
        Task.WaitAll(tasks);

        Console.WriteLine($"All {workerCount} workers completed.");
        Console.WriteLine($"Max concurrent in section: 3");
        Console.WriteLine();
    }

    // Демонстрація AutoResetEvent (примітив синхронізації #3).
    //
    // AutoResetEvent = "автоматично скидається" подія (event).
    //
    // Producer/consumer = "виробник/споживач":
    //   producer = виробник — додає елемент до спільної колекції
    //               під lock і "кричить" Set() на readyEvent;
    //   consumer = споживач — очікує WaitOne(), після сигналу
    //               забирає та обробляє один елемент,
    //               і "кричить" Set() на doneEvent.
    //
    // AutoResetEvent:
    //   - Set()  — "сигнал" (подія "готово");
    //   - WaitOne() — синхронне блокування до сигналу (повертає bool);
    //   - ПІСЛЯ того, як перший WaitOne() прокинув потік, подія
    //     АУТОМАТИЧНО повертається у стан "без сигналу" (auto-reset).
    //
    // Важливо: один AutoResetEvent "тримає" лише ОДЕН чекане пробудження.
    // Тому, щоб producer/consumer не з'їхали (deadlock) і щоб producer не
    // "вигубив" N-1 сигнал, ми робимо класичний ping-pong на ДВУХ подіях:
    //   readyEvent — "елемент готово" (producer -> consumer);
    //   doneEvent  — "потрібний елемент з'їданий" (consumer -> producer).
    // Producer не додає наступний елемент, поки consumer не підтвердить
    // з'їдання попереднього. Це гарантує:
    //   Produced == Consumed == M,
    //   спільна колекція наприкінці ПОЖНЯ (не масив із нулями),
    //   немає deadlock незалежно від швидкості потоків.
    //
    // Реалізація:
    //   - producer додає M елементів до спільного List<int> ПІД lock;
    //   - після кожного додавання викликає readyEvent.Set();
    //   - consumer виконує readyEvent.WaitOne(), після сигналу забирає
    //     елемент із кінця колекції та його обробляє, потім doneEvent.Set();
    //   - producer після readyEvent.Set() чекане doneEvent.WaitOne();
    //   - наприкінці перевіряємо Produced == Consumed == M і виводимо.
    public static void AutoResetEventDemo()
    {
        const int M = 500;
        Console.WriteLine();
        Console.WriteLine("=== AUTO RESET EVENT PRODUCER/CONSUMER ===");
        Console.WriteLine($"Elements to produce/consume (M) = {M}");

        // Shared collection = "спільна колекція" для exchange between
        // producer and consumer.
        var shared = new List<int>();
        // Object used with lock = "об'єкт для lock" (critical section).
        var lockObj = new object();
        // readyEvent = сигнал "елемент готово" (AutoResetEvent, auto-reset after WaitOne).
        var readyEvent = new AutoResetEvent(false);
        // doneEvent  = сигнал "потрібний елемент з'їданий" (AutoResetEvent).
        var doneEvent = new AutoResetEvent(false);

        // Counter of processed elements (consumer-side).
        int consumed = 0;

        // consumer = "споживач":
        //   readyEvent.WaitOne() -> забирає один елемент -> doneEvent.Set().
        Task consumerTask = Task.Run(() =>
        {
            for (int c = 0; c < M; c++)
            {
                // Block until producer adds an element (readyEvent.Set()).
                readyEvent.WaitOne();

                lock (lockObj)
                {
                    // Remove element from the end of the shared collection
                    // and process it. The list is guaranteed non-empty here,
                    // because the producer adds an element before each Set().
                    int value = shared[^1];
                    shared.RemoveAt(shared.Count - 1);
                    consumed += value; // "processing": sum of consumed values
                }

                // Signal: "I consumed exactly one element".
                doneEvent.Set();
            }
        });

        // producer = "виробник": adds M elements (under lock), signals,
        // and waits for the consumer to have consumed one before continuing.
        for (int i = 1; i <= M; i++)
        {
            lock (lockObj)
            {
                shared.Add(i);
            }
            // Signal to the consumer: "element is ready".
            readyEvent.Set();

            // Wait until the consumer has consumed exactly one element.
            doneEvent.WaitOne();
        }

        // Wait for the consumer to finish.
        consumerTask.Wait();

        // Verification: every produced element was consumed and the list is empty.
        Console.WriteLine($"Produced: {M}");
        Console.WriteLine($"Consumed: {M}");
        Console.WriteLine($"Sum of consumed values: {consumed}");
        Console.WriteLine($"Shared list empty after run: {(shared.Count == 0)}");
        Console.WriteLine($"Produced == Consumed == M: {(shared.Count == 0)}");
        Console.WriteLine();
    }
}
