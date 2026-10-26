using System.Linq;
using MusicContest.Common.Models;
using MusicContest.Common.Services;
using Xunit;

namespace MusicContest.Common.Tests;

// Модульні (Unit) тести асинхронного CRUD-сервісу CrudServiceAsync<T>.
public class CrudServiceAsyncTests
{
    // ============================================================
    // CREATE
    // ============================================================

    // Тест 1: CreateAsync повертає true і реально додає елемент.
    [Fact]
    public async Task CreateAsync_AddsElement_ReturnsTrue()
    {
        CrudServiceAsync<Song> service = new();
        Song song = Song.CreateNew();

        bool created = await service.CreateAsync(song);

        Assert.True(created);

        List<Song> all = (await service.ReadAllAsync()).ToList();
        Assert.Contains(song, all);
        Assert.Single(all);
    }

    // Тест 2: duplicate (подвійний) Id → false (елемент не додано вдруге).
    [Fact]
    public async Task CreateAsync_DuplicateId_ReturnsFalse()
    {
        CrudServiceAsync<Performance> service = new();
        Performance first = Performance.CreateNew();

        Assert.True(await service.CreateAsync(first));

        // Другий об'єкт з ТИМ САМИМ Id, що в першого.
        Performance second = new Performance("x", "y", 10)
        {
            Id = first.Id
        };

        bool created = await service.CreateAsync(second);

        Assert.False(created);
        // У сервісі залишився ТИМ САМИЙ перший об'єкт (не замінений).
        Assert.Same(first, await service.ReadAsync(first.Id));
        Assert.Single((await service.ReadAllAsync()).ToList());
    }

    // ============================================================
    // READ
    // ============================================================

    // Тест 3: ReadAsync поверта той самий об'єкт (не дубль).
    [Fact]
    public async Task ReadAsync_ReturnsCorrectElement()
    {
        CrudServiceAsync<Song> service = new();
        Song expected = Song.CreateNew();
        await service.CreateAsync(expected);

        Song found = await service.ReadAsync(expected.Id);

        Assert.Same(expected, found);
    }

