using System.Text.Json;
using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

public class CrudService<T> : ICrudService<T> where T : IEntity
{
    private readonly List<T> _items = new();

    // Метод CRUD: Create
    public void Create(T element)
    {
        if (_items.Any(x => x.Id == element.Id))
        {
            throw new InvalidOperationException($"Елемент з Id {element.Id} уже існує.");
        }

        _items.Add(element);
    }

    // Метод CRUD: Read
    public T Read(Guid id)
    {
        return _items.FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Елемент з Id {id} не знайдено.");
    }

    // Метод CRUD: ReadAll
    public IEnumerable<T> ReadAll()
    {
        return _items.ToList();
    }

    // Метод CRUD: Update
    public void Update(T element)
    {
        int index = _items.FindIndex(x => x.Id == element.Id);

        if (index < 0)
        {
            throw new KeyNotFoundException($"Елемент з Id {element.Id} не знайдено.");
        }

        _items[index] = element;
    }

    // Метод CRUD: Remove
    public void Remove(T element)
    {
        T existingElement = Read(element.Id);
        _items.Remove(existingElement);
    }

    // Додаткове завдання: Save
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

    // Додаткове завдання: Load
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
