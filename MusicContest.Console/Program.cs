using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicContest.Common;
using MusicContest.Common.Extensions;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

// UTF-8 потрібен, щоб українські символи коректно відображалися в консолі.
Console.OutputEncoding = Encoding.UTF8;

// async Main: якщо в топ-рівних статментах є await,
// компілятор сам генерує async Task Main().
// async = метод може використовувати await і повертає Task/Task<T>.
// await = асинхронне очікування завершення Task без блокування потоку.
//
// RunLab1 — синхронна (synchronous) демонстрація ЛР1. Вона не має I/O-очікувань
// у Task, тому викликається без await (просто RunLab1()).
// RunLab2 — async (містить await через SaveAsync), тому очікуємо через await.
RunLab1();
await RunLab2();

static void RunLab1()
{
    Console.WriteLine("=== MUSIC CONTEST CRUD DEMO (Lab 1) ===");
    Console.WriteLine();

    // Створюємо generic CRUD-сервіс (синхронну версію з ЛР1).
    // T = ContestParticipant: сервіс зберігає нащадків: SoloSinger і VocalGroup.
    CrudService<ContestParticipant> participantService = new();

    SoloSinger singer = new("Astra", "Україна", 21, "сопрано", "A3-E6", 8)
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
    };

    VocalGroup group = new("Northern Lights", "Україна", 24, 4, "pop / soul", true)
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000002")
    };

    participantService.Create(singer);
    participantService.Create(group);

    Console.WriteLine("1. Після Create:");
    PrintParticipants(participantService.ReadAll());

    Console.WriteLine();
    Console.WriteLine("2. Read за Id:");
    ContestParticipant found = participantService.Read(singer.Id);
    Console.WriteLine(found.GetDescription());
    Console.WriteLine($"Коротко: {found.ToShortInfo()}");

    Console.WriteLine();
    Console.WriteLine("3. Після Update:");
    singer.StageName = "Astra Nova";
    participantService.Update(singer);
    PrintParticipants(participantService.ReadAll());

    Console.WriteLine();
    Console.WriteLine("4. Після Remove гурту:");
    participantService.Remove(group);
    PrintParticipants(participantService.ReadAll());

    Console.WriteLine();
    Console.WriteLine($"Створено об'єктів-нащадків ContestParticipant: {ContestParticipant.GetCreatedCount()}");

    Song song = Song.FromMinutes("Gravity", "Sara Bareilles", 3.9, "F major");
    Console.WriteLine($"Пісня: {song.Title}; тривалість: {song.GetFormattedDuration()}");

    Performance performance = new(singer.StageName, song.Title, 86.5)
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000003")
    };

    performance.ScoreChanged += (item, oldScore, newScore) =>
    {
        Console.WriteLine($"Подія ScoreChanged: {item.ParticipantStageName}: {oldScore:F1} -> {newScore:F1}");
    };

    Console.WriteLine();
    Console.WriteLine("5. Делегат і подія:");
    performance.SetScore(92.0);
    performance.Complete();
    Console.WriteLine($"Виступ завершено: {performance.IsCompleted}");

    string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
    string filePath = Path.Combine(dataDirectory, "participants.json");
    participantService.Save(filePath);

    CrudService<ContestParticipant> loadedService = new();
    loadedService.Load(filePath);

    Console.WriteLine();
    Console.WriteLine("6. Дані після Save та Load:");
    PrintParticipants(loadedService.ReadAll());
    Console.WriteLine($"JSON-файл: {filePath}");
}

