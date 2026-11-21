using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MusicContest.Infrastructure.Data;
using MusicContest.Common.Data;
using MusicContest.Infrastructure.Models;
using MusicContest.Infrastructure.Repositories;
using MusicContest.Infrastructure.Services;
using Xunit;

namespace MusicContest.Common.Tests;

public class Lab3DatabaseTests
{
    private static async Task<(SqliteConnection Connection, MusicContestContext Context)> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MusicContestContext>()
            .UseSqlite(connection).Options;
        var db = new MusicContestContext(options);
        await db.Database.MigrateAsync();
        return (connection, db);
    }

    [Fact]
    public async Task UkrainianSeederCreatesRealisticRelationalSampleAndIsIdempotent()
    {
        var (connection, db) = await CreateAsync();
        await using (connection)
        await using (db)
        {
            Assert.True(await UkrainianDemoSeeder.EnsureAsync(db));
            Assert.False(await UkrainianDemoSeeder.EnsureAsync(db));

            var actualNames = await db.Participants.Select(x => x.StageName).ToListAsync();
            var actualSongs = await db.Songs.Select(x => x.Title).ToListAsync();

            Assert.Equal(UkrainianMusicData.Singers.Length + UkrainianMusicData.Groups.Length,
                actualNames.Count);
            Assert.Equal(UkrainianMusicData.Songs.Length, actualSongs.Count);
            Assert.All(actualNames, name => Assert.DoesNotContain("Astra", name));
            Assert.All(actualSongs, title => Assert.DoesNotContain("Gravity", title));
            Assert.Contains("Олена Ковальчук", actualNames);
            Assert.Contains("Червона рута", actualSongs);
            Assert.Equal(actualNames.Count, await db.ParticipantProfiles.CountAsync());
            Assert.Equal(actualNames.Count * 2, await db.Performances.CountAsync());
            Assert.Equal(actualNames.Count * 2,
                await db.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM ParticipantSongs").SingleAsync());
        }
    }

    [Fact]
    public async Task MigrationCreatesRelationalTables()
    {
        var (connection, db) = await CreateAsync();
        await using (connection)
        await using (db)
        {
            var tableNames = await db.Database.SqlQueryRaw<string>(
                "SELECT name AS Value FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'"
            ).ToListAsync();

            Assert.Contains("ContestParticipants", tableNames);
            Assert.Contains("SoloSingers", tableNames);
            Assert.Contains("VocalGroups", tableNames);
            Assert.Contains("ParticipantProfiles", tableNames);
            Assert.Contains("Songs", tableNames);
            Assert.Contains("Performances", tableNames);
            Assert.Contains("ParticipantSongs", tableNames);
            Assert.Contains("__EFMigrationsHistory", tableNames);
        }
    }

    [Fact]
    public async Task GenericRepositoryCrudWorksAgainstSqlite()
    {
        var (connection, db) = await CreateAsync();
        await using (connection)
        await using (db)
        {
            var repository = new EfRepository<SongModel>(db);
            var service = new RepositoryCrudServiceAsync<SongModel>(repository);
            var song = new SongModel { Title = "Test", DurationSeconds = 210, Key = "C" };

            Assert.True(await service.CreateAsync(song));
            Assert.False(await service.CreateAsync(song));
            Assert.Equal("Test", (await service.ReadAsync(song.Id)).Title);
            Assert.Equal(song.Id, (await repository.GetByIdAsync(song.DatabaseId)).Id);

            song.Title = "Updated";
            Assert.True(await service.UpdateAsync(song));
            Assert.Equal("Updated", (await service.ReadAsync(song.Id)).Title);
            Assert.True(await service.RemoveAsync(song));
            Assert.Empty(await service.ReadAllAsync());
            Assert.False(await service.RemoveAsync(song));
        }
    }

    [Fact]
    public async Task FluentRelationshipsAndTptPersist()
    {
        var (connection, db) = await CreateAsync();
        await using (connection)
        await using (db)
        {
            var singer = new SoloSingerModel
            {
                StageName = "Singer", Country = "UA", Age = 22,
                VoiceType = "alto", VocalRange = "G3-D5",
                Profile = new ParticipantProfileModel { Biography = "Bio" }
            };
            var group = new VocalGroupModel
            {
                StageName = "Group", Country = "UA", Age = 20, Genre = "pop", MemberCount = 4
            };
            var song = new SongModel { Title = "Example", Key = "G", DurationSeconds = 200 };
            db.Participants.AddRange(singer, group);
            db.Songs.Add(song);
            await db.SaveChangesAsync();

            singer.Songs.Add(song);
            group.Songs.Add(song);
            db.Performances.Add(new PerformanceModel
            {
                Participant = singer, Song = song, Score = 87, IsCompleted = true
            });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var loaded = await db.Participants.AsNoTracking()
                .Include(x => x.Profile).Include(x => x.Songs).Include(x => x.Performances)
                .SingleAsync(x => x.Id == singer.Id);
            Assert.IsType<SoloSingerModel>(loaded);
            Assert.NotNull(loaded.Profile);
            Assert.Single(loaded.Songs);
            Assert.Single(loaded.Performances);
            Assert.Equal(1, await db.SoloSingers.CountAsync());
            Assert.Equal(1, await db.VocalGroups.CountAsync());
            Assert.Equal(2, await db.Participants.CountAsync());
            Assert.Equal(2, await db.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM ParticipantSongs").SingleAsync());
        }
    }

    [Fact]
    public async Task PaginationAndGuidReadAreConsistent()
    {
        var (connection, db) = await CreateAsync();
        await using (connection)
        await using (db)
        {
            var service = new RepositoryCrudServiceAsync<SongModel>(new EfRepository<SongModel>(db));
            await service.CreateAsync(new SongModel
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Title = "First"
            });
            await service.CreateAsync(new SongModel
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Title = "Second"
            });
            Assert.Single(await service.ReadAllAsync(2, 1));
            Assert.Empty(await service.ReadAllAsync(3, 1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.ReadAllAsync(0, 2));
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.ReadAsync(Guid.NewGuid()));
        }
    }
}
