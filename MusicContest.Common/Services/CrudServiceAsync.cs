using System.Collections;
using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// АСІНХРОННИЙ (async) generic CRUD-сервіс.
//
// T — generic (універсальний) тип, обмежений "where T : IEntity":
// сервіс працює з будь-якою моделлю, що має унікальний Id.
//
// ПРОБЛЕМА THREAD SAFETY (поточної безпеки):
// внутрішня колекція — List<T>. List<T> НЕ є thread-safe (поточно-безпечною):
// якщо два потоки одночасно викличуть Add або IndexOf, вони можуть "перебивати"
// один одного і загубити дані. Тому КОЖНА критична секція (critical section —
// блок коду, який працює зі спільним станом) обгорнута в lock.
//
// lock (або lock-блок) — механізм взаємного виключення (mutual exclusion):
// одночасно в блоці lock може бути лише ОДИН потік, інші чекають біля блоку.
public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : IEntity
{
    // List<T> — внутрішня колекція даних. readonly: посилання не можна замінити.
    private readonly List<T> _items = new();

    // Окремий об'єкт-блокувальник (lock object).
    // lock вимагає об'єкт-ключ, яким усі потоки користуються одночасно.
    private readonly object _gate = new();

    // СПІЛЬНИЙ (static) SemaphoreSlim на рівні 1:
    // в момент запису JSON-файлу одночасно пише лише ОДИН writer.
    // SemaphoreSlim — "лічильниковий" семафор: у нього є кількість "міток",
    // і методи WaitAsync/Release забирають та повертають ці мітки.
    // На відміну від lock, цей семафор підтримує await — потік чекає
    // АСІНХРОННО, а не блокує потік пулу (thread pool).
    private static readonly SemaphoreSlim _jsonWriteGate = new(1, 1);

    // Спільні параметри серіалізації JSON (відформатований, читабельний).
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    // SNAPSHOT (миттєва копія колекції).
    // Сніпшот створюється під lock: ми отримуємо послідовну колекцію у стані,
    // який був на момент виклику, без впливу одночасних Create/Update/Remove.
    // (Окремий приватний метод: в C# локальна функція всередині ітератора
    // GetEnumerator не може звертатися до generic-типу T, тому копію
    // будують усі методи через один і той самий метод.)
    private List<T> BuildSnapshot()
    {
        // ToList() — копія списку (посилань на об'єкти).
        lock (_gate)
        {
            return _items.ToList();
        }
    }

    // CREATE = створити / додати.
    public async Task CreateAsync(T element)
    {
        // Перевірка на duplicate (подвійний) Id і додавання — у ОДНІЙ критичній
        // секції. Так перевірка і додавання — атомарна дія (atomic):
        // між ними ніхто не може "вприснути" той самий Id.
        lock (_gate)
        {
            // Any(...) перевіряє, чи є вже елемент з таким Id.
            if (_items.Any(x => x.Id == element.Id))
            {
                throw new InvalidOperationException($"Елемент з Id {element.Id} уже існує.");
            }

            _items.Add(element);
        }

        // await Task.Yield() — метод справжній async: потік пулу віддає контроль
        // і продовжує виконувати цей метод з іншого потоку пулу.
        await Task.Yield();
    }

    // READ = читати: один елемент за його Id.
    public async Task<T> ReadAsync(Guid id)
    {
        List<T> snapshot = BuildSnapshot();
        await Task.Yield();

        T? found = snapshot.FirstOrDefault(x => x.Id == id);

        // FirstOrDefault, якщо нічого не знайдено, повертає
        // default значення: null для референс-типів.
        if (found is null)
        {
            throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
        }

        return found;
    }

    // READ ALL = всі елементи (snapshot: копія, а не внутрішня колекція).
    public async Task<List<T>> ReadAllAsync()
    {
        List<T> snapshot = BuildSnapshot();
        await Task.Yield();
        return snapshot;
    }

    // READ ALL (варіант з пагінацією) через LINQ.
    // LINQ (Language Integrated Query) — вбудований у мову C# механізм
    // роботи з колекціями: Skip/Take/Where/Order...
    public async Task<List<T>> ReadAllAsync(int page, int amount)
    {
        // Валідація (перевірка коректності значень).
        // Нумерація сторінок починається з 1.
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

    // UPDATE = оновити.
    public async Task UpdateAsync(T element)
    {
        bool replaced;

        // Пошук і заміна — в ОДНІЙ критичній секції (atomic).
        lock (_gate)
        {
            int index = _items.FindIndex(x => x.Id == element.Id);

            if (index < 0)
            {
                replaced = false;
            }
            else
            {
                // Замінюємо стару версію на нову.
                _items[index] = element;
                replaced = true;
            }
        }

        await Task.Yield();

        if (!replaced)
        {
            throw new KeyNotFoundException($"Елемент з Id {element.Id} не знайдено.");
        }
    }

    // REMOVE = видалити.
    public async Task RemoveAsync(Guid id)
    {
        bool removed;

        lock (_gate)
        {
            int index = _items.FindIndex(x => x.Id == id);

            if (index < 0)
            {
                removed = false;
            }
            else
            {
                _items.RemoveAt(index);
                removed = true;
            }
        }

        await Task.Yield();

        if (!removed)
        {
            throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
        }
    }

    // SAVE = зберегти (асинхронно).
    // Файловий запис — справжній async: File.WriteAllTextAsync не блокує
    // викликуючий потік на весь час запису.
    public async Task SaveAsync(string filePath)
    {
        // Спочатку — папка (створити, якщо немає).
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 1) Snapshot під lock: стабільний стан колекції.
        List<T> snapshot = BuildSnapshot();

        // 2) JSON-серіалізація — поза семафором: семафор захищає саме
        // ФАЙЛОВИЙ запис, а не серіалізацію.
        string json = JsonSerializer.Serialize(snapshot, _jsonOptions);

        // 3) WaitAsync — "зайняти" одна єдина мітка. Якщо інший writer уже
        // пише файл, потік асинхронно чекає (await не блокує потік пулу).
        await _jsonWriteGate.WaitAsync();

        try
        {
            // WriteAllTextAsync — асинхронний запис JSON у файл.
            // Лише один writer одночасно пише файл, тому паралельні
            // SaveAsync не пошкодять JSON.
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            // RELEASE = повернути мітку: "місце вільне, можна писати знову".
            _jsonWriteGate.Release();
        }
    }

    // IENUMERABLE<T>: пряма ітерація foreach (var item in service).
    // GetEnumerator повертає IEnumerator над SNAPSHOT (миттєвою копією):
    // foreach бачить незмінну копію, а не внутрішню List<T>, тому
    // одночасні CreateAsync/UpdateAsync/RemoveAsync не ламають ітерацію
    // і не кидають InvalidOperationException ("Collection was modified"
    // — колекцію змінено під час ітерації).
    public IEnumerator<T> GetEnumerator()
    {
        List<T> snapshot = BuildSnapshot();
        return snapshot.GetEnumerator();
    }

    // IEnumerator (не generic) — обов'язкова частина IEnumerable<T>
    // (IEnumerable<T> успадковує IEnumerable, тож не-generic метод
    // теж треба переоб'явити). IEnumerator<T> успадковує IEnumerator,
    // тож повернутий IEnumerator<T> автоматично відповідає
    // не-generic сигнатурі.
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
