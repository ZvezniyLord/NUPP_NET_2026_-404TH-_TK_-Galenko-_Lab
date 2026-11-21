using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Сервіс працює з типами, що реалізують IEntity.
public class CrudService<T> : ICrudService<T> where T : IEntity
{
    private readonly List<T> _items = new();

    public void Create(T element)
    {
        if (_items.Any(x => x.Id == element.Id))
        {
            throw new InvalidOperationException($"Елемент з Id {element.Id} уже існує.");
        }

        _items.Add(element);
    }

    public T Read(Guid id)
    {
        return _items.FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
    }

    public IEnumerable<T> ReadAll()
    {
        // ToList() повертає копію списку, щоб зовнішній код
        // не отримав прямий доступ до внутрішньої колекції _items.
        return _items.ToList();
    }

    public void Update(T element)
    {
        int index = _items.FindIndex(x => x.Id == element.Id);

        if (index < 0)
        {
            throw new KeyNotFoundException($"Елемент з Id {element.Id} не знайдено.");
        }

        _items[index] = element;
    }

    public void Remove(T element)
    {
        T existingElement = Read(element.Id);
        _items.Remove(existingElement);
    }

    // Збереження колекції у форматі JSON.
    public void Save(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(_items, options);

        File.WriteAllText(filePath, json);
    }

    // Завантаження колекції з JSON.
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл із даними не знайдено.", filePath);
        }

        string json = File.ReadAllText(filePath);

        List<T>? loadedItems = JsonSerializer.Deserialize<List<T>>(json);

        _items.Clear();

        if (loadedItems is not null)
        {
            _items.AddRange(loadedItems);
        }
    }
}
