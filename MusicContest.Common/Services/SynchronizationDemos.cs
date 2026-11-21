using System.Collections.Concurrent;
using System.Threading;

namespace MusicContest.Common.Services;

// У класі зібрано три демонстрації синхронізації:
// lock, SemaphoreSlim і AutoResetEvent.
// Результати обчислюються під час запуску та беруться з лічильників і колекцій.

public static class SynchronizationDemos
{
    // 1. lock захищає спільний лічильник від одночасного оновлення.
    // Без синхронізації операція counter++ може втратити одне з оновлень.
    // У критичній секції lock дозволяє працювати лише одному потоку.

    public static async Task<LockDemoResult> RunLockDemoAsync(
        int threadCount,
        int incrementsPerThread,
        TextWriter? console = null)
    {
        // Усі робочі завдання змінюють цей лічильник.
        int counter = 0;

        // Приватний об'єкт використовується як маркер блокування.
        object gate = new();

        async Task WorkerAsync()
        {
            for (int i = 0; i < incrementsPerThread; i++)
            {
                // Збільшення лічильника захищено блокуванням.
                lock (gate)
                {
                    counter++;
                }

                // Task.Yield() планує продовження асинхронно.
                await Task.Yield();
            }
        }

        // Створюємо завдання для всіх робочих циклів.
        Task[] workers = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            workers[i] = WorkerAsync();
        }

        // Очікуємо завершення всіх робочих циклів.
        await Task.WhenAll(workers);

        int expected = threadCount * incrementsPerThread;
        int actual = counter; // фактичне значення лічильника.

        if (console is not null)
        {
            console.WriteLine($"Expected: {expected}");
            console.WriteLine($"Actual:   {actual}");
            console.WriteLine($"Match:    {actual == expected}");
        }

        return new LockDemoResult(expected, actual, actual == expected);
    }

    // 2. SemaphoreSlim обмежує кількість завдань, які одночасно
    // перебувають у захищеній ділянці. Тут встановлено ліміт 3.
    // Фактичний максимум вимірюється за допомогою Interlocked.

    public static async Task<SemaphoreDemoResult> RunSemaphoreDemoAsync(
        int workerCount,
        int maxConcurrency,
        TextWriter? console = null)
    {
        // Кожен дозвіл дає змогу одному завданню увійти в захищену ділянку.
        SemaphoreSlim semaphore = new(maxConcurrency);

        // Лічильник кількості завдань, які зараз працюють у секції.
        int activeWorkers = 0;
        // Виміряний (measured) максимум одночасної активності.
        int measuredMax = 0;

        async Task WorkerAsync(int index)
        {
            // Очікує вільний дозвіл. Очікування не блокує потік.
            await semaphore.WaitAsync();

            try
            {
                // Фіксує вхід завдання до захищеної ділянки.
                int current = Interlocked.Increment(ref activeWorkers);

                // Оновлює виміряний максимум атомарним порівнянням.
                int previous = measuredMax;
                while (current > previous)
                {
                    // CompareExchange повертає попереднє значення.
                    previous = Interlocked.CompareExchange(
                        ref measuredMax, current, previous);
                    // previous == current → запис відбувся (break).
                    if (previous == current)
                    {
                        break;
                    }
                }

                // Коротка затримка імітує роботу завдання.
                await Task.Delay(10);
            }
            finally
            {
                // Зменшує лічильник і повертає дозвіл семафору.
                Interlocked.Decrement(ref activeWorkers);
                semaphore.Release();
            }
        }

        // Створює задану кількість робочих завдань.
        Task[] workers = new Task[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            workers[i] = WorkerAsync(i);
        }

        // Очікує завершення всіх завдань.
        await Task.WhenAll(workers);

        if (console is not null)
        {
            console.WriteLine($"Завдань: {workerCount}; задано maxConcurrency = {maxConcurrency}");
            console.WriteLine($"Виміряний максимум одночасних завдань: {measuredMax}");
        }

        return new SemaphoreDemoResult(maxConcurrency, measuredMax);
    }

    // 3. AutoResetEvent передає сигнали між виробником і споживачем.
    // Producer додає елементи до ConcurrentQueue та повідомляє про них.
    // Consumer очікує сигнал, забирає доступні елементи й підраховує їх.
    // Після завершення виробник надсилає окремий сигнал, а також будить
    // consumer через itemReady, щоб той міг перевірити порожній буфер.

    public static async Task<ProducerConsumerResult> RunProducerConsumerDemoAsync(
        int itemCount,
        TextWriter? console = null)
    {
        // ConcurrentQueue є потокобезпечною чергою для передавання даних.
        ConcurrentQueue<int> buffer = new();

        // Сигналізує, що в черзі з'явилися дані або виробник завершив роботу.
        AutoResetEvent itemReady = new(false);

        // Повідомляє споживачу про завершення виробництва всіх елементів.
        AutoResetEvent producerFinished = new(false);

        // Лічильники фактично створених і оброблених елементів.
        int producedCount = 0;
        int consumedCount = 0;

        // Producer створює елементи та додає їх до спільної черги.
        void ProducerAsync()
        {
            for (int i = 0; i < itemCount; i++)
            {
                // Для прикладу кожен елемент містить парне число.
                int data = i * 2;

                // Додає елемент до потокобезпечної черги.
                buffer.Enqueue(data);

                // Фіксує створення елемента.
                Interlocked.Increment(ref producedCount);

                // Повідомляє споживача про появу даних.
                itemReady.Set();
            }

            // Повідомляє про завершення та будить consumer,
            // щоб він перевірив, чи залишилися необроблені елементи.
            producerFinished.Set();
            itemReady.Set();
        }

        // Consumer очікує сигнали й забирає доступні елементи з черги.
        void ConsumerAsync()
        {
            while (true)
            {
                // Очікує сигнал від виробника.
                itemReady.WaitOne();

                // Обробляє всі елементи, які на цей момент є в черзі.
                while (buffer.TryDequeue(out int data))
                {
                    // Фіксує обробку одного елемента.
                    Interlocked.Increment(ref consumedCount);
                }

                // Перевіряє, чи виробник уже завершив роботу.
                bool producerDone = producerFinished.WaitOne(0);

                // Завершує цикл, коли виробництво завершено й черга порожня.
                if (producerDone && buffer.IsEmpty)
                {
                    break;
                }
            }
        }

        // Запускає обидві функції та очікує завершення кожної.
        await Task.WhenAll(
            Task.Run(ProducerAsync),
            Task.Run(ConsumerAsync));

        // Зчитує підсумкові значення атомарних лічильників.
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
