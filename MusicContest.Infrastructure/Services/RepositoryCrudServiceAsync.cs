using System.Collections;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

namespace MusicContest.Infrastructure.Services;

// ЛР3: версія generic CRUD, що працює з IRepository<T>, а не з List/JSON.
// Оригінальний CrudServiceAsync<T> ЛР2 залишений для сумісності з її тестами.
public sealed class RepositoryCrudServiceAsync<T>(IRepository<T> repository) : ICrudServiceAsync<T>
    where T : class, IEntity
{
    public async Task<bool> CreateAsync(T element)
    {
        if ((await ReadAllAsync()).Any(x => x.Id == element.Id))
            return false;

        await repository.AddAsync(element);
        return await SaveAsync();
    }

    public async Task<T> ReadAsync(Guid id)
    {
        return (await ReadAllAsync()).FirstOrDefault(x => x.Id == id)
               ?? throw new KeyNotFoundException($"Елемент з Id={id} не знайдено.");
    }

    public async Task<IEnumerable<T>> ReadAllAsync()
    {
        IEnumerable<T> items = await repository.GetAllAsync();
        return items.OrderBy(x => x.Id).ToList();
    }

    public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
        return (await ReadAllAsync()).Skip(checked((page - 1) * amount))
                                     .Take(amount).ToList();
    }

    public async Task<bool> UpdateAsync(T element)
    {
        if (!(await ReadAllAsync()).Any(x => x.Id == element.Id))
            return false;
        await repository.Update(element);
        return await SaveAsync();
    }

    public async Task<bool> RemoveAsync(T element)
    {
        // Використовуємо запис із БД, а не довіряємо DatabaseId від клієнта.
        T? existing = (await ReadAllAsync()).FirstOrDefault(x => x.Id == element.Id);
        if (existing is null) return false;
        await repository.Delete(existing);
        return await SaveAsync();
    }

    public async Task<bool> SaveAsync() => (await repository.SaveChangesAsync()) >= 0;

    // IEnumerable<T> лишився для сумісності з контрактом лабораторної №2.
    // У UI-застосунках слід віддавати перевагу ReadAllAsync(), а не цьому sync мосту.
    public IEnumerator<T> GetEnumerator() =>
        ReadAllAsync().GetAwaiter().GetResult().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
