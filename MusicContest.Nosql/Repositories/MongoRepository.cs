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
        var result = await collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        if (result.MatchedCount == 0)
            throw new KeyNotFoundException("MongoDB: документа для оновлення немає.");
    }

    public async Task Delete(T entity) =>
        await collection.DeleteOneAsync(x => x.Id == entity.Id);

    // MongoDB Insert/Replace/Delete виконуються негайно, окремого SaveChanges немає.
    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
