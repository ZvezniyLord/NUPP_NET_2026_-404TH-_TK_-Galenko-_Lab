using System.Collections.Concurrent;
using System.Threading;

namespace MusicContest.Common.Services;

// Три демонстрації механізмів синхронізації:
// 1) lock; 2) SemaphoreSlim; 3) AutoResetEvent (справжня producer/consumer).
// Кожна демонстрація повертає СПРАВЖНІ (real) значення, зібрані з
// атомарних лічильників та колекцій, а не "надруковані" числа
// з коментаря.
public static class SynchronizationDemos
{
    // ============================================================
    // 1) LOCK-демонстрація: 8 потоків × 10 000 зростань.
    // ============================================================
    // Є СПІЛЬНА (shared) змінна counter і 8 потоків, кожен з яких
    // виконує 10 000 разів:
    //     counter++;
    // Оператор ++ складається з трьох кроків: read-modify-write
    // (прочитати — змінити — записати). Без синхронізації два потоки
    // можуть одночасно прочитати одне й те саме значення, і одне
    // зростання "губиться" (race condition — гоніжка потоків).
    // lock (gate) — взаємне виключення (mutual exclusion):
    // в критичній секції одночасно ОДИН потік, інші чекають.
    public static async Task<LockDemoResult> RunLockDemoAsync(
        int threadCount,
        int incrementsPerThread,
        TextWriter? console = null)
    {
        // Спільний (shared) лічильник — саме ця змінна є об'єктом
        // демонстрації lock.
        int counter = 0;

        // object gate — об'єкт-ключ блоку: lock (gate) = "увійди в
        // критичну секцію, якщо ключ вільний; інакше чекати".
        object gate = new();

        async Task WorkerAsync()
        {
            for (int i = 0; i < incrementsPerThread; i++)
            {
                // КРИТИЧНА СЕКЦІЯ (critical section):
                // зростання під lock, тобто атомарна дія.
                lock (gate)
                {
                    counter++;
                }

                // await Task.Yield() — потік пулу віддає керування
                // і знову продовжує в пулі: потіки перемінюються,
                // і lock справді тримає критичну секцію.
                await Task.Yield();
            }
        }

        // 8 потоків запускаємо одночасно (Task.Run — у пулі потоків).
        Task[] workers = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            workers[i] = WorkerAsync();
        }

        // Join (зшивання): чекаємо, доки ВСІ потоки не закінчаться.
        await Task.WhenAll(workers);

        int expected = threadCount * incrementsPerThread;
        int actual = counter; // ФАКТИЧНЕ значення лічильника.

        if (console is not null)
        {
            console.WriteLine($"Expected: {expected}");
            console.WriteLine($"Actual:   {actual}");
            console.WriteLine($"Match:    {actual == expected}");
        }

