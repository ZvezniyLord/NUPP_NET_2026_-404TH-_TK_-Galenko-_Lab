using System.Linq;
using MusicContest.Common.Models;
using MusicContest.Common.Services;
using Xunit;

namespace MusicContest.Common.Tests;

// Тести асинхронного CRUD-сервісу (async generic CRUD).
public class CrudServiceAsyncTests
{
    // ============================================================
    // CREATE
    // ============================================================

    // Тест 1: CreateAsync додає елемент (після створення сервіс
    // насправді містить цей елемент).
    [Fact]
    public async Task CreateAsync_AddsElement()
    {
        CrudServiceAsync<Song> service = new();
        Song song = Song.CreateNew();

        await service.CreateAsync(song);

        List<Song> all = await service.ReadAllAsync();

        // Assert: перевірка, що елемент НАСПРАВДІ у колекції.
        Assert.Contains(song, all);
        Assert.Single(all);
    }

    // Тест 2: duplicate (подвійний) Id → InvalidOperationException.
    [Fact]
    public async Task CreateAsync_DuplicateId_Throws()
    {
        CrudServiceAsync<Performance> service = new();
        Performance first = Performance.CreateNew();
        Performance second = new Performance("x", "y", 10)
        {
            // Другий об'єкт з ТИМ САМИМ Id, що в першого.
            Id = first.Id
        };

        await service.CreateAsync(first);

        // Assert.ThrowsAsync: перевіряє, що метод НАСПРАВДІ кинув
        // виключення саме цього типу.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(second));
    }

    // ============================================================
    // READ
    // ============================================================

    // Тест 3: ReadAsync повертає правильний елемент (той самий об'єкт,
    // а не дубль і не інший об'єкт).
    [Fact]
    public async Task ReadAsync_ReturnsCorrectElement()
    {
        CrudServiceAsync<Song> service = new();
        Song expected = Song.CreateNew();
        await service.CreateAsync(expected);

        Song? found = await service.ReadAsync(expected.Id);

        Assert.Same(expected, found);
    }

