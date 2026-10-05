namespace MusicContest.Common.Services;

// Результат producer/consumer-демонстрації.
public readonly record struct ProducerConsumerResult(
    int PlannedCount,
    int ProducedCount,
    int ConsumedCount)
{
    // Лічильники збалансовані:-produced (створено) == consumed (спожито) == план.
    public bool IsBalanced =>
        ProducedCount == PlannedCount && ConsumedCount == PlannedCount;

    // Buffer (буфер) порожній: створено стільки, скільки спожито —
    // в черзі не залишилося необроблених елементів.
    public bool BufferEmpty => ProducedCount == ConsumedCount;
}
