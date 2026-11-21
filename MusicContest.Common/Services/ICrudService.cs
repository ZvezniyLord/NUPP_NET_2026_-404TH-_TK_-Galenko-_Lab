namespace MusicContest.Common.Services;

// Узагальнений контракт CRUD-сервісу.
// Тип T визначається під час використання інтерфейсу.
public interface ICrudService<T>
{
    // Додає елемент.
    public void Create(T element);

    // Повертає елемент за Guid.
    public T Read(Guid id);

    // Повертає всі збережені елементи.
    public IEnumerable<T> ReadAll();

    // Оновлює наявний елемент.
    public void Update(T element);

    // Видаляє елемент.
    public void Remove(T element);
}
