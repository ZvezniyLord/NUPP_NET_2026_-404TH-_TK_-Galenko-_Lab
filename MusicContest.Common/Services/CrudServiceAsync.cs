using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// АСІНХРОННИЙ (async) generic CRUD-сервіс (ЛР2).
//
// T — generic (універсальний) тип, обмежений "where T : IEntity":
// сервіс працює з будь-якою моделлю, що має унікальний Guid Id.
//
// THREAD-SAFE (поточно-безпечна) КОЛЕКЦІЯ:
// внутрішня колекція — ConcurrentDictionary<Guid, T> — ВБУДОВANA thread-safe
// колекція .NET (System.Collections.Concurrent). Її методи Add/Update/Remove
// атомарні під конкурентним доступом багатьох потоків: без lock і без втрат
// даних. Тому CRUD-операції не потребують критичних секцій (critical section):
// TryAdd/TryUpdate/TryRemove виконуються одночасно різними потоками безпечно.
public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : IEntity
{
    // ConcurrentDictionary — вбудована поточно-безпечна колекція .NET:
    // ключ (Guid Id сутності) → елемент. Містить власні внутрішні блокування,
    // тому багато потоків можуть одночасно Add/Remove без зовнішнього lock.
    private readonly ConcurrentDictionary<Guid, T> _items = new();

    // Шлях файлу, у який сервіс зберігає колекцію (SaveAsync()).
    public string? FilePath { get; set; }

    // СПІЛЬНИЙ (static) SemaphoreSlim на рівні 1: під час запису JSON-файлу
    // одночасно пише лише ОДИН writer. SemaphoreSlim — "лічильниковий"
    // семафор: WaitAsync/Release забирають та повертають мітки (ліцензії).
    // На відміну від lock, він підтримує await: потік чекає АСІНХРОННО,
    // а не блокує потік пулу (thread pool).
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

    // SNAPSHOT (миттєва копія колекції) у ДЕТЕРМІНОВАНому порядку за Id.
    // Enumerating ConcurrentDictionary безпечний для потоків, але його порядок
    // не гарантований, тому зчитування впорядковуємо за Id: пагінація та
    // foreach дають стабільний, повторюваний результат між викликами.
    private List<T> BuildSnapshot()
    {
        return _items.Values.OrderBy(x => x.Id).ToList();
    }

    // CREATE = створити / додати. TryAdd атомарний: якщо такий Id уже є —
    // false (duplicate неможливий навіть при одночасних викликах з потоків).
    public async Task<bool> CreateAsync(T element)
    {
        bool added = _items.TryAdd(element.Id, element);

        // await Task.Yield() — метод справжній async: потік пулу віддає
        // контроль і продовжує виконання з іншого потоку пулу.
        await Task.Yield();

        return added;
    }

    // READ = читати: один елемент за його Id.
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

    // READ ALL = всі елементи (snapshot: копія, а не внутрішня колекція).
    public async Task<IEnumerable<T>> ReadAllAsync()
    {
        List<T> snapshot = BuildSnapshot();
        await Task.Yield();
        return snapshot;
    }

    // READ ALL (варіант з пагінацією) через LINQ.
    // LINQ (Language Integrated Query) — вбудований у мову C# механізм
    // роботи з колекціями: Skip/Take/Where/Order...
    public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
    {
        // Валідація (перевірка коректності значень). Нумерація сторінок з 1.
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

        // Skip(n) — пропустити n перших елементів; Take(n) — взяти n наступних.
        // Сторінка page з amount елементами починається з індексу (page - 1) * amount.
        List<T> pageItems = snapshot
            .Skip((page - 1) * amount)
            .Take(amount)
            .ToList();

        return pageItems;
    }

    // UPDATE = оновити. TryUpdate замінює значення лише якщо ключ існує
    // (порівнює зі старим значенням); false — елемента з таким Id немає,
    // і новий елемент НЕ створюється помилково.
    public async Task<bool> UpdateAsync(T element)
    {
        bool replaced = false;

        if (_items.TryGetValue(element.Id, out T? existing))
        {
            // Атомарна заміна "старий → новий" під конкурентним доступом.
            replaced = _items.TryUpdate(element.Id, element, existing);
        }

        await Task.Yield();

        return replaced;
    }

    // REMOVE = видалити. TryRemove атомарний: false — елемента з таким Id немає.
    public async Task<bool> RemoveAsync(T element)
    {
        bool removed = _items.TryRemove(element.Id, out _);

        await Task.Yield();

        return removed;
    }

    // SAVE = зберегти (асинхронно) колекцію у JSON-файл за FilePath.
    public async Task<bool> SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new InvalidOperationException("FilePath не задано для CrudServiceAsync.");
        }

        string filePath = FilePath!;

        // Спочатку — папка (створити, якщо немає).
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 1) Snapshot: стабільний стан колекції на момент виклику.
        List<T> snapshot = BuildSnapshot();

        // 2) JSON-серіалізація — поза семафором: семафор захищає саме
        // ФАЙЛОВИЙ запис, а не серіалізацію.
        string json = JsonSerializer.Serialize(snapshot, _jsonOptions);

        // 3) WaitAsync — "зайняти" одну єдину мітку. Якщо інший writer уже
        // пише файл, потік асинхронно чекає (await не блокує потік пулу).
        await _jsonWriteGate.WaitAsync();

        try
        {
            // File.WriteAllTextAsync — справжній async запис: файловий IO
            // не блокує викликуючий потік на весь час запису.
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        finally
        {
            // RELEASE = повернути мітку: "місце вільне, можна писати знову".
            _jsonWriteGate.Release();
        }
    }

    // IENUMERABLE<T>: пряма ітерація foreach (var item in service).
    // GetEnumerator повертає IEnumerator над SNAPSHOT (миттєвою копією):
    // foreach бачить незмінну впорядковану копію, тому одночасні
    // CreateAsync/UpdateAsync/RemoveAsync не ламають ітерацію.
    public IEnumerator<T> GetEnumerator()
    {
        List<T> snapshot = BuildSnapshot();
        return snapshot.GetEnumerator();
    }

    // IEnumerator (не generic) — обов'язкова частина IEnumerable<T>
    // (IEnumerable<T> успадковує IEnumerable, тож не-generic метод теж
    // треба реалізувати).
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
