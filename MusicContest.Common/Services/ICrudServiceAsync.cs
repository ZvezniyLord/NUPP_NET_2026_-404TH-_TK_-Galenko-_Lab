using MusicContest.Common.Models;

namespace MusicContest.Common.Services;

// Контракт асинхронного CRUD-сервісу, який використовується в ЛР2.
//
// Методи повертають Task або Task<T>, що представляє виконання операції.
// Оператор await призупиняє поточний асинхронний метод на час очікування,
// не блокуючи потік під час асинхронного введення-виведення.
//
// Тип T задає модель, з якою працює сервіс. Обмеження IEntity гарантує,
// що кожна модель має унікальний ідентифікатор Guid Id.
//
// Реалізація також підтримує IEnumerable<T>, тому сервіс можна перебирати
// через foreach. Перебір використовує сформований список snapshot.
public interface ICrudServiceAsync<T> : IEnumerable<T> where T : IEntity
{
    // Додає елемент. Повертає false, якщо такий Id уже є.
    public Task<bool> CreateAsync(T element);

    // Повертає елемент за Guid. Якщо запису немає, генерує KeyNotFoundException.
    public Task<T> ReadAsync(Guid id);

    // Повертає впорядкований snapshot послідовності.
    // Це поверхнева копія: об'єкти посилальних типів не клонуються.
    public Task<IEnumerable<T>> ReadAllAsync();

    // Повертає сторінку з amount елементів. Нумерація починається з 1.
    // Некоректні page або amount спричиняють ArgumentOutOfRangeException.
    // Якщо сторінка виходить за межі колекції, повертається порожня послідовність.
    public Task<IEnumerable<T>> ReadAllAsync(int page, int amount);

    // Оновлює наявний елемент. Повертає false, якщо Id не знайдено.
    public Task<bool> UpdateAsync(T element);

    // Видаляє елемент за Id. Повертає false, якщо запису немає.
    public Task<bool> RemoveAsync(T element);

    // Асинхронно зберігає колекцію у JSON-файл за шляхом FilePath.
    // Доступ до запису захищено SemaphoreSlim.
    public Task<bool> SaveAsync();
}
