using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MusicContest.Common.Models;
using MusicContest.Common.Services;
using Xunit;

// Модульні (unit) тести для ICrudServiceAsync<T>.
//
// Кожний тест створює свій сервіс із окремим тимчасовим файлом
// (Guid.NewGuid() гарантує унікальність), тому тести НЕ залежать
// від порядку виконання. Після кожного тесту файл видалюється.
namespace MusicContest.Common.Tests;

public class ICrudServiceAsyncTests
{
    // Helper: створити тимчасовий шлях для файлу.
    // Path.GetTempPath() = тимчасова папка Windows.
    // Guid.NewGuid() = унікальний ідентифікатор (щоб файли не зіпкалися).
    static string MakeTempPath(string suffix)
    {
        return Path.Combine(Path.GetTempPath(), $"lab2_test_{Guid.NewGuid():N}_{suffix}");
    }

    // 1. CreateAsync: додає елемент.
    [Fact]
    public async Task CreateAsync_AddsElement()
    {
        string path = MakeTempPath("create");
        var service = new CrudServiceAsync<Song>(path);
        var song = Song.CreateNew();
        bool result = await service.CreateAsync(song);
        Assert.True(result);
        int count = (await service.ReadAllAsync()).Count();
        Assert.Equal(1, count);
        File.Delete(path);
    }

    // 2. CreateAsync: дублікат Id -> InvalidOperationException.
    [Fact]
    public async Task CreateAsync_DuplicateId_ThrowsInvalidOperationException()
    {
        string path = MakeTempPath("dup");
        var service = new CrudServiceAsync<Song>(path);
        var song = Song.CreateNew();
        await service.CreateAsync(song);

        // Створюємо другий об'єкт з ТОЙ САМІм Id — це дублікат.
        var dup = new Song
        {
            Id = song.Id,
            Title = "dup",
            OriginalArtist = "x",
            DurationSeconds = 120,
            Key = "C major"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dup));

