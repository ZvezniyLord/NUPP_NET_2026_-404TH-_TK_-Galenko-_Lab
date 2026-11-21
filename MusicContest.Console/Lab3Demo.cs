using Microsoft.EntityFrameworkCore;
using MusicContest.Infrastructure.Data;
using MusicContest.Infrastructure.Models;
using MusicContest.Infrastructure.Repositories;
using MusicContest.Infrastructure.Services;

namespace MusicContest.ConsoleApp;

// ЛР3 використовує справжню реляційну SQLite, а не список у пам'яті.
// Всі навчальні конкурсанти, гурти та пісні — українські.
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

        bool seeded = await UkrainianDemoSeeder.EnsureAsync(db);
        Console.WriteLine(seeded
            ? "Додано українських виконавців, гурти, пісні, профілі та виступи."
            : "Відкрито SQLite: демонстраційні записи вже існують.");

        var songs = new RepositoryCrudServiceAsync<SongModel>(new EfRepository<SongModel>(db));
        var performances = new RepositoryCrudServiceAsync<PerformanceModel>(
            new EfRepository<PerformanceModel>(db));

        // Справжні Create / Read / Update / Delete через EfRepository<T>.
        // Пробну пісню після демонстрації видаляємо.
        var trial = new SongModel
        {
            Title = "Пробна українська пісня",
            OriginalArtist = "Навчальний конкурс",
            DurationSeconds = 180, Key = "до мажор"
        };
        Console.WriteLine($"CREATE: {await songs.CreateAsync(trial)}");
        var read = await songs.ReadAsync(trial.Id);
        Console.WriteLine($"READ: {read.Title}, DatabaseId={read.DatabaseId}");
        read.Title = "Пробна українська пісня (оновлена)";
        Console.WriteLine($"UPDATE: {await songs.UpdateAsync(read)}");
        Console.WriteLine($"DELETE: {await songs.RemoveAsync(read)}");
        Console.WriteLine($"SAVE: {await songs.SaveAsync()}");

        Console.WriteLine("Перша сторінка, по 3 українські пісні:");
        foreach (var song in await songs.ReadAllAsync(1, 3))
            Console.WriteLine($"  Пісня #{song.DatabaseId}: {song.Title} — {song.OriginalArtist}");

        var people = await db.Participants.AsNoTracking()
            .Include(x => x.Profile)
            .Include(x => x.Songs)
            .Include(x => x.Performances)
            .OrderBy(x => x.DatabaseId).ToListAsync();
        foreach (var p in people)
        {
            Console.WriteLine($"Учасник #{p.DatabaseId}: {p.StageName}; " +
                $"країна={p.Country}; профіль={p.Profile is not null}; " +
                $"пісень M:N={p.Songs.Count}; виступів 1:N={p.Performances.Count}");
        }

        Console.WriteLine($"ContestParticipants: {people.Count}");
        Console.WriteLine($"TPT SoloSingers: {await db.SoloSingers.CountAsync()}");
        Console.WriteLine($"TPT VocalGroups: {await db.VocalGroups.CountAsync()}");
        Console.WriteLine($"Songs: {await db.Songs.CountAsync()}");
        Console.WriteLine($"ParticipantProfiles: {await db.ParticipantProfiles.CountAsync()}");
        Console.WriteLine($"Performances: {await db.Performances.CountAsync()}");
        Console.WriteLine($"ParticipantSongs: {await db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM ParticipantSongs").SingleAsync()}");
        Console.WriteLine($"SQLite: {MusicContestDatabase.PathToDatabase()}");
    }
}
