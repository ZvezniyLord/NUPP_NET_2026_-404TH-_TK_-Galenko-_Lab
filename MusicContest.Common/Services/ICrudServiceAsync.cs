namespace MusicContest.Common.Services;

// ICrudServiceAsync<T> = асинхронний generic-контракт CRUD.
//
// generic тип T = тип елементів буде визначений на момент створення об'єкта-сервісу.
// Обмеження "where T : IEntity" (у реалізації) вимагає, щоб T мав унікальний
// Id (GUID) — це необхідна умова для пошуку, оновлення та видалення.
//
// CRUD:
//   CreateAsync  = створити (Create);
//   ReadAsync    = прочитати (Read);
//   UpdateAsync  = оновити (Update);
//   RemoveAsync  = видалити (Remove);
//   SaveAsync    = асинхронно зберегти колекцію у файл JSON.
//
// ICrudServiceAsync<T> : IEnumerable<T> — сервіс сам є послідовністю:
// його можна перебирати через foreach (foreach (T item in service)).
//
// async-методи повертають Task<T>.
// Task<T> = об'єкт-обіцянка: після завершення операції можна отримати результат T.
// async = метод може використовувати await і повертає Task/Task<T>.
// IMPORTANT: async НЕ створює новий потік — це лише синтаксис, який дозволяє
//            "зупинитися" на I/O (файл, мережа) без блокування потоку.
public interface ICrudServiceAsync<T> : IEnumerable<T>
{
    // Add element (Create). Повертає true, якщо елемент було додано.
    // При дублі Id (повторному створенні об'єкта з тим самим Id)
    // метод кидає InvalidOperationException.
    public Task<bool> CreateAsync(T element);

    // Get one element by its Guid.
    // Повертає елемент за його Id.
    // Якщо елемент не знайдено — кидає KeyNotFoundException.
    public Task<T> ReadAsync(Guid id);

    // Get a snapshot (copy) of the entire collection.
    // Повертає знімок (копію) всієї колекції.
    // Знімок — це копія, тому зовнішній код не має прямого доступу
    // до внутрішнього List<T> сервісу.
    public Task<IEnumerable<T>> ReadAllAsync();

    // Pagination (пагінація): page = номер сторінки,
    // amount = кількість елементів на сторінці.
    // Повертає елементи сторінки.
    // Якщо сторінка за межами колекції — повертається порожня колекція.
    // Якщо page <= 0 або amount <= 0 — ArgumentOutOfRangeException.
    public Task<IEnumerable<T>> ReadAllAsync(int page, int amount);

    // Update existing element (Update).
    // Оновлює наявний елемент (шуканний за Id).
    // Повертає true. Якщо елемент не знайдено — KeyNotFoundException.
    public Task<bool> UpdateAsync(T element);

    // Delete existing element (Remove).
    // Видалити наявний елемент (шуканний за Id).
    // Повертає true. Якщо елемент не знайдено — KeyNotFoundException.
    public Task<bool> RemoveAsync(T element);

    // SaveAsync = save the current collection to disk asynchronously
    // (JSON) to the file whose path was given in the constructor.
    // Асинхронно зберегти колекцію у файл JSON.
    // Шлях до файлу задано в конструкторі сервісу.
    public Task<bool> SaveAsync();
}
