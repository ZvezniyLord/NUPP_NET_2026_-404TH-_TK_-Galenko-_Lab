using MongoDB.Driver;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

namespace MusicContest.Nosql.Repositories;

// Додаткове завдання ЛР3: той самий IRepository<T>, але замість EF — MongoDB.
public sealed class MongoRepository<T>(IMongoCollection<T> collection) : IRepository<T>
    where T : class, IEntity
{
    public async Task<T> GetByIdAsync(int id) =>
        await collection.Find(Builders<T>.Filter.Eq("DatabaseId", id)).FirstOrDefaultAsync()
        ?? throw new KeyNotFoundException($"MongoDB: DatabaseId={id} не знайдено.");

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await collection.Find(Builders<T>.Filter.Empty).ToListAsync();

    public async Task AddAsync(T entity) => await collection.InsertOneAsync(entity);

    public async Task Update(T entity)
    {
        var result = await collection.ReplaceOneAsync(GetDatabaseIdFilter(entity), entity);
        if (result.MatchedCount == 0)
            throw new KeyNotFoundException("MongoDB: документа для оновлення немає.");
    }

    public async Task Delete(T entity)
    {
        await collection.DeleteOneAsync(GetDatabaseIdFilter(entity));
    }

    // Використовуємо int DatabaseId — той самий ключ, що й у GetByIdAsync(int).
    // Фільтр за Guid (_id) може розрізняти BSON GUID representations.
    private static FilterDefinition<T> GetDatabaseIdFilter(T entity)
    {
        var property = typeof(T).GetProperty("DatabaseId")
            ?? throw new InvalidOperationException(
                "MongoRepository вимагає int властивість DatabaseId.");
        if (property.PropertyType != typeof(int))
            throw new InvalidOperationException("DatabaseId повинен мати тип int.");
        int id = (int)(property.GetValue(entity)
            ?? throw new InvalidOperationException("DatabaseId не встановлено."));
        return Builders<T>.Filter.Eq("DatabaseId", id);
    }

    // MongoDB Insert/Replace/Delete виконуються негайно, окремого SaveChanges немає.
    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
