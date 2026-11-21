namespace MusicContest.Common.Services;

// Контракт доступу до БД: CRUD-сервіс не залежить від конкретної СУБД.
// DatabaseId (int) є первинним ключем таблиці. Id (Guid) зберігає публічний ідентифікатор моделі.
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task Update(T entity);
    Task Delete(T entity);

    // Додатковий метод для завершення транзакції EF Core.
    Task<int> SaveChangesAsync();
}
