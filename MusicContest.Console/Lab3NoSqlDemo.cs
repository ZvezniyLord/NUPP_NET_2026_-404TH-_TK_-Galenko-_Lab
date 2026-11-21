using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MusicContest.Infrastructure.Services;
using MusicContest.Nosql.Models;
using MusicContest.Nosql.Repositories;

namespace MusicContest.ConsoleApp;

public static class Lab3NoSqlDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=========================================");
        Console.WriteLine("ЛР3 ДОДАТКОВЕ ЗАВДАННЯ: MongoDB Repository");
        Console.WriteLine("=========================================");

        string uri = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
                     ?? "mongodb://127.0.0.1:27017";
        var client = new MongoClient(uri);
        var database = client.GetDatabase("MusicContestLab3");
        var collection = database.GetCollection<SongDocument>("songs");
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<SongDocument>(
                Builders<SongDocument>.IndexKeys.Ascending(x => x.DatabaseId),
                new CreateIndexOptions { Unique = true }));

        var repository = new MongoRepository<SongDocument>(collection);
        var songs = new RepositoryCrudServiceAsync<SongDocument>(repository);

        if (!(await songs.ReadAllAsync()).Any())
        {
            await songs.CreateAsync(new SongDocument
            {
                DatabaseId = 1, Title = "Gravity", OriginalArtist = "Sara Bareilles",
                DurationSeconds = 234, Key = "F major",
                Stats = new SongStatsDocument { ListenerVotes = 250, Tags = new() { "pop", "ballad" } }
            });
            await songs.CreateAsync(new SongDocument
            {
                DatabaseId = 2, Title = "Skyfall", OriginalArtist = "Adele",
                DurationSeconds = 286, Key = "C minor",
                Stats = new SongStatsDocument { ListenerVotes = 420, Tags = new() { "soundtrack", "soul" } }
            });
        }

        var temp = new SongDocument
        {
            DatabaseId = 999, Title = "Temporary Mongo song",
            OriginalArtist = "Lab3", DurationSeconds = 165, Key = "A minor"
        };
        Console.WriteLine($"CREATE: {await songs.CreateAsync(temp)}");
        var read = await songs.ReadAsync(temp.Id);
        Console.WriteLine($"READ: {read.Title}");
        read.Title = "Temporary Mongo song (updated)";
        Console.WriteLine($"UPDATE: {await songs.UpdateAsync(read)}");
        Console.WriteLine($"DELETE: {await songs.RemoveAsync(read)}");

        foreach (var item in await songs.ReadAllAsync())
            Console.WriteLine($"MongoDB document: {item.DatabaseId}, {item.Title}, "
                + $"votes={item.Stats.ListenerVotes}, tags={string.Join("/", item.Stats.Tags)}");
        Console.WriteLine($"Documents count: {(await songs.ReadAllAsync()).Count()}");
        Console.WriteLine("MongoDB collection: MusicContestLab3.songs");
    }
}
