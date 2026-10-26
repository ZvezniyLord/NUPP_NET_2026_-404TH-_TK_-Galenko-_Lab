using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// АСІНХРОННИЙ (async) generic-контракт CRUD-сервісу (ЛР2).
//
// Асинхронність: метод не повертає дані напряму, а повертає Task<T> —
// «обіцянку»: «дані з'являться потім». Викликаючий код чекає через await
// БЕЗ БЛОКУВАННЯ потоку: за час очікування потік пулу (thread pool)
// може виконувати іншу роботу.
//
// Generic тип T: один і той самий сервіс працює з будь-якою сутністю,
// що реалізує IEntity (має Guid Id) — Song, SoloSinger, VocalGroup...
//
// IEnumerable<T>: інтерфейс успадковує IEnumerable<T>, тому об'єкт сервісу
// можна безпосередньо ітерувати: foreach (var item in service) { ... }
public interface ICrudServiceAsync<T> : IEnumerable<T> where T : IEntity
{
    // CREATE: додає елемент. true — додано; false — такий Id вже існує.
    public Task<bool> CreateAsync(T element);

    // READ: повертає елемент за його Guid.
    // Якщо елемент не знайдено — KeyNotFoundException.
    public Task<T> ReadAsync(Guid id);

    // READ ALL: миттєва копія (snapshot) усіх елементів у детермінованому
    // порядку. Зовнішній код не може змінити стан сервісу через цю колекцію.
    public Task<IEnumerable<T>> ReadAllAsync();

    // READ ALL із пагінацією (pagination): сторінка page (нумерація з 1)
    // містить amount елементів. Наприклад page = 2, amount = 20 — елементи
    // з 21 по 40. page <= 0 або amount <= 0 — ArgumentOutOfRangeException;
    // сторінка за межами колекції — порожня колекція (не помилка).
    public Task<IEnumerable<T>> ReadAllAsync(int page, int amount);

    // UPDATE: замінює елемент із таким самим Id новою версією.
    // true — оновлено; false — елемента з таким Id немає.
    public Task<bool> UpdateAsync(T element);

    // REMOVE: видаляє елемент за його Id.
    // true — видалено; false — елемента з таким Id немає.
    public Task<bool> RemoveAsync(T element);

    // SAVE: асинхронно серіалізує колекцію у JSON-файл за шляхом FilePath.
    // true — файл записано. Одночасний запис захищено семафором: лише один
    // writer пише файл одночасно, тож паралельні SaveAsync не псують JSON.
    public Task<bool> SaveAsync();
}
