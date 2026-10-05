using System.Collections;
using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Async-версія generic CRUD-сервісу.
//
// "where T : IEntity" — обмеження generic-типу:
// T обов'язково повинен реалізовувати IEntity, тобто мати Id.
//
// List<T> — вбудована колекція .NET, у якій зберігаються дані.
//
// ВАЖЛИВО: List<T> сама по собі НЕ є thread-safe (потокобезпечна).
// Якщо кілька потоків одночасно додають/видаляють/змінюють елементи,
// внутрішній стан списку може порушитися (race condition = "стан гонитви":
// результат залежить від порядку виконання потоків).
//
// Тому будь-які операції читання/модифікації спільної колекції виконуються
// у критичній секції через lock (_syncRoot):
//
//   lock = блокування.
//   У критичну секцію одночасно може увійти лише ОДЕН потік.
//   Це захищає List<T> від одночасної модифікації кількома потоками.
//
// _syncRoot = sync + root = об'єкт, на якому виконується lock.
// Це просто приватний об'єкт, який потрібен лише як "місце" для lock.
public class CrudServiceAsync<T> : ICrudServiceAsync<T>
    where T : IEntity
{
    // List<T> — колекція, у якій зберігаються елементи.
    // readonly означає, що посилання _items не можна замінити іншим List,
    // але елементи всередині списку можна додавати/видаляти.
    private readonly List<T> _items = new();

    // lock = synchronization = синхронізація.
    // _syncRoot = root = об'єкт, на якому виконується lock (критична секція).
    private readonly object _syncRoot = new();

    // Зберігання файлу — асинхронне (I/O). Звичайний lock НЕ МОЖНА утримувати
    // через await. Тому асинхронний доступ до файлу окремо захищає SemaphoreSlim.
    //
    // SemaphoreSlim(1, 1) = семафор, який дозволяє записувати файл одночасно
    // лише ОДНІЙ operation (операції).
    //
    // async-доступ: while waiting, the thread is not blocked,
    // the task is suspended (await).
    private readonly SemaphoreSlim _fileSemaphore = new(1, 1);

    // filePath = шлях до файлу.
    // Задано в конструкторі, бо метод SaveAsync() не має параметра.
    private readonly string _filePath;

    // Constructor: бере шлях до JSON-файлу.
    // Якщо файл уже існує — завантажити колекцію з пам'яті (як у Lab 1 Load).
    public CrudServiceAsync(string filePath)
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            LoadFromDisk();
        }
    }

    // LoadFromDisk = завантажити з диска (файлу).
    //
    // Десеріалізація — зворотний процес: JSON -> об'єкти C#.
    //
    // Поліморфні атрибути на ContestParticipant ($type)
    // відновлюють конкретний підтип (SoloSinger / VocalGroup)
    // під час Load.
    private void LoadFromDisk()
    {
        List<T>? loaded;

        // load під lock (критична секція).
        lock (_syncRoot)
        {
            string json = File.ReadAllText(_filePath);

            // Deserialize = десеріалізувати: JSON -> List<T>.
            loaded = JsonSerializer.Deserialize<List<T>>(json);

            // Очищаємо колекцію перед завантаженням: файл — джерело істини.
            _items.Clear();

            if (loaded is not null)
            {
                _items.AddRange(loaded);
            }
        }
    }

    // CreateAsync = створити (Create).
    //
    // Task<bool> = асинхронна операція, яка після завершення поверне bool.
    //
    // Operation works only with memory and is very fast, so we do not need
    // to artificially create a new thread.
    //
    // Task.FromResult(true) — створює вже завершену Task з результатом true.
    //
    // Якщо дублікат Id — InvalidOperationException.
    public Task<bool> CreateAsync(T element)
    {
        // КРИТИЧНА СЕКЦІЯ:
        // lock-секція. У критичну секцію одночасно може увійти
        // лише один потік. Це захищає _items.
        lock (_syncRoot)
        {
            // Any(...) — перевірити, чи є вже елемент з таким Id.
            // x => ... — лямбда: "для кожного елемента x в списку...".
            if (_items.Any(x => x.Id == element.Id))
            {
                throw new InvalidOperationException(
                    $"Елемент з Id {element.Id} уже існує.");
            }

            _items.Add(element);
        }

        // Повертаємо завершену Task з результатом true.
        return Task.FromResult(true);
    }

    // ReadAsync = прочитати (Read).
    //
    // Повертає один елемент за його Id.
    //
    // FirstOrDefault = перший збіг або null, якщо нічого не знайдено.
    //
    // ?? throw — якщо FirstOrDefault повернув null (не знайдено),
    //            кидаємо KeyNotFoundException.
    public Task<T> ReadAsync(Guid id)
    {
        T result;

        // КРИТИЧНА СЕКЦІЯ:
        // lock-секція. У критичну секцію одночасно може увійти
        // лише один потік. Це захищає _items.
        lock (_syncRoot)
        {
            result = _items.FirstOrDefault(x => x.Id == id)
                    ?? throw new KeyNotFoundException(
                        $"Елемент з Id {id} не знайдено.");
        }

        return Task.FromResult(result);
    }

    // ReadAllAsync = прочитати всі (ReadAll).
    //
    // snapshot = знімок / копія стану колекції у конкретний момент.
    //
    // Під lock робимо копію (ToList());
    // ми НЕ повертаємо безпосередньо _items, тому зовнішній код
    // не має прямого доступу до внутрішнього List<T>.
    public Task<IEnumerable<T>> ReadAllAsync()
    {
        // GetSnapshot — робить копію під lock (зайняв critical section).
        List<T> snapshot = GetSnapshot();

        // Явний тип IEnumerable<T>, щоб Task<List<T>> підійшов під Task<IEnumerable<T>>.
        return Task.FromResult<IEnumerable<T>>(snapshot);
    }

    // ReadAllAsync(int page, int amount) = pagination (пагінація).
    //
    // page   = номер сторінки (від 1).
    // amount = кількість елементів на сторінці.
    //
    // Валідація:
    //   page   <= 0 -> ArgumentOutOfRangeException
    //   amount <= 0 -> ArgumentOutOfRangeException
    //
    // Алгоритм:
    //   Start index = (page - 1) * amount.
    //   Skip = пропустити N елементів;
    //   Take = взяти N елементів.
    //
    // Example: page = 2, amount = 20.
    //   Skip: (2 - 1) * 20 = 20
    //   I.e., skip the first 20 and take the next 20.
    //
    // Якщо сторінка за межами колекції — повертається порожня колекція.
    public Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page), "Page (page) має бути більшою від 0.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Amount (amount) має бути більшим від 0.");
        }

        // КРИТИЧНА СЕКЦІЯ:
        // робимо snapshot під lock — так гарантується цілісність
        // знімка (якщо інший потік додасть елемент, він не потрапить
        // у поточний snapshot).
        List<T> snapshot = GetSnapshot();

        // LINQ:
        //   Skip(n) — пропустити n елементів;
        //   Take(n) — взяти n елементів.
        //
        // page = 2, amount = 20 -> Skip(20).Take(20) -> елементи 21-40.
        List<T> pageElements = snapshot
            .Skip((page - 1) * amount)
            .Take(amount)
            .ToList();

        // Явний тип IEnumerable<T>, щоб Task<List<T>> підійшов під Task<IEnumerable<T>>.
        return Task.FromResult<IEnumerable<T>>(pageElements);
    }

    // UpdateAsync = оновити (Update).
    //
    // Оновлює наявний елемент (шуканний за Id).
    //
    // FindIndex — шукати індекс елемента з таким Id.
    // Якщо нічого не знайдено — повертає -1.
    //
    // Якщо елемент не знайдено — KeyNotFoundException.
    public Task<bool> UpdateAsync(T element)
    {
        // КРИТИЧНА СЕКЦІЯ:
        // lock-секція. У критичну секцію одночасно може увійти
        // лише один потік. Це захищає _items.
        lock (_syncRoot)
        {
            int index = _items.FindIndex(x => x.Id == element.Id);

            if (index < 0)
            {
                throw new KeyNotFoundException(
                    $"Елемент з Id {element.Id} не знайдено.");
            }

            // Замінюємо старий об'єкт новою версією.
            _items[index] = element;
        }

        return Task.FromResult(true);
    }

    // RemoveAsync = видалити (Remove).
    //
    // Видалити наявний елемент (шуканний за Id).
    //
    // Якщо елемент не знайдено — KeyNotFoundException.
    public Task<bool> RemoveAsync(T element)
    {
        // КРИТИЧНА СЕКЦІЯ:
        // lock-секція. У критичну секцію одночасно може увійти
        // лише один потік. Це захищає _items.
        lock (_syncRoot)
        {
            // Шукаємо елемент за Id.
            T existing = _items.FirstOrDefault(x => x.Id == element.Id)
                        ?? throw new KeyNotFoundException(
                            $"Елемент з Id {element.Id} не знайдено.");

            // Remove(element) — видалити елемент зі списку за посиланням.
            _items.Remove(existing);
        }

        return Task.FromResult(true);
    }

    // SaveAsync = зберегти (Save).
    //
    // Асинхронно зберегти колекцію у файл JSON.
    //
    // Алгоритм:
    //   1. Під lock зробити snapshot: _items.ToList()
    //   2. ВИЙТИ з lock
    //   3. Серіалізувати snapshot (System.Text.Json)
    //   4. await _fileSemaphore.WaitAsync() — асинхронно
    //      очікати доступу до файлу
    //   5. Створити directory, якщо потрібно;
    //      await File.WriteAllTextAsync(...)
    //   6. return true
    //   7. finally: _fileSemaphore.Release()
    //
    // ВАЖЛИВО:
    //   Звичайний lock НЕ МОЖНА утримувати через await.
    //   Тому всередині lock ми лише швидко робимо snapshot,
    //   після чого звільняємо lock.
    //   Асинхронний доступ до файлу окремо захищає SemaphoreSlim.
    //
    // SemaphoreSlim(1, 1) = одночасно записувати файл може лише одна operation.
    //
    // System.Text.Json = сучасний .NET JSON-серіалізатор.
    // WriteIndented = true — формат JSON з відступами (читабельний).
    //
    // File.WriteAllTextAsync = асинхронний запис у файл (I/O).
    // await = асинхронне очікування завершення Task
    //        без блокування потоку.
    public async Task<bool> SaveAsync()
    {
        // Step 1: snapshot під lock (critical section).
        List<T> snapshot = GetSnapshot();

        // Step 3: serialize (поза lock, швидко, але не тримаємо lock).
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        // Serialize = серіалізувати: List<T> -> JSON-рядок.
        string json = JsonSerializer.Serialize(snapshot, options);

        // Step 4: async file write, protected by SemaphoreSlim.
        await _fileSemaphore.WaitAsync();

        try
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                // Directory.CreateDirectory — створити папку,
                // якщо її ще немає.
                Directory.CreateDirectory(directory);
            }

            // Step 5: async file write (I/O).
            await File.WriteAllTextAsync(_filePath, json);

            return true;
        }
        finally
        {
            // Step 7: finally — always release.
            // Even in case of an exception — release.
            _fileSemaphore.Release();
        }
    }

    // GetSnapshot = зробити копію колекції під одній lock.
    // Використовується ReadAllAsync, ReadAllAsync(page, amount),
    // SaveAsync, GetEnumerator.
    private List<T> GetSnapshot()
    {
        // КРИТИЧНА СЕКЦІЯ:
        // ToList() = копія списку. Оригінальний _items не змінюється.
        lock (_syncRoot)
        {
            return _items.ToList();
        }
    }

    // ====== IEnumerable<T> implementation ======
    //
    // IEnumerable<T> = "перелічуваний" — інтерфейс, який визначає
    // послідовність: її можна перебирати через foreach.
    //
    // Example of use:
    //   foreach (SoloSinger singer in service)
    //   {
    //       ...
    //   }
    //
    // IEnumerator<T> = перелічувач — "курсор", який фактично
    // проходить по елементах послідовності.
    //
    // ВАЖЛИВО: НЕ повертати enumerator внутрішнього _items напряму!
    // Інакше, якщо інший потік змінить list під час foreach,
    // це порушить iter (IndexOutOfRange, "об'єкт було змінено").
    //
    // Тому під lock робимо snapshot (GetSnapshot) і перебираємо snapshot.
    public IEnumerator<T> GetEnumerator()
    {
        // КРИТИЧНА СЕКЦІЯ:
        // робимо snapshot під lock.
        List<T> snapshot = GetSnapshot();

        // snapshot.GetEnumerator() — перелічувач для snapshot.
        return snapshot.GetEnumerator();
    }

    // Негеноеричний IEnumerable (System.Collections.IEnumerable)
    // теж спадкується з IEnumerable<T>. Тому треба реалізувати
    // його метод явно — він повертає нереєстрований IEnumerator.
    IEnumerator IEnumerable.GetEnumerator()
    {
        // КРИТИЧНА СЕКЦІЯ:
        // робимо snapshot під lock.
        List<T> snapshot = GetSnapshot();

        // (System.Collections.)IEnumerator — "класичний" нереєстрований
        // перелічувач.
        return snapshot.GetEnumerator();
    }
}
