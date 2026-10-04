using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Generic-клас: CrudService<T> може працювати з різними типами.
// "where T : IEntity" — обмеження generic-типу:
// T обов'язково повинен реалізовувати IEntity, тобто мати Id.
public class CrudService<T> : ICrudService<T> where T : IEntity
{
    // List<T> — вбудована колекція .NET, у якій зберігаються дані.
    // readonly означає, що посилання _items не можна замінити іншим List,
    // але елементи всередині списку можна додавати/видаляти.
    private readonly List<T> _items = new();

    // CREATE = створити / додати.
    public void Create(T element)
    {
        // Any(...) перевіряє, чи вже існує елемент з таким Id.
        // x => ... — лямбда-вираз; x означає поточний елемент списку.
        if (_items.Any(x => x.Id == element.Id))
        {
            throw new InvalidOperationException($"Елемент з Id {element.Id} уже існує.");
        }

        _items.Add(element);
    }

    // READ = прочитати / знайти один об'єкт.
    public T Read(Guid id)
    {
        // FirstOrDefault повертає перший збіг або null/default, якщо нічого не знайдено.
        // Оператор ?? означає: якщо зліва null — виконати те, що справа.
        return _items.FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
    }

    // READ ALL = отримати всі об'єкти.
    public IEnumerable<T> ReadAll()
    {
        // ToList() повертає копію списку, щоб зовнішній код
        // не отримав прямий доступ до внутрішньої колекції _items.
        return _items.ToList();
    }

    // UPDATE = оновити.
    public void Update(T element)
    {
        // FindIndex шукає позицію елемента з таким самим Id.
        // Якщо нічого не знайдено, повертає -1.
        int index = _items.FindIndex(x => x.Id == element.Id);

        if (index < 0)
        {
            throw new KeyNotFoundException($"Елемент з Id {element.Id} не знайдено.");
        }

        // Замінюємо старий об'єкт новою версією.
        _items[index] = element;
    }

    // REMOVE = видалити.
    public void Remove(T element)
    {
        // Спочатку через Read перевіряємо, чи такий елемент існує.
        T existingElement = Read(element.Id);
        _items.Remove(existingElement);
    }

    // ДОДАТКОВЕ ЗАВДАННЯ: SAVE = зберегти.
    // Серіалізація — перетворення об'єктів C# у формат, придатний для збереження.
    // Тут колекція перетворюється в текст JSON і записується у файл.
    public void Save(string filePath)
    {
        // Отримуємо папку з повного шляху до файлу.
        string? directory = Path.GetDirectoryName(filePath);

        // Якщо папка вказана і ще не існує — створюємо її.
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // WriteIndented = true робить JSON відформатованим і читабельним.
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        // Serialize = серіалізувати: List<T> -> JSON-рядок.
        string json = JsonSerializer.Serialize(_items, options);

        // Записуємо JSON у файл.
        File.WriteAllText(filePath, json);
    }

    // ДОДАТКОВЕ ЗАВДАННЯ: LOAD = завантажити.
    // Десеріалізація — зворотний процес: JSON -> об'єкти C#.
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл із даними не знайдено.", filePath);
        }

        string json = File.ReadAllText(filePath);

        // Deserialize відновлює List<T> із тексту JSON.
        List<T>? loadedItems = JsonSerializer.Deserialize<List<T>>(json);

        // Очищаємо поточну колекцію перед завантаженням з файлу.
        _items.Clear();

        if (loadedItems is not null)
        {
            _items.AddRange(loadedItems);
        }
    }
}
