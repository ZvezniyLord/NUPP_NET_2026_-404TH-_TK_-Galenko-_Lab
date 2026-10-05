using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// АСІНХРОННИЙ (async) generic-контракт CRUD-сервісу.
//
// Асинхронність: метод не повертає самі дані напряму,
// а повертає Task<T> — «обіцянку»: «дані з'являться потім».
// Викликаючий код чекає на await, і чекає БЕЗ БЛОКУВАННЯ потоку:
// потік пулу (thread pool) за це час може виконувати іншу роботу.
//
// Зауваження про IEnumerable<T>:
// ICrudServiceAsync<T> успадковує інтерфейс IEnumerable<T>.
// Це означає, що об'єкт сервісу можна безпосередньо ітерувати
// циклом foreach:
//
//     foreach (ContestParticipant item in service) { ... }
//
// Забезпечити таку поведінку повинна реалізація (GetEnumerator).
public interface ICrudServiceAsync<T> : IEnumerable<T> where T : IEntity
{
    // CREATE: додає елемент у внутрішню колекцію.
    // Якщо елемент із таким самим Id вже існує — InvalidOperationException.
    Task CreateAsync(T element);

    // READ: повертає елемент за його Guid.
    // Якщо елемент не знайдено — KeyNotFoundException.
    Task<T> ReadAsync(Guid id);

    // READ ALL: повертає snapshot (миттєву копію) усіх елементів.
    // Повертається КОПІЯ, а не внутрішня колекція: зовнішній код
    // не може змінити стан сервісу поза ним.
    Task<List<T>> ReadAllAsync();

    // READ ALL (варіант з пагінацією).
    // Pagination (пагінація) — сторінка page (нумерація з 1)
    // містить amount елементів.
    // Наприклад: page = 2, amount = 20 — елементи з 21 по 40.
    // page <= 0 або amount <= 0 — ArgumentOutOfRangeException.
    // Сторінка за межами колекції — порожня колекція (не помилка).
    Task<List<T>> ReadAllAsync(int page, int amount);

    // UPDATE: замінює елемент із таким самим Id новою версією.
    // Якщо елемент не знайдено — KeyNotFoundException.
    Task UpdateAsync(T element);

    // REMOVE: видаляє елемент за його Id.
    // Якщо елемент не знайдено — KeyNotFoundException.
    Task RemoveAsync(Guid id);

    // SAVE: асинхронно серіалізує колекцію у JSON-файл.
    // Захист одночасного запису: лише один «writer» одночасно
    // пише файл (SemaphoreSlim на рівні 1).
    Task SaveAsync(string filePath);
}
