using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Асинхронний CRUD-сервіс ЛР2 працює з узагальненим типом T.
// Обмеження IEntity вимагає, щоб модель мала Guid Id.
//
// Елементи зберігаються в ConcurrentDictionary<Guid, T>.
// Колекція підтримує потокобезпечні операції додавання, оновлення
// та видалення. Для цих операцій окремий lock не потрібен.
//
// Методи CRUD повертають Task, але операції над словником виконуються
// синхронно. Task.Yield() планує асинхронне продовження методу.
// Реальне асинхронне файлове введення-виведення виконує SaveAsync.

public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : IEntity
{
    // Ключем словника є Guid Id сутності, значенням є сам об'єкт.
    private readonly ConcurrentDictionary<Guid, T> _items = new();

    // Шлях файлу, у який сервіс зберігає колекцію (SaveAsync()).
    public string? FilePath { get; set; }

    // Спільний семафор обмежує запис JSON-файла однією операцією.
    // WaitAsync очікує дозвіл асинхронно, а Release повертає його.
    private static readonly SemaphoreSlim _jsonWriteGate = new(1, 1);

    // Спільні параметри серіалізації JSON (відформатований, читабельний).
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public CrudServiceAsync(string? filePath = null)
    {
        FilePath = filePath;
    }

    // Створює поверхневу копію послідовності та впорядковує її за Id.
    // Порядок елементів ConcurrentDictionary не гарантований, тому
    // сортування робить результати читання й пагінації передбачуваними.
    // Самі об'єкти не клонуються: посилання на елементи залишаються спільними.
    private List<T> BuildSnapshot()
    {
        return _items.Values.OrderBy(x => x.Id).ToList();
    }

    // Додає елемент. TryAdd повертає false, якщо такий Id уже існує.
    public async Task<bool> CreateAsync(T element)
    {
        bool added = _items.TryAdd(element.Id, element);

        // Task.Yield() планує продовження асинхронно. Операція TryAdd
        // вище вже виконана синхронно; введення-виведення тут немає.
        await Task.Yield();

        return added;
    }

    // Читає один елемент за його Id.
    public async Task<T> ReadAsync(Guid id)
    {
        bool found = _items.TryGetValue(id, out T? item);

        await Task.Yield();

        if (!found || item is null)
        {
            throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
        }

        return item;
    }

    // Повертає поверхневу копію послідовності, а не сам словник.
    public async Task<IEnumerable<T>> ReadAllAsync()
    {
        List<T> snapshot = BuildSnapshot();
        await Task.Yield();
        return snapshot;
    }

    // Формує сторінку зі snapshot за допомогою LINQ.
    public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
    {
        // Перевіряє параметри пагінації. Нумерація сторінок починається з 1.
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Сторінка має бути >= 1.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Розмір сторінки має бути > 0.");
        }

        List<T> snapshot = BuildSnapshot();
        await Task.Yield();

        // Skip пропускає попередні елементи, а Take обмежує розмір сторінки.
        // Індекс першого елемента сторінки: (page - 1) * amount.
        List<T> pageItems = snapshot
            .Skip((page - 1) * amount)
            .Take(amount)
            .ToList();

        return pageItems;
    }

    // Оновлює наявний елемент. TryUpdate порівнює поточне значення
    // зі збереженим і не створює новий запис, якщо ключа немає.
    public async Task<bool> UpdateAsync(T element)
    {
        bool replaced = false;

        if (_items.TryGetValue(element.Id, out T? existing))
        {
            // TryUpdate виконує умовну заміну значення для наявного ключа.
            replaced = _items.TryUpdate(element.Id, element, existing);
        }

        await Task.Yield();

        return replaced;
    }

    // Видаляє елемент за Id. TryRemove повертає false, якщо запису немає.
    public async Task<bool> RemoveAsync(T element)
    {
        bool removed = _items.TryRemove(element.Id, out _);

        await Task.Yield();

        return removed;
    }

    // Зберігає колекцію у JSON-файл за шляхом FilePath.
    public async Task<bool> SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new InvalidOperationException("FilePath не задано для CrudServiceAsync.");
        }

        string filePath = FilePath!;

        // За потреби створює каталог для файла.
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Спочатку створює поверхневу копію поточної послідовності.
        List<T> snapshot = BuildSnapshot();

        // Серіалізує копію до JSON до отримання дозволу на запис.
        // Семафор захищає файлову операцію, а не процес серіалізації.
        string json = JsonSerializer.Serialize(snapshot, _jsonOptions);

        // Очікує дозвіл на запис. Якщо файл уже записує інший виклик,
        // поточний метод очікує асинхронно.
        await _jsonWriteGate.WaitAsync();

        try
        {
            // File.WriteAllTextAsync виконує асинхронний запис файла.
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        finally
        {
            // Повертає дозвіл, навіть якщо запис завершився помилкою.
            _jsonWriteGate.Release();
        }
    }

    // GetEnumerator перебирає snapshot. Зміни словника не змінюють
    // уже сформовану послідовність, але самі об'єкти не клонуються.
    public IEnumerator<T> GetEnumerator()
    {
        List<T> snapshot = BuildSnapshot();
        return snapshot.GetEnumerator();
    }

    // IEnumerable<T> успадковує IEnumerable, тому реалізовано обидва методи.
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