    // Тест 4: Unknown (відомий) Id → KeyNotFoundException.
    [Fact]
    public async Task ReadAsync_UnknownId_ThrowsKeyNotFound()
    {
        CrudServiceAsync<Song> service = new();
        Guid unknownId = Guid.NewGuid();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ReadAsync(unknownId));
    }

    // ============================================================
    // READ ALL
    // ============================================================

    // Тест 5: Empty (порожній) ReadAllAsync — сервіс без елементів
    // повертає POROZHNYU (порожню) колекцію, а не null і не виключення.
    [Fact]
    public async Task ReadAllAsync_Empty_ReturnsEmptyList()
    {
        CrudServiceAsync<Song> service = new();

        List<Song> all = await service.ReadAllAsync();

        Assert.Empty(all);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    // Тест 6: UpdateAsync реально оновлює елемент
    // (після оновлення ReadAsync повертає НОВУ версію).
    [Fact]
    public async Task UpdateAsync_ReplacesElement()
    {
        CrudServiceAsync<VocalGroup> service = new();
        VocalGroup original = VocalGroup.CreateNew();
        await service.CreateAsync(original);

        // Нова версія: той самий Id, інші дані.
        VocalGroup updatedVersion = new VocalGroup("Updated Name", "Updated Country", 30, 5, "Jazz", true)
        {
            // ВАЖЛИВО (важливо): зберігаємо той самий Id,
            // щоб сервіс знайшов, що саме цей елемент оновити.
            Id = original.Id
        };

        await service.UpdateAsync(updatedVersion);

        VocalGroup? readBack = await service.ReadAsync(original.Id);

        Assert.Same(updatedVersion, readBack);
        Assert.Equal("Updated Name", readBack.StageName);
    }

    // Додатково: UpdateAsync на неіснуючий Id → KeyNotFoundException.
    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        CrudServiceAsync<VocalGroup> service = new();
        VocalGroup missing = VocalGroup.CreateNew();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(missing));
    }

    // ============================================================
    // REMOVE
    // ============================================================

    // Тест 7: RemoveAsync видаляє елемент (після видалення сервіс
    // більше його не містить).
    [Fact]
    public async Task RemoveAsync_RemovesElement()
    {
        CrudServiceAsync<Song> service = new();
        Song toRemove = Song.CreateNew();
        await service.CreateAsync(toRemove);

        await service.RemoveAsync(toRemove.Id);

        List<Song> all = await service.ReadAllAsync();
        Assert.Empty(all);
    }

    // Додатково: RemoveAsync на неіснуючий Id → KeyNotFoundException.
    [Fact]
    public async Task RemoveAsync_UnknownId_Throws()
    {
        CrudServiceAsync<Song> service = new();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RemoveAsync(Guid.NewGuid()));
    }

    // ============================================================
    // ПАГІНАЦІЯ (pagination)
    // ============================================================

    private static async Task<CrudServiceAsync<Song>> CreateServiceWith(int count)
    {
        // Допомагаючий (helper) метод: створює сервіс із count
        // (кількістю) Song-ів для повторного використання в тестах
        // пагінації.
        CrudServiceAsync<Song> service = new();
        for (int i = 0; i < count; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }

        return service;
    }

    // Тест 8: пагінація — page=2, amount=20 у колекції з 50 елементів
    // повертає елементи з порядковими номерами 21-40.
    [Fact]
    public async Task ReadAllAsync_Pagination_ReturnsCorrectSlice()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(50);

        List<Song> page = await service.ReadAllAsync(page: 2, amount: 20);

        // Елементи з позицій 20 по 39 (0-based) = з 21 по 40 (1-based).
        Assert.Equal(20, page.Count);

        // Page (сторінка) 1 — зовсім інша збірна (інша половина).
        List<Song> firstPage = await service.ReadAllAsync(page: 1, amount: 20);

        // Перший елемент сторінки 2 == 21-й елемент (індекс 20).
        List<Song> ordered = (await service.ReadAllAsync());
        Assert.Same(ordered[20], page[0]);
        Assert.Same(ordered[39], page[19]);
        // А перший елемент сторінки 1 НЕ збігається з 21-м елементом.
        Assert.NotSame(ordered[0], page[0]);
    }

    // Тест 9: pagination (пагінація) за межами (out of range)
    // колекції повертає ПОРОЖНЮ колекцію (не виключення).
    [Fact]
    public async Task ReadAllAsync_PaginationOutOfRange_ReturnsEmpty()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(10);

        // Sторінка (page) 99 — далеко за межами 10 елементів.
        List<Song> page = await service.ReadAllAsync(page: 99, amount: 5);

        Assert.Empty(page);
    }

    // Тест 10: invalid (невірний) page (page <= 0) → ArgumentOutOfRangeException.
    [Fact]
    public async Task ReadAllAsync_InvalidPage_Throws()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(5);

        // page = 0 і page = -1 — обидва невалідові (неправильні).
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 0, amount: 5));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: -1, amount: 5));
    }

    // Тест 11: invalid (невірний) amount (amount <= 0)
    // → ArgumentOutOfRangeException.
    [Fact]
    public async Task ReadAllAsync_InvalidAmount_Throws()
    {
        CrudServiceAsync<Song> service = await CreateServiceWith(5);

        // amount = 0 і amount = -5 — обидва невалідові.
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 1, amount: 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ReadAllAsync(page: 1, amount: -5));
    }

    // ============================================================
    // IENUMERABLE / FOREACH (foreach = цикл перебору)
    // ============================================================

    // Тест 12: IEnumerable/foreach — сервіс можна перебирати
    // циклом foreach (var item in service) і отримати ВСІ елементи.
    [Fact]
    public async Task Foreach_Iterates_AllElements()
    {
        CrudServiceAsync<ContestParticipant> service = new();
        ContestParticipant first = VocalGroup.CreateNew();
        ContestParticipant second = SoloSinger.CreateNew();
        await service.CreateAsync(first);
        await service.CreateAsync(second);

        // Foreach безпосередньо (прямо) по сервісу (завдяки IEnumerable<T>).
        int visited = 0;
        foreach (ContestParticipant item in service)
        {
            Assert.NotNull(item);
            visited++;
        }

        // Обидва елементи побачені (як order, так і "зворотний").
        Assert.Equal(2, visited);
    }

    // ============================================================
    // THREAD SAFETY (поточна безпека)
    // ============================================================

    // Тест 13: thread-safe паралельне (parallel) створення.
    // 8 потоків × 500 = 4000 створень. Після ВСІХ паралельних
    // CreateAsync сервіс має містити ДОСЛЕДНЮ (попередню) кількість:
    // 4000 елементів.
    [Fact]
    public async Task ParallelCreate_IsThreadSafe()
    {
        const int threadCount = 8;
        const int itemsPerThread = 500;
        const int expectedTotal = threadCount * itemsPerThread; // = 4000

        CrudServiceAsync<Song> service = new();

        // 8 потоків одночасно виконують CreateAsync.
        // Task.Run запускає кожен блок в пулі потоків (thread pool).
        Task[] creators = new Task[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            creators[t] = Task.Run(async () =>
            {
                for (int i = 0; i < itemsPerThread; i++)
                {
                    // Кожен елемент має УНІКАЛЬНИЙ Id
                    // (Guid у конструкторі Song), тому duplicate (двійник)
                    // (подвійник) не можливий.
                    await service.CreateAsync(Song.CreateNew());
                }
            });
        }

        await Task.WhenAll(creators);

        // Реальна кількість: 4000 (не "почутим" 4000, а з колекції).
        List<Song> all = await service.ReadAllAsync();
        Assert.Equal(expectedTotal, all.Count);

        // Унікальність Id: усі 4000 — різні (ніхто не "потрапив"
        // у duplicate Id, і не було "потрачено" елементів).
        int uniqueIds = all.Select(x => x.Id).Distinct().Count();
        Assert.Equal(expectedTotal, uniqueIds);
    }

    // ============================================================
    // SAVEASYNC (збереження)
    // ============================================================

    private static string GetTempJsonFile()
    {
        // Унікальний тимчасовий (temp) файл: кожний тест
        // (кожен тест) працює зі своїм файлом, без сторонніх даних.
        string path = Path.Combine(
            Path.GetTempPath(),
            $"musiccontest_test_{Guid.NewGuid():N}.json");
        return path;
    }

    private static void DeleteTempFile(string path)
    {
        // Причищення (почищення) тимчасових файлів після тесту.
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Файлу не було — нічого чистити.
        }
    }

    // Тест 14: SaveAsync записує ВАЛІДНИЙ JSON із збереженими
    // (справжніми) даними об'єктів.
    [Fact]
    public async Task SaveAsync_WritesValidJson()
    {
        CrudServiceAsync<Song> service = new();
        Song song = Song.CreateNew();
        await service.CreateAsync(song);

        string filePath = GetTempJsonFile();
        try
        {
            await service.SaveAsync(filePath);

            // Файл знову "читаний" (потік знову вийшов з запису).
            Assert.True(File.Exists(filePath));

            // Читання JSON і перевірка: дані (data) збережені
            // (справді збережені, а не "надруковані").
            string json = await File.ReadAllTextAsync(filePath);
            List<Song>? deserialized =
                System.Text.Json.JsonSerializer.Deserialize<List<Song>>(json);

            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            // Id і назва збережені 1:1 (one-to-one).
            Assert.Equal(song.Id, deserialized![0].Id);
            Assert.Equal(song.Title, deserialized[0].Title);
        }
        finally
        {
            DeleteTempFile(filePath);
        }
    }

    // Тест 15: concurrent (конкурентний) SaveAsync.
    // Багато (потіків/викликів) паралельних SaveAsync на ОДИН
    // той самий файл. Фінальний (останній) JSON має бути ВАЛІДНИМ
    // і містити рівно ту саму кількість елементів, що в сервісі.
    // (Не "подивитись", а саме валідним (valid).)
    [Fact]
    public async Task ConcurrentSaveAsync_FinalFileIsValid()
    {
        const int elementCount = 50;
        const int concurrentSaveCount = 10;

        CrudServiceAsync<Song> service = new();
        for (int i = 0; i < elementCount; i++)
        {
            await service.CreateAsync(Song.CreateNew());
        }

        string filePath = GetTempJsonFile();
        try
        {
            // 10 SaveAsync одночасно на ОДИН файл.
            Task[] saves = new Task[concurrentSaveCount];
            for (int i = 0; i < concurrentSaveCount; i++)
            {
                saves[i] = service.SaveAsync(filePath);
            }

            // Чекання (await) завершення ВСІХ записів.
            await Task.WhenAll(saves);

            // Фінальний (останній) файл: JSON валідний
            // (valid) і з рівно elementCount (кількість елементів) записами.
            string json = await File.ReadAllTextAsync(filePath);
            List<Song>? deserialized =
                System.Text.Json.JsonSerializer.Deserialize<List<Song>>(json);

            Assert.NotNull(deserialized);
            // Елементів рівно (concurrent) elementCount, а не "двічі"
            // (не "doubled") записані (не "подвійно").
            Assert.Equal(elementCount, deserialized!.Count);
        }
        finally
        {
            DeleteTempFile(filePath);
        }
    }
}