    // Тест 4: невідомий Id → KeyNotFoundException.
    [Fact]
    public async Task ReadAsync_UnknownId_ThrowsKeyNotFound()
    {
        CrudServiceAsync<Song> service = new();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ReadAsync(Guid.NewGuid()));
    }

    // ============================================================
    // READ ALL
    // ============================================================

    // Тест 5: порожній сервіс повертає ПОРОЖНЮ колекцію (не null).
    [Fact]
    public async Task ReadAllAsync_Empty_ReturnsEmpty()
    {
        CrudServiceAsync<Song> service = new();

        List<Song> all = (await service.ReadAllAsync()).ToList();

        Assert.Empty(all);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    // Тест 6: UpdateAsync повертає true і реально замінює елемент.
    [Fact]
    public async Task UpdateAsync_ReplacesElement_ReturnsTrue()
    {
        CrudServiceAsync<VocalGroup> service = new();
        VocalGroup original = VocalGroup.CreateNew();
        await service.CreateAsync(original);

        // Нова версія: той самий Id, інші дані.
        VocalGroup updatedVersion = new VocalGroup("Updated Name", "Updated Country", 30, 5, "Jazz", true)
        {
            Id = original.Id
        };

        bool replaced = await service.UpdateAsync(updatedVersion);

        Assert.True(replaced);

        VocalGroup readBack = await service.ReadAsync(original.Id);
        Assert.Same(updatedVersion, readBack);
        Assert.Equal("Updated Name", readBack.StageName);
    }

    // Тест 7: UpdateAsync на неіснуючий Id → false (не створює елемент).
    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsFalse()
    {
        CrudServiceAsync<VocalGroup> service = new();
        VocalGroup missing = VocalGroup.CreateNew();

        bool replaced = await service.UpdateAsync(missing);

        Assert.False(replaced);
        Assert.Empty((await service.ReadAllAsync()).ToList());
    }

    // ============================================================
    // REMOVE
    // ============================================================

    // Тест 8: RemoveAsync(element) повертає true та видаляє елемент.
    [Fact]
    public async Task RemoveAsync_RemovesElement_ReturnsTrue()
    {
        CrudServiceAsync<Song> service = new();
        Song toRemove = Song.CreateNew();
        await service.CreateAsync(toRemove);

        bool removed = await service.RemoveAsync(toRemove);

        Assert.True(removed);
        Assert.Empty((await service.ReadAllAsync()).ToList());
    }

    // Тест 9: RemoveAsync на неіснуючий елемент → false.
    [Fact]
    public async Task RemoveAsync_UnknownElement_ReturnsFalse()
    {
        CrudServiceAsync<Song> service = new();

        bool removed = await service.RemoveAsync(Song.CreateNew());

        Assert.False(removed);
    }

    // ============================================================
    // ПАГІНАЦІЯ (pagination)
    // ============================================================

    private static async Task<CrudServiceAsync<Song>> CreateServiceWith(int count)
    {
        // Helper: сервіс із count Song-ів для тестів пагінації.
        CrudServiceAsync<Song> service = new();
        for (int i = 0; i < count; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }

        return service;
    }

    // Тест 10: пагінація — page=2, amount=20 у колекції з 50 елементів
    // повертає елементи з позицій 21-40 того самого порядку, що ReadAllAsync.
    [Fact]
    public async Task ReadAllAsync_Pagination_ReturnsCorrectSlice()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(50);

        List<Song> page = (await service.ReadAllAsync(page: 2, amount: 20)).ToList();
        Assert.Equal(20, page.Count);

        // Детермінований порядок: обидва зчитування впорядковані за Id.
        List<Song> ordered = (await service.ReadAllAsync()).ToList();

        Assert.Same(ordered[20], page[0]);   // 21-й елемент — перший на сторінці 2
        Assert.Same(ordered[39], page[19]);  // 40-й елемент — останній на сторінці 2

        List<Song> firstPage = (await service.ReadAllAsync(page: 1, amount: 20)).ToList();
        Assert.NotSame(ordered[0], page[0]); // сторінка 1 — інша частина колекції
    }

    // Тест 11: сторінка за межами колекції → ПОРОЖНЯ колекція (не помилка).
    [Fact]
    public async Task ReadAllAsync_PaginationOutOfRange_ReturnsEmpty()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(10);

        List<Song> page = (await service.ReadAllAsync(page: 99, amount: 5)).ToList();

        Assert.Empty(page);
    }

    // Тест 12: невалідовий page (<= 0) → ArgumentOutOfRangeException.
    [Fact]
    public async Task ReadAllAsync_InvalidPage_Throws()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(5);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 0, amount: 5));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: -1, amount: 5));
    }

    // Тест 13: невалідовий amount (<= 0) → ArgumentOutOfRangeException.
    [Fact]
    public async Task ReadAllAsync_InvalidAmount_Throws()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(5);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 1, amount: 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 1, amount: -5));
    }

    // ============================================================
    // IENUMERABLE / FOREACH
    // ============================================================

    // Тест 14: сервіс перебирається foreach (var item in service).
    [Fact]
    public async Task Foreach_Iterates_AllElements()
    {
        CrudServiceAsync<ContestParticipant> service = new();
        ContestParticipant first = VocalGroup.CreateNew();
        ContestParticipant second = SoloSinger.CreateNew();
        await service.CreateAsync(first);
        await service.CreateAsync(second);

        int visited = 0;
        foreach (ContestParticipant item in service)
        {
            Assert.NotNull(item);
            visited++;
        }

        Assert.Equal(2, visited);
    }

    // ============================================================
    // THREAD SAFETY (поточно-безпека ConcurrentDictionary)
    // ============================================================

    // Тест 15: thread-safe паралельне створення: 8 потоків × 500 = 4000.
    [Fact]
    public async Task ParallelCreate_IsThreadSafe()
    {
        const int threadCount = 8;
        const int itemsPerThread = 500;
        const int expectedTotal = threadCount * itemsPerThread; // 4000

        CrudServiceAsync<Song> service = new();

        Task[] creators = new Task[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            creators[t] = Task.Run(async () =>
            {
                for (int i = 0; i < itemsPerThread; i++)
                {
                    // Кожен Song.CreateNew() має УНІКАЛЬНИЙ Guid Id,
                    // тому duplicate неможливий, а ConcurrentDictionary
                    // жодного елемента не губить.
                    await service.CreateAsync(Song.CreateNew());
                }
            });
        }

        await Task.WhenAll(creators);

        List<Song> all = (await service.ReadAllAsync()).ToList();
        Assert.Equal(expectedTotal, all.Count);

        int uniqueIds = all.Select(x => x.Id).Distinct().Count();
        Assert.Equal(expectedTotal, uniqueIds);
    }

    // ============================================================
    // SAVEASYNC
    // ============================================================

    private static string GetTempJsonFile()
    {
        // Унікальний тимчасовий файл: кожен тест — зі своїм файлом.
        return Path.Combine(
            Path.GetTempPath(),
            $"musiccontest_test_{Guid.NewGuid():N}.json");
    }

    private static void DeleteTempFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Файлу не було — нічого чистити.
        }
    }

    // Тест 16: SaveAsync повертає true та записує ВАЛІДНИЙ JSON зі справжніми даними.
    [Fact]
    public async Task SaveAsync_WritesValidJson_ReturnsTrue()
    {
        string filePath = GetTempJsonFile();
        CrudServiceAsync<Song> service = new(filePath);

        Song song = Song.CreateNew();
        await service.CreateAsync(song);

        try
        {
            bool saved = await service.SaveAsync();
            Assert.True(saved);
            Assert.True(File.Exists(filePath));

            string json = await File.ReadAllTextAsync(filePath);
            List<Song>? deserialized =
                System.Text.Json.JsonSerializer.Deserialize<List<Song>>(json);

            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            // Id і назва збережені 1:1.
            Assert.Equal(song.Id, deserialized![0].Id);
            Assert.Equal(song.Title, deserialized[0].Title);
        }
        finally
        {
            DeleteTempFile(filePath);
        }
    }

    // Тест 17: SaveAsync без FilePath → InvalidOperationException.
    [Fact]
    public async Task SaveAsync_NoFilePath_Throws()
    {
        CrudServiceAsync<Song> service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveAsync());
    }

    // Тест 18: конкурентні SaveAsync на ОДИН файл — фінальний JSON валідний
    // і містить рівно стільки елементів, скільки у сервісі (не "подвійно").
    [Fact]
    public async Task ConcurrentSaveAsync_FinalFileIsValid()
    {
        const int elementCount = 50;
        const int concurrentSaveCount = 10;

        string filePath = GetTempJsonFile();
        CrudServiceAsync<Song> service = new(filePath);
        for (int i = 0; i < elementCount; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }

        try
        {
            Task[] saves = new Task[concurrentSaveCount];
            for (int i = 0; i < concurrentSaveCount; i++)
            {
                saves[i] = service.SaveAsync();
            }

            await Task.WhenAll(saves);

            string json = await File.ReadAllTextAsync(filePath);
            List<Song>? deserialized =
                System.Text.Json.JsonSerializer.Deserialize<List<Song>>(json);

            Assert.NotNull(deserialized);
            Assert.Equal(elementCount, deserialized!.Count);
        }
        finally
        {
            DeleteTempFile(filePath);
        }
    }

    // ============================================================
    // PARALLELL (паралельне створення через CRUD-сервіс)
    // ============================================================

    // Тест 19: Parallell.RunAsync створює рівно count об'єктів у сервісі.
    [Fact]
    public async Task Parallell_RunAsync_CreatesAllObjectsInService()
    {
        const int count = 1500; // >= 1000 за умовою завдання

        CrudServiceAsync<Song> service = new();
        ParallelCreateResult result = await Parallell.RunAsync(service, count);

        Assert.Equal(count, result.ActualCount);   // реальна кількість з сервісу
        Assert.Equal(count, result.RequestedCount);
        Assert.True(result.Elapsed.TotalMilliseconds > 0);

        int uniqueIds = (await service.ReadAllAsync()).Select(x => x.Id).Distinct().Count();
        Assert.Equal(count, uniqueIds);
    }
}
