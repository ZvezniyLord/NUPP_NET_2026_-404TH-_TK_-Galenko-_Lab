namespace MusicContest.Common.Services;

// Підсумок демонстрації взаємодії producer і consumer.
public readonly record struct ProducerConsumerResult(
    // Кількість елементів, яку планувалося створити.
    int PlannedCount,
    // Кількість елементів, фактично доданих до буфера.
    int ProducedCount,
    // Кількість елементів, фактично забраних із буфера.
    int ConsumedCount)
{
    // Перевіряє, чи створено й оброблено всю заплановану кількість елементів.
    public bool IsBalanced =>
        ProducedCount == PlannedCount && ConsumedCount == PlannedCount;

    // Після завершення обох завдань рівність лічильників означає,
    // що в буфері не залишилося необроблених елементів.
    public bool BufferEmpty => ProducedCount == ConsumedCount;
}
