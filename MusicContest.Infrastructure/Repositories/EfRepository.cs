using Microsoft.EntityFrameworkCore;
using MusicContest.Common.Services;
using MusicContest.Infrastructure.Data;

namespace MusicContest.Infrastructure.Repositories;

// Репозиторій приховує EF Core від CRUD-сервісу.
public sealed class EfRepository<T>(MusicContestContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    // EF.Property дозволяє звернутися до DatabaseId навіть для generic T.
    public async Task<T> GetByIdAsync(int id) =>
        await _set.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "DatabaseId") == id)
        ?? throw new KeyNotFoundException($"Не знайдено запис із DatabaseId={id}.");

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _set.AsNoTracking().ToListAsync();

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public Task Update(T entity)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(T entity)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        int changes = await context.SaveChangesAsync();
        // Кожна CRUD-операція працює з новим detached станом.
        // Це запобігає конфлікту двох екземплярів з однаковим ключем.
        context.ChangeTracker.Clear();
        return changes;
    }
}
