namespace MusicContest.Common.Services;

// Результат producer/consumer-демонстрації (M = plannedCount).
// Усі значення — СПРАВЖНІ, з атомарних лічильників,
// а не константи з коментаря.
public readonly record struct ProducerConsumerResult(
    // Планова кількість елементів M (з параметра-завдання).
    int PlannedCount,
    // ФАКТ: скільки елементів producer НАСПРАВДІ створив
    // (лічильник producedCount, Interlocked.Increment).
    int ProducedCount,
    // ФАКТ: скільки елементів consumer НАСПРАВДІ зняв із буфера
    // (лічильник consumedCount, Interlocked.Increment).
    int ConsumedCount)
{
    // Лічильники збалансовані (balanced): produced (створено)
    // == consumed (спожито) == план M — жоден елемент не загубився.
    public bool IsBalanced =>
        ProducedCount == PlannedCount && ConsumedCount == PlannedCount;

    // Buffer (буфер) порожній: створено стільки, скільки спожито —
    // в черзі не залишилося необроблених елементів.
    public bool BufferEmpty => ProducedCount == ConsumedCount;
}