static async Task RunLab2()
{
    Console.WriteLine();
    Console.WriteLine("==================================================");
    Console.WriteLine("=== ЛАБОРАТОРНА РОБОТА №2 ===");
    Console.WriteLine("Багатопотоковість. Асинхронність. IEnumerable. LINQ.");
    Console.WriteLine("==================================================");

    // Шлях до файлу для async CRUD.
    // AppContext.BaseDirectory = папка, звідки запущена програма.
    // Data = підпапка; async.json = ім'я файлу.
    string asyncDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "async-participants.json");

    // Створюємо async CRUD-сервіс.
    // ICrudServiceAsync<T> : IEnumerable<T> — сервіс сам послідовність.
    // where T : IEntity — T має Id.
    ICrudServiceAsync<SoloSinger> service = new CrudServiceAsync<SoloSinger>(asyncDataPath);

    // -------------------------------------------------------------
    // 1. Паралельне створення об'єктів через Parallell.CreateInParallel
    // -------------------------------------------------------------
    // Func<SoloSinger> = функція, що повертає SoloSinger.
    // SoloSinger.CreateNew — статичний метод, підходить як Func<SoloSinger>.
    //
    // count = кількість.
    // Створюємо 2000 об'єктів — багатопотоковість буде очевидною.
    int count = 2000;
    Parallell.CreateInParallel(service, count, SoloSinger.CreateNew);

    // ReadAllAsync() = отримати snapshot всієї колекції.
    // snapshot = знімок / копія стану колекції у конкретний момент.
    IEnumerable<SoloSinger> singers = await service.ReadAllAsync();

    // -------------------------------------------------------------
    // 2. LINQ статистика
    // -------------------------------------------------------------
    // LINQ = Language Integrated Query = інтегровані у мову запити.
    //
    // Count = кількість;
    // Min = мінімум;
    // Max = максимум;
    // Average = середнє;
    // Where = фільтрація;
    // Select = проєкція / вибір потрібного значення;
    // OrderBy = сортування;
    // Aggregate = накопичувальна операція.
    //
    // Лямбда: x => x.Age означає "для кожного елемента x взяти Age".

    int ageCount = singers.Count();
    int minAge = singers.Min(s => s.Age);
    int maxAge = singers.Max(s => s.Age);
    double avgAge = singers.Average(s => s.Age);

    int expMin = singers.Min(s => s.YearsOfExperience);
    int expMax = singers.Max(s => s.YearsOfExperience);
    double expAvg = singers.Average(s => s.YearsOfExperience);

    Console.WriteLine();
    Console.WriteLine($"2. LINQ статистика (всього {ageCount} виконавців):");
    Console.WriteLine($"   Вік (Age): min={minAge}, max={maxAge}, avg={avgAge:F2}");
    Console.WriteLine($"   Досвід (YearsOfExperience): min={expMin}, max={expMax}, avg={expAvg:F2}");

    // -------------------------------------------------------------
    // 3. Select / Where / Aggregate / OrderBy
    // -------------------------------------------------------------
    Console.WriteLine();
    Console.WriteLine("3. Select / Where / Aggregate / OrderBy:");

    // Where = фільтрація.
    // Виконавці з досвідом >= 10 років.
    var experienced = singers.Where(s => s.YearsOfExperience >= 10).ToList();
    Console.WriteLine($"   Where: виконавці з досвідом >= 10 років: {experienced.Count()}");

    // Select = проєкція / вибір потрібного значення.
    // Видобрати StageName (тільки ім'я, без решти об'єкта).
    var names = singers.Select(s => s.StageName).ToList();
    Console.WriteLine($"   Select: вибрано {names.Count()} StageName, перше = \"{names.First()}\"");

    // Aggregate = накопичувальна операція.
    // Сумарна кількість років досвіду (sum).
    int totalExp = singers.Aggregate(0, (sum, s) => sum + s.YearsOfExperience);
    Console.WriteLine($"   Aggregate: сумарний досвід (sum) = {totalExp} років");

    // OrderBy = сортування.
    // Sort by Age (ascending).
    var byAge = singers.OrderBy(s => s.Age).ToList();
    var top3 = byAge.Take(3).ToList();
    Console.WriteLine("   OrderBy(Age) — перші 3 (наймолодші):");
    foreach (var s in top3)
    {
        Console.WriteLine($"      - {s.StageName}: {s.Age} років");
    }

    // -------------------------------------------------------------
    // 4. Пагінація
    // -------------------------------------------------------------
    // page = номер сторінки (від 1);
    // amount = кількість елементів на сторінці.
    //
    // Skip = пропустити N елементів;
    // Take = взяти N елементів.
    //
    // page = 1, amount = 10 -> Skip(0).Take(10) -> елементи 1-10.
    // page = 2, amount = 10 -> Skip(10).Take(10) -> елементи 11-20.
    Console.WriteLine();
    Console.WriteLine("4. Пагінація (ReadAllAsync(page, amount)):");

    var page1 = await service.ReadAllAsync(1, 10);
    Console.WriteLine("   Сторінка 1 (10 елем.):\n");
    foreach (var s in page1)
    {
        Console.WriteLine($"      - {s.StageName}, {s.Age} років, досвід {s.YearsOfExperience}");
    }

    var page2 = await service.ReadAllAsync(2, 10);
    Console.WriteLine("   Сторінка 2 (10 елем.):\n");
    foreach (var s in page2)
    {
        Console.WriteLine($"      - {s.StageName}, {s.Age} років, досвід {s.YearsOfExperience}");
    }

    // -------------------------------------------------------------
    // 5. IEnumerable / foreach
    // -------------------------------------------------------------
    // IEnumerable<T> — сервіс є послідовністю:
    // його можна перебирати через foreach.
    //
    // foreach (T item in service) — перебирає snapshot колекції.
    Console.WriteLine();
    Console.WriteLine("5. IEnumerable / foreach:");
    int foreachCount = 0;
    foreach (SoloSinger s in service)
    {
        foreachCount++;
    }
    Console.WriteLine($"   foreach по сервісу: {foreachCount} об'єктів");
    Console.WriteLine($"   Перший об'єкт: {(await service.ReadAsync(page1.First().Id)).GetDescription()}");

    // -------------------------------------------------------------
    // 6. Асинхронне збереження (SaveAsync)
    // -------------------------------------------------------------
    Console.WriteLine();
    Console.WriteLine("6. Асинхронне збереження:");
    bool saved = await service.SaveAsync();
    Console.WriteLine($"   Файл: {asyncDataPath}");
    Console.WriteLine($"   Збережено: {saved}");

    // -------------------------------------------------------------
    // 7. lock
    // -------------------------------------------------------------
    Console.WriteLine("7. lock demo:");
    int lockResult = Parallell.LockDemo();

    // -------------------------------------------------------------
    // 8. SemaphoreSlim
    // -------------------------------------------------------------
    Console.WriteLine("8. SemaphoreSlim demo:");
    Parallell.SemaphoreDemo();

    // -------------------------------------------------------------
    // 9. AutoResetEvent
    // -------------------------------------------------------------
    Console.WriteLine("9. AutoResetEvent demo:");
    Parallell.AutoResetEventDemo();

    Console.WriteLine();
    Console.WriteLine("=== ЛАБОРАТОРНА РОБОТА №2 ЗАВЕРШЕНО ===");
}

// Локальний статичний метод для виведення учасників.
// IEnumerable<T> означає: метод приймає послідовність об'єктів.
static void PrintParticipants(IEnumerable<ContestParticipant> participants)
{
    // foreach перебирає кожен елемент колекції по черзі.
    foreach (ContestParticipant participant in participants)
    {
        Console.WriteLine($"- {participant.Id} | {participant.GetDescription()}");
    }
}
