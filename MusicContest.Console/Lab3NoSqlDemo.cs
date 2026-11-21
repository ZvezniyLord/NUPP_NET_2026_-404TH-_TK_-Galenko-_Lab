using MongoDB.Driver;
using MusicContest.Common.Data;
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

        var songs = new RepositoryCrudServiceAsync<SongDocument>(
            new MongoRepository<SongDocument>(collection));

        // Усі збережені зразки репертуару належать до української музики.
        // Перезаписуються тільки записи нашого демонстраційного набору (id 1..8).
        var current = (await songs.ReadAllAsync()).ToList();
        for (int i = 0; i < UkrainianMusicData.Songs.Length; i++)
        {
            var seed = UkrainianMusicData.Songs[i];
            var existing = current.FirstOrDefault(x => x.DatabaseId == i + 1);
            if (existing is not null)
            {
                existing.Title = seed.Title;
                existing.OriginalArtist = seed.OriginalArtist;
                existing.DurationSeconds = seed.DurationSeconds;
                existing.Key = seed.Key;
                existing.Stats = new SongStatsDocument
                {
                    ListenerVotes = 250 + i * 30,
                    Tags = new() { "українська музика", "конкурсний репертуар" }
                };
                await songs.UpdateAsync(existing);
            }
            else
            {
                await songs.CreateAsync(new SongDocument
                {
                    DatabaseId = i + 1, Title = seed.Title,
                    OriginalArtist = seed.OriginalArtist,
                    DurationSeconds = seed.DurationSeconds, Key = seed.Key,
                    Stats = new SongStatsDocument
                    {
                        ListenerVotes = 250 + i * 30,
                        Tags = new() { "українська музика", "конкурсний репертуар" }
                    }
                });
            }
        }

        var temp = new SongDocument
        {
            DatabaseId = 999, Title = "Пробна українська пісня",
            OriginalArtist = "Навчальний конкурс", DurationSeconds = 165,
            Key = "ля мінор"
        };
        Console.WriteLine($"CREATE: {await songs.CreateAsync(temp)}");
        var read = await songs.ReadAsync(temp.Id);
        Console.WriteLine($"READ: {read.Title}");
        read.Title = "Пробна українська пісня (оновлена)";
        Console.WriteLine($"UPDATE: {await songs.UpdateAsync(read)}");
        Console.WriteLine($"DELETE: {await songs.RemoveAsync(read)}");

        foreach (var item in (await songs.ReadAllAsync()).OrderBy(x => x.DatabaseId))
            Console.WriteLine($"MongoDB документ: {item.DatabaseId}, {item.Title}; виконавець: " +
                $"{item.OriginalArtist}; голосів={item.Stats.ListenerVotes}; " +
                $"теги={string.Join("/", item.Stats.Tags)}");
        Console.WriteLine($"Documents count: {(await songs.ReadAllAsync()).Count()}");
        Console.WriteLine("MongoDB collection: MusicContestLab3.songs");
    }
}
