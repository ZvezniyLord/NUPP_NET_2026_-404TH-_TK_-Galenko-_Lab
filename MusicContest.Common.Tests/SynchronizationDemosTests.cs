using MusicContest.Common.Services;
using Xunit;

namespace MusicContest.Common.Tests;

// Тести на механізми синхронізації: lock, SemaphoreSlim,
// AutoResetEvent (producer/consumer).
public class SynchronizationDemosTests
{
    // ============================================================
    // LOCK-демонстрація: 8 потоків × 10 000 increment = 80 000.
    // ============================================================

    // Тест: lock реально синхронізує. Очікуване значення
    // threadCount × incrementsPerThread (число з коментаря-завдання,
    // а не з лічильника), фактичне — лічильник після завершення.
    [Fact]
    public async Task LockDemo_ActualMatchesExpectedAfterParallelIncrements()
    {
        const int threadCount = 8;
        const int incrementsPerThread = 10_000;

        // Результат з лічильника (а не з коментаря).
        LockDemoResult result = await SynchronizationDemos.RunLockDemoAsync(
            threadCount,
            incrementsPerThread,
            console: null);

        // Expected = 8 × 10_000 (заплановано) і Actual
        // (з лічильника, а не з константи).
        int expected = threadCount * incrementsPerThread;

        Assert.Equal(expected, result.Actual);
        Assert.True(result.Match);
    }

    // ============================================================
    // SEMAPHORESLIM-демонстрація: 10 worker'ів, maxConcurrency = 3.
    // ============================================================

    // Тест: семафор РЕАЛЬНО ОБМЕЖУЄ одночасність:
    // виміряний максимум НЕ перевищує заявленого maxConcurrency.
    // Якби семафор не працював, максимум став би 10.
    [Fact]
    public async Task SemaphoreDemo_MeasuredMaxDoesNotExceedLimit()
    {
        const int workerCount = 10;
        const int maxConcurrency = 3;

        SemaphoreDemoResult result = await SynchronizationDemos.RunSemaphoreDemoAsync(
            workerCount,
            maxConcurrency,
            console: null);

        // Межа — НЕ "надрукована", а ФАКТИЧНИЙ (з лічильника) максимум:
        // він НЕ перевищує заявлений maxConcurrency.
        Assert.True(result.MeasuredMaxConcurrency <= result.MaxConcurrencySetting,
            $"Виміряний максимум {result.MeasuredMaxConcurrency} перевищує ліміт {result.MaxConcurrencySetting}.");
    }

    // ============================================================
    // AUTORESETEVENT producer/consumer — M = 500.
    // ============================================================

    // Тест: producer НАСПРАВДІ (потік) створює M = 500 елементів,
    // consumer НАСПРАВДІ (потік) обробляє M = 500 елементів:
    // Produced == Consumed == 500 (не "надруковано" 500,
    // а з лічильників лічильників).
    [Fact]
    public async Task ProducerConsumer_ProducedEqualsConsumedEqualsM()
    {
        const int plannedM = 500;

        ProducerConsumerResult result =
            await SynchronizationDemos.RunProducerConsumerDemoAsync(plannedM, console: null);

        // Лічильники: producedCount (створено) та consumedCount (спожито).
        Assert.Equal(plannedM, result.ProducedCount);
        Assert.Equal(plannedM, result.ConsumedCount);
        // producedCount == consumedCount == M.
        Assert.True(result.IsBalanced);
        // Buffer (буфер) порожній (не залишилося необроблених елементів).
        Assert.True(result.BufferEmpty);
    }
}