        return new LockDemoResult(expected, actual, actual == expected);
    }

    // ============================================================
    // 2) SEMAPHORESLIM-демонстрація: 10 worker'ів, maxConcurrency = 3.
    // ============================================================
    // SemaphoreSlim(max) — семафор-лічильник з max "ліцензіями".
    // WaitAsync "забирає" ліцензію, Release — повертає її.
    // Не більше max потоків одночасно перебуває в ділянці,
    // обмеженій WaitAsync/Release. Очікування — асинхронне
    // (await, без блокування потоку).
    // Ми НЕ пишемо максимум вручну: вимірюємо його атомарним
    // лічильником одночасно-активних worker'ів (Interlocked)
    // — саме це є фактичним результатом.
    public static async Task<SemaphoreDemoResult> RunSemaphoreDemoAsync(
        int workerCount,
        int maxConcurrency,
        TextWriter? console = null)
    {
        // Семафор з maxConcurrency ліцензіями.
        SemaphoreSlim semaphore = new(maxConcurrency);

        // Лічильник поточно-активних worker'ів.
        // Interlocked — атомарні операції з цілими числами:
        // зростання та запис без втрат (без lock, але thread-safe).
        int activeWorkers = 0;
        // Виміряний (measured) максимум одночасної активності.
        int measuredMax = 0;

        async Task WorkerAsync(int index)
        {
            // Зайняти одну ліцензію; якщо немає — асинхронно чекати.
            await semaphore.WaitAsync();

            try
            {
                // Worker "всередині": атомарно збільшити
                // активний лічильник.
                // Interlocked.Increment атомарно повертає нове значення.
                int current = Interlocked.Increment(ref activeWorkers);

                // Якщо current перевищує раніше виміряний максимум —
                // записати новий максимум атомарно
                // (CompareExchange: "застосувати, якщо все ще однакове").
                int previous = measuredMax;
                while (current > previous)
                {
                    // previous (колишнє) значення — результат порівняння.
                    previous = Interlocked.CompareExchange(
                        ref measuredMax, current, previous);
                    // previous == current → запис відбувся (break).
                    if (previous == current)
                    {
                        break;
                    }
                }

                // Пауза: симуляція роботи (IO, API, обчислення).
                await Task.Delay(10);
            }
            finally
            {
                // Worker завершує: зменшити активний лічильник
                // і повернути ліцензію семафору.
                Interlocked.Decrement(ref activeWorkers);
                semaphore.Release();
            }
        }

        // Запускаємо 10 worker'ів одночасно.
        Task[] workers = new Task[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            workers[i] = WorkerAsync(i);
        }

        // Join (зшивання): чекаємо завершення всіх worker'ів.
        await Task.WhenAll(workers);

        if (console is not null)
        {
            console.WriteLine($"Worker'ів: {workerCount}; зазначено maxConcurrency = {maxConcurrency}");
            console.WriteLine($"ФАКТИЧНО виміряно максимум одночасних worker'ів: {measuredMax}");
        }

        return new SemaphoreDemoResult(maxConcurrency, measuredMax);
    }

    // ============================================================
    // 3) AUTORESETEVENT — справжня PRODUCER/CONSUMER-демонстрація.
    // ============================================================
    // AutoResetEvent — подія автоскидання (auto reset):
    // Set() пробуджує ТОЛЬКИ ОДИН потік, який чекає WaitOne(),
    // і одразу повертається в стан очікування.
    //
    // Архітектура (architecture):
    //   Producer ("виробник") створює M елементів, додає їх
    //   у ConcurrentQueue<int> (спільний буфер) і на КОЖЕН
    //   елемент викликає itemReady.Set();
    //   Consumer ("споживач") чекає WaitOne() на itemReady і
    //   потім РЕАЛЬНО знімає елементи з буфера (TryDequeue)
    //   та обробляє їх (збільшує consumedCount).
    //
    // DEADLOCK (мертве блокування) — коли два потоки чекають
    // один на одного, і жоден не може виконати код.
    // Deadlock У НАС НЕВОЗМОЖЛИВИЙ:
    //   - producer НЕ чекає ні на що (ні на consumer, ні на буфер);
    //   - consumer чекає ТОЛЬКИ на producer, який обов'язково
    //     викликає producerFinished.Set();
    //   - після останнього Set() consumer бачить порожній
    //     буфер і завершується.
    public static async Task<ProducerConsumerResult> RunProducerConsumerDemoAsync(
        int itemCount,
        TextWriter? console = null)
    {
        // Буфер: ConcurrentQueue — thread-safe черга (queue).
        // Enqueue (вставляти) з одного боку,
        // TryDequeue (знімати) з іншого боку.
        ConcurrentQueue<int> buffer = new();

        // AutoResetEvent №1: "елемент у буфері, обробіть його".
        AutoResetEvent itemReady = new(false);

        // AutoResetEvent №2: "producer завершив УСЮ роботу".
        AutoResetEvent producerFinished = new(false);

        // СПРАВЖНІ (real) лічильники: стартують з 0,
        // а не з itemCount. itemCount (M) з'являється
        // лише як план у звіті та в перевірці наприкінці.
        int producedCount = 0;
        int consumedCount = 0;

        // === PRODUCER: виробляє елементи та додає їх у буфер ===
        // Звичайна (не async) локальна функція: виконується
        // на потоці пулу через Task.Run(ProducerAsync).
        void ProducerAsync()
        {
            for (int i = 0; i < itemCount; i++)
            {
                // "Дані" (data), які producer виробляє.
                int data = i * 2;

                // Додаємо в буфер (без lock: ConcurrentQueue
                // сам по собі thread-safe).
                buffer.Enqueue(data);

                // Producer НАСПРАВДІ створив елемент:
                // атомарне зростання лічильника.
                Interlocked.Increment(ref producedCount);

                // Сигнал (notify) consumer'у: Set() пробуджує ОДИН
                // потік, що чекає WaitOne().
                itemReady.Set();
            }

            // Producer завершив: сигнал "все вироблено".
            producerFinished.Set();
        }

        // === CONSUMER: чекає сигнали та РЕАЛЬНО обробляє елементи ===
        // Звичайна (не async) локальна функція: виконується
        // на потоці пулу через Task.Run(ConsumerAsync).
        // WaitOne() блокує ТОЛЬКИ потік пулу (не UI-потік),
        // тому deadlock-а немає: producer не чекає на consumer.
        void ConsumerAsync()
        {
            while (true)
            {
                // WaitOne(): потік очікує, доки producer не
                // викликає Set().
                itemReady.WaitOne();

                // Знімаємо ВСІ елементи, які є в буфері,
                // і НАСПРАВДІ їх обробляємо (збільшуємо лічильник).
                while (buffer.TryDequeue(out int data))
                {
                    // Обробка: використання даних,
                    // наданих producer'ом.
                    Interlocked.Increment(ref consumedCount);
                }

                // WaitOne(0) — "нульовий" (неблокуючий) запит:
                // перевіряємо, чи producer уже завершив.
                bool producerDone = producerFinished.WaitOne(0);

                // Якщо producer закінчив і буфер порожній —
                // consumer виходить із циклу (break).
                if (producerDone && buffer.IsEmpty)
                {
                    break;
                }
            }
        }

        // Запускаємо producer та consumer в пулі потоків
        // (Task.Run — кожна функція на своєму потоці пулу)
        // і чекаємо їх завершення (join — зшивання).
        await Task.WhenAll(
            Task.Run(ProducerAsync),
            Task.Run(ConsumerAsync));

        // РЕАЛЬНІ (real) лічильники: значення з
        // атомарних лічильників, а не константи
        // з коментаря.
        int finalProduced = Volatile.Read(ref producedCount);
        int finalConsumed = Volatile.Read(ref consumedCount);

        var result = new ProducerConsumerResult(
            itemCount,
            finalProduced,
            finalConsumed);

        if (console is not null)
        {
            console.WriteLine($"Produced:  {finalProduced}");
            console.WriteLine($"Consumed:  {finalConsumed}");
            console.WriteLine($"Produced == Consumed == M: {result.IsBalanced}");
            console.WriteLine($"Buffer empty: {result.BufferEmpty}");
        }

        return result;
    }
}
