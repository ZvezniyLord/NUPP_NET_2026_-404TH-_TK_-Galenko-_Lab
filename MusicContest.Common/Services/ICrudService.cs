namespace MusicContest.Common.Services;

// Generic-інтерфейс CRUD.
// <T> означає "тип буде визначено пізніше".
// Один і той самий контракт можна застосувати до різних типів об'єктів.
//
// CRUD:
// Create = створити / додати;
// Read = прочитати / знайти;
// Update = оновити;
// Delete/Remove = видалити.
public interface ICrudService<T>
{
    // Додає один елемент.
    public void Create(T element);

    // Повертає один елемент за його Guid.
    public T Read(Guid id);

    // IEnumerable<T> — послідовність елементів типу T.
    // Метод повертає всі збережені об'єкти.
    public IEnumerable<T> ReadAll();

    // Оновлює наявний елемент.
    public void Update(T element);

    // Видаляє елемент.
    public void Remove(T element);
}