        File.Delete(path);
    }

    // 3. ReadAsync: повертає правильний елемент.
    [Fact]
    public async Task ReadAsync_ReturnsCorrectElement()
    {
        string path = MakeTempPath("read");
        var service = new CrudServiceAsync<Song>(path);
        var song = Song.CreateNew();
        await service.CreateAsync(song);
        var result = await service.ReadAsync(song.Id);
        Assert.Equal(song.Id, result.Id);
        File.Delete(path);
    }

    // 4. ReadAsync: невідомий Id -> KeyNotFoundException.
    [Fact]
    public async Task ReadAsync_UnknownId_ThrowsKeyNotFoundException()
    {
        string path = MakeTempPath("readunknown");
        var service = new CrudServiceAsync<Song>(path);
        var unknownId = new Guid();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ReadAsync(unknownId));
        File.Delete(path);
    }

    // 5. ReadAllAsync: порожня колекція -> порожній результат.
    [Fact]
    public async Task ReadAllAsync_Empty_ReturnsEmpty()
    {
        string path = MakeTempPath("empty");
        var service = new CrudServiceAsync<Song>(path);
        var items = await service.ReadAllAsync();
        Assert.Empty(items);
        File.Delete(path);
    }

    // 6. UpdateAsync: оновлює елемент.
    [Fact]
    public async Task UpdateAsync_UpdatesElement()
    {
        string path = MakeTempPath("update");
        var service = new CrudServiceAsync<Song>(path);
        var song = Song.CreateNew();
        await service.CreateAsync(song);

        song.Title = "NEW TITLE";
        await service.UpdateAsync(song);

        var result = await service.ReadAsync(song.Id);
        Assert.Equal("NEW TITLE", result.Title);
        File.Delete(path);
    }

    // 7. RemoveAsync: видалює елемент.
    [Fact]
    public async Task RemoveAsync_RemovesElement()
    {
        string path = MakeTempPath("remove");
        var service = new CrudServiceAsync<Song>(path);
        var song = Song.CreateNew();
        await service.CreateAsync(song);

        await service.RemoveAsync(song);
        int count = (await service.ReadAllAsync()).Count();
        Assert.Equal(0, count);
        File.Delete(path);
    }

    // 8. Pagination: правильна сторінка.
    // 10 елементів, page 2, amount 5 -> елементи 6-10 (5 елементів).
    [Fact]
    public async Task Pagination_ReturnsCorrectPage()
    {
        string path = MakeTempPath("pag");
        var service = new CrudServiceAsync<Song>(path);
        var allIds = new List<Guid>();
        for (int i = 0; i < 10; i++)
        {
            var song = Song.CreateNew();
            allIds.Add(song.Id);
            await service.CreateAsync(song);
        }

        var page2 = await service.ReadAllAsync(2, 5);
        Assert.Equal(5, page2.Count());
        // page 2 = елементи з індексу 5 до 9 (0-based),
        // тобто 6-й до 10-го об'єкта.
        var expected = allIds.Skip(5).Take(5);
        foreach (var id in expected)
        {
            // Assert.Contains перевіряє, що id є в колекції Id.
            Assert.Contains(id, page2.Select(x => x.Id));
        }
        File.Delete(path);
    }

    // 9. Pagination: сторінка за межами колекції -> порожньо.
    [Fact]
    public async Task Pagination_OutOfRange_ReturnsEmpty()
    {
        string path = MakeTempPath("pagout");
        var service = new CrudServiceAsync<Song>(path);
        for (int i = 0; i < 5; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }
        var page = await service.ReadAllAsync(10, 5);
        Assert.Empty(page);
        File.Delete(path);
    }

    // 10. Pagination: невалідна сторінка (<=0) -> ArgumentOutOfRangeException.
    [Fact]
    public async Task Pagination_InvalidPage_Throws()
    {
        string path = MakeTempPath("paginvpage");
        var service = new CrudServiceAsync<Song>(path);
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(0, 5));
        Assert.Contains("page", ex.Message, StringComparison.OrdinalIgnoreCase);
        File.Delete(path);
    }

    // 11. Pagination: невалідна кількість (<=0) -> ArgumentOutOfRangeException.
    [Fact]
    public async Task Pagination_InvalidAmount_Throws()
    {
        string path = MakeTempPath("paginvamount");
        var service = new CrudServiceAsync<Song>(path);
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(1, 0));
        Assert.Contains("amount", ex.Message, StringComparison.OrdinalIgnoreCase);
        File.Delete(path);
    }

    // 12. IEnumerable: foreach по сервісу працює.
    [Fact]
    public async Task IEnumerable_ForeachWorks()
    {
        string path = MakeTempPath("foreach");
        var service = new CrudServiceAsync<Song>(path);
        var ids = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var song = Song.CreateNew();
            ids.Add(song.Id);
            await service.CreateAsync(song);
        }
        int count = 0;
        foreach (var song in service)
        {
            count++;
            Assert.Contains(song.Id, ids);
        }
        Assert.Equal(3, count);
        File.Delete(path);
    }

    // 13. Thread safety: паралельне створення.
    // 8 потоків × 500 = 4000 об'єктів.
    [Fact]
    public async Task ThreadSafety_ParallelCreate()
    {
        string path = MakeTempPath("thread");
        var service = new CrudServiceAsync<Song>(path);
        const int total = 8 * 500;
        var tasks = new Task[total];
        for (int i = 0; i < total; i++)
        {
            var serviceLocal = service;
            tasks[i] = Task.Run(() =>
            {
                var song = Song.CreateNew();
                serviceLocal.CreateAsync(song).GetAwaiter().GetResult();
            });
        }

        // async-test: не використовувати Task.WaitAll (blocking operation).
        // Замість цього — очікуємо на кожну задачу через Task.WhenAll.
        await Task.WhenAll(tasks);
        int actual = (await service.ReadAllAsync()).Count();
        Assert.Equal(4000, actual);
        File.Delete(path);
    }

    // 14. SaveAsync: створює валідний JSON-файл.
    [Fact]
    public async Task SaveAsync_CreatesValidJsonFile()
    {
        string path = MakeTempPath("save");
        var service = new CrudServiceAsync<Song>(path);
        for (int i = 0; i < 10; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }
        bool saved = await service.SaveAsync();
        Assert.True(saved);
        Assert.True(File.Exists(path));
        var json = File.ReadAllText(path);
        var loaded = JsonSerializer.Deserialize<List<Song>>(json);
        Assert.NotNull(loaded);
        Assert.Equal(10, loaded.Count);
        File.Delete(path);
    }

    // 15. ConcurrentSaveAsync: одночасні збереження не коригують файл.
    // Кілька SaveAsync одночасно -> JSON має бути коректним після.
    [Fact]
    public async Task ConcurrentSaveAsync_DoesNotCorruptFile()
    {
        string path = MakeTempPath("concsave");
        var service = new CrudServiceAsync<Song>(path);
        for (int i = 0; i < 20; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }
        // 10 одночасних SaveAsync.
        var saves = Enumerable.Range(0, 10).Select(_ => service.SaveAsync());
        await Task.WhenAll(saves);
        var json = File.ReadAllText(path);
        var loaded = JsonSerializer.Deserialize<List<Song>>(json);
        Assert.NotNull(loaded);
        Assert.Equal(20, loaded.Count);
        File.Delete(path);
    }
}
