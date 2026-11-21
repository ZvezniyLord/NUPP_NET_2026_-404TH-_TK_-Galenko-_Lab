using Microsoft.EntityFrameworkCore;
using MusicContest.Infrastructure.Data;
using MusicContest.Infrastructure.Models;
using MusicContest.Infrastructure.Repositories;
using MusicContest.Infrastructure.Services;

namespace MusicContest.ConsoleApp;

// Демонстрація з реальним SQLite-файлом, а не колекцією в пам'яті.
public static class Lab3Demo
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=========================================");
        Console.WriteLine("ЛАБОРАТОРНА РОБОТА №3: EF Core + SQLite");
        Console.WriteLine("=========================================");

        await using var db = new MusicContestContext(MusicContestDatabase.Options());
        await db.Database.MigrateAsync();

        var songs = new RepositoryCrudServiceAsync<SongModel>(new EfRepository<SongModel>(db));
        var participants = new RepositoryCrudServiceAsync<ContestParticipantModel>(
            new EfRepository<ContestParticipantModel>(db));
        var performances = new RepositoryCrudServiceAsync<PerformanceModel>(
            new EfRepository<PerformanceModel>(db));

        if (!await db.Participants.AnyAsync())
        {
            var songA = new SongModel
            {
                Title = "Gravity", OriginalArtist = "Sara Bareilles",
                DurationSeconds = 234, Key = "F major"
            };
            var songB = new SongModel
            {
                Title = "Skyfall", OriginalArtist = "Adele",
                DurationSeconds = 286, Key = "C minor"
            };
            await songs.CreateAsync(songA);
            await songs.CreateAsync(songB);

            var singer = new SoloSingerModel
            {
                StageName = "Astra", Country = "Україна", Age = 21,
                VoiceType = "сопрано", VocalRange = "A3-E6", YearsOfExperience = 8,
                Profile = new ParticipantProfileModel
                {
                    Biography = "Сольна учасниця музичного конкурсу",
                    ContactEmail = "astra@example.test"
                }
            };
            var group = new VocalGroupModel
            {
                StageName = "Northern Lights", Country = "Україна", Age = 24,
                MemberCount = 4, Genre = "pop / soul", HasBackingVocals = true,
                Profile = new ParticipantProfileModel
                {
                    Biography = "Вокальний квартет",
                    ContactEmail = "group@example.test"
                }
            };
            await participants.CreateAsync(singer);
            await participants.CreateAsync(group);

            await performances.CreateAsync(new PerformanceModel
            {
                ParticipantDatabaseId = singer.DatabaseId,
                SongDatabaseId = songA.DatabaseId,
                Score = 92.0, IsCompleted = true
            });
            await performances.CreateAsync(new PerformanceModel
            {
                ParticipantDatabaseId = group.DatabaseId,
                SongDatabaseId = songB.DatabaseId,
                Score = 89.5, IsCompleted = true
            });

            // Створення M:N у таблиці ParticipantSongs через навігації EF.
            var singerDb = await db.Participants.Include(x => x.Songs)
                .SingleAsync(x => x.DatabaseId == singer.DatabaseId);
            var groupDb = await db.Participants.Include(x => x.Songs)
                .SingleAsync(x => x.DatabaseId == group.DatabaseId);
            var songDbA = await db.Songs.SingleAsync(x => x.DatabaseId == songA.DatabaseId);
            var songDbB = await db.Songs.SingleAsync(x => x.DatabaseId == songB.DatabaseId);
            singerDb.Songs.Add(songDbA);
            singerDb.Songs.Add(songDbB);
            groupDb.Songs.Add(songDbA);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            Console.WriteLine("Створено початкові записи учасників, пісень, виступів і профілів.");
        }
        else
            Console.WriteLine("Відкрито раніше створену SQLite базу.");

        // Реальні CRUD-операції через універсальний сервіс і репозиторій.
        var trial = new SongModel
        {
            Title = "Temporary song", OriginalArtist = "Lab3",
            DurationSeconds = 180, Key = "C major"
        };
        Console.WriteLine($"CREATE: {await songs.CreateAsync(trial)}");
        SongModel read = await songs.ReadAsync(trial.Id);
        Console.WriteLine($"READ: {read.Title}, DatabaseId={read.DatabaseId}");
        read.Title = "Temporary song (updated)";
        Console.WriteLine($"UPDATE: {await songs.UpdateAsync(read)}");
        Console.WriteLine($"DELETE: {await songs.RemoveAsync(read)}");
        Console.WriteLine($"SAVE: {await songs.SaveAsync()}");

        Console.WriteLine("Сторінка 1, по 2 пісні:");
        foreach (var s in await songs.ReadAllAsync(1, 2))
            Console.WriteLine($"  Song #{s.DatabaseId}: {s.Title}");

        var people = await db.Participants.AsNoTracking()
            .Include(x => x.Profile)
            .Include(x => x.Songs)
            .Include(x => x.Performances).OrderBy(x => x.DatabaseId).ToListAsync();
        foreach (var p in people)
            Console.WriteLine($"Учасник #{p.DatabaseId}: {p.StageName}; "
                + $"профіль={p.Profile is not null}; пісень M:N={p.Songs.Count}; виступів 1:N={p.Performances.Count}");

        Console.WriteLine($"TPT SoloSingers: {await db.SoloSingers.CountAsync()}");
        Console.WriteLine($"TPT VocalGroups: {await db.VocalGroups.CountAsync()}");
        Console.WriteLine($"Songs: {await db.Songs.CountAsync()}");
        Console.WriteLine($"Performances: {await db.Performances.CountAsync()}");
        Console.WriteLine($"ParticipantSongs: {await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM ParticipantSongs").SingleAsync()}");
        Console.WriteLine($"SQLite: {MusicContestDatabase.PathToDatabase()}");
    }
}
