using System.Linq;
using System.Text;
using MusicContest.Common.Extensions;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

// UTF-8 потрібен, щоб українські символи коректно відображалися в консолі.
Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== MUSIC CONTEST CRUD DEMO ===");
Console.WriteLine();

// ЛАБОРАТОРНА РОБОТА №1 — демонстрація CRUD, подій,
// Save/Load (базовий варіант проєкту).
// Створюємо generic (універсальний) CRUD-сервіс.
// T тут дорівнює ContestParticipant, тому цей сервіс зберігає
// будь-яких нащадків ContestParticipant: і SoloSinger, і VocalGroup.
CrudService<ContestParticipant> participantService = new();

// Створюємо об'єкт SoloSinger.
SoloSinger singer = new(
    "Astra",       // stageName = сценічне ім'я
    "Україна",     // country = країна
    21,            // age = вік
    "сопрано",     // voiceType = тип голосу
    "A3-E6",       // vocalRange = вокальний діапазон
    8)             // yearsOfExperience = років досвіду
{
    // Для детермінованого результату задаємо постійний GUID.
    Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
};

// Другий тип нащадка — VocalGroup.
VocalGroup group = new(
    "Northern Lights",
    "Україна",
    24,
    4,             // memberCount = кількість учасників
    "pop / soul",  // genre = жанр
    true)          // hasBackingVocals = є бек-вокал
{
    Id = Guid.Parse("00000000-0000-0000-0000-000000000002")
};

// CREATE = додавання об'єктів до внутрішньої колекції CRUD-сервісу.
participantService.Create(singer);
participantService.Create(group);

Console.WriteLine("1. Після Create:");
PrintParticipants(participantService.ReadAll());

// READ = пошук одного об'єкта за унікальним Id.
Console.WriteLine();
Console.WriteLine("2. Read за Id:");
ContestParticipant found = participantService.Read(singer.Id);
Console.WriteLine(found.GetDescription());
Console.WriteLine($"Коротко: {found.ToShortInfo()}");

// UPDATE = оновлення.
Console.WriteLine();
Console.WriteLine("3. Після Update:");
singer.StageName = "Astra Nova";
participantService.Update(singer);
PrintParticipants(participantService.ReadAll());

// REMOVE = видалення об'єкта.
Console.WriteLine();
Console.WriteLine("4. Після Remove гурту:");
participantService.Remove(group);
PrintParticipants(participantService.ReadAll());

// Виклик статичного методу.
Console.WriteLine();
Console.WriteLine($"Створено об'єктів-нащадків ContestParticipant: {ContestParticipant.GetCreatedCount()}");

Song song = Song.FromMinutes("Gravity", "Sara Bareilles", 3.9, "F major");
Console.WriteLine($"Пісня: {song.Title}; тривалість: {song.GetFormattedDuration()}");

// Створюємо об'єкт виступу.
Performance performance = new(singer.StageName, song.Title, 86.5)
{
    Id = Guid.Parse("00000000-0000-0000-0000-000000000003")
};

// Підписка (subscribe) на подію (event).
performance.ScoreChanged += (item, oldScore, newScore) =>
{
    Console.WriteLine($"Подія ScoreChanged: {item.ParticipantStageName}: {oldScore:F1} -> {newScore:F1}");
};

Console.WriteLine();
Console.WriteLine("5. Делегат і подія:");
performance.SetScore(92.0);
performance.Complete();
Console.WriteLine($"Виступ завершено: {performance.IsCompleted}");

// SAVE / LOAD — додаткове завдання.
string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string filePath = Path.Combine(dataDirectory, "participants.json");

participantService.Save(filePath);

// Новий порожній сервіс, щоб показати, що дані
// відновлено з файлу, а не з пам'яті старого сервісу.
CrudService<ContestParticipant> loadedService = new();
loadedService.Load(filePath);

Console.WriteLine();
Console.WriteLine("6. Дані після Save та Load:");
PrintParticipants(loadedService.ReadAll());
Console.WriteLine($"JSON-файл: {filePath}");

// ============================================================
// ЛАБОРАТОРНА РОБОТА №2
// ============================================================
// Тема: Багатопотоковість. Асинхронність. IEnumerable. LINQ.
Console.WriteLine();
Console.WriteLine("=========================================");
Console.WriteLine("ЛАБОРАТОРНА РОБОТА №2");
Console.WriteLine("=========================================");

// ------------------------------------------------------------
// 1. PARALLEL CREATION (паралельне створення об'єктів)
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("1. PARALLEL CREATION (Parallel + Stopwatch):");

// Parallell.Run — створюємо 2000 об'єктів Song одночасно
// (Parallel.For) і вимірюємо ФАКТИЧНИЙ час через Stopwatch.
ParallelCreateResult parallelResult = Parallell.Run(Parallell.DefaultObjectCount, console: Console.Out);

// ------------------------------------------------------------
// 2. LINQ (Language Integrated Query) на СПРАВДЖНІХ даних
// ------------------------------------------------------------
// Демонстраційні дані — РЕАЛЬНО створена колекція (parallelResult.Songs),
// а не інша (не новий) набір об'єктів.
Console.WriteLine();
Console.WriteLine("2. LINQ (на створених Parallel-об'єктах):");

// Count (кількість) — скільки елементів в колекції.
int total = parallelResult.Songs.Count();
Console.WriteLine($"   Count:       {total}");

// Min (мінімум) — мінімальне значення тривалості.
int minDuration = parallelResult.Songs.Min(x => x.DurationSeconds);
Console.WriteLine($"   Min (DurationSeconds): {minDuration} s");

// Max (максимум) — максимальне значення тривалості.
int maxDuration = parallelResult.Songs.Max(x => x.DurationSeconds);
Console.WriteLine($"   Max (DurationSeconds): {maxDuration} s");

// Average (середнє) — середнє арифметичне значення тривалості.
double avgDuration = parallelResult.Songs.Average(x => x.DurationSeconds);
Console.WriteLine($"   Average (DurationSeconds): {avgDuration:F2} s");

// Where (фільтр) — пісні з тривалістю більшою (від >) за середню.
int longerThanAverage = parallelResult.Songs
    .Where(x => x.DurationSeconds > avgDuration)
    .Count();
Console.WriteLine($"   Where (Duration > Average): {longerThanAverage}");

// Select (вибір) — найдовша за тривалістю пісня:
// сортуємо за спаданням і беремо перший Title.
string? longestTitle = parallelResult.Songs
    .OrderByDescending(x => x.DurationSeconds)
    .Select(x => x.Title)
    .First();
Console.WriteLine($"   Select/OrderBy (найдовша пісня за Time): {longestTitle}");

// Aggregate (агрегація) — сумарна тривалість усіх пісень.
double sumOfDurations = parallelResult.Songs
    .Aggregate(0.0, (acc, x) => acc + x.DurationSeconds);
Console.WriteLine($"   Aggregate (сума тривалостей, с): {sumOfDurations:0}");

// OrderBy (сортування) — топ-3 найдовших пісень.
List<Song> topThree = parallelResult.Songs
    .OrderByDescending(x => x.DurationSeconds)
    .Take(3)
    .ToList();
Console.WriteLine($"   OrderBy/Top-3 (найдовші пісні):");
foreach (Song s in topThree)
{
    Console.WriteLine($"      - {s.Title}: {s.DurationSeconds} s, {s.Key}");
}

// ------------------------------------------------------------
// 3. ASYNC GENERIC CRUD + ПАГІНАЦІЯ (pagination — сторінковий
//    (сторінковий) доступ)
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("3. ASYNC GENERIC CRUD + ПАГІНАЦІЯ (ReadAllAsync(page, amount)):");

// CrudServiceAsync<Song> — асинхронний generic-сервіс
// для моделі Song (яка реалізує IEntity).
CrudServiceAsync<Song> asyncSongService = new();

// П'ять пісень з конкретними назвами
// (specific = конкретний).
Song[] presetSongs =
{
    new("Morning", "Artist A", 130, "C major"),
    new("Sunset",  "Artist B", 210, "F major"),
    new("Storm",   "Artist C", 250, "G major"),
    new("Calm",    "Artist D", 180, "A minor"),
    new("Night",   "Artist E", 300, "D minor")
};

foreach (Song s in presetSongs)
{
    await asyncSongService.CreateAsync(s);
}

int totalSongs = (await asyncSongService.ReadAllAsync()).Count;
Console.WriteLine($"   Елементів у сервісі: {totalSongs}");

// ReadAllAsync(page: 2, amount: 2) — друга сторінка, два елементи:
// пісні з індексів 2 і 3 ("Storm" і "Calm").
List<Song> pageTwo = await asyncSongService.ReadAllAsync(page: 2, amount: 2);
Console.WriteLine($"   ReadAllAsync(page: 2, amount: 2): {string.Join(", ", pageTwo.Select(s => s.Title))}");

// ReadAllAsync(page: 99, amount: 2) — Сторінка (page) за межами
// (out of range): порожня колекція.
List<Song> pageBeyond = await asyncSongService.ReadAllAsync(page: 99, amount: 2);
Console.WriteLine($"   ReadAllAsync(page: 99, amount: 2): елементів {pageBeyond.Count} (порожня сторінка)");

// ------------------------------------------------------------
// 4. ITERABLE (IEnumerable<T>): foreach безпосередньо по сервісу
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("4. IENUMERABLE: foreach безпосередньо по сервісу (snapshot):");

// foreach (var item in service) працює завдяки реалізації
// ICrudServiceAsync<T> : IEnumerable<T>. IEnumerator ітерує
// SNAPSHOT (миттєву копію), а не внутрішній змінюваний List<T>,
// тому одночасні зміни сервісу не ламають цикл.
int visited = 0;
foreach (Song item in asyncSongService)
{
    visited++;
    Console.WriteLine($"   - {item.Title} (відвідано: {visited})");
}
Console.WriteLine($"   Загальна кількість відвіданих: {visited}");

// ------------------------------------------------------------
// 5. SAVEASYNC (збереження в JSON)
// ------------------------------------------------------------
// АСІНХРОННЕ (async) generic-збереження в JSON,
// і демо конкурентного збереження:
// 3 SaveAsync одночасно, файл залишається валідним.
Console.WriteLine();
Console.WriteLine("5. SAVEASYNC (збереження в JSON):");

string lab2DataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string lab2FilePath = Path.Combine(lab2DataDirectory, "lab2_songs.json");

// Детермінований запуск: прибираємо файл попередньої
// демонстрації, щоб результат кожного запуску був однаковим.
if (File.Exists(lab2FilePath))
{
    File.Delete(lab2FilePath);
}

// Одна SaveAsync: сервіс -> JSON-файл.
await asyncSongService.SaveAsync(lab2FilePath);

// Три SaveAsync одночасно в ОДИЙ файл:
// SemaphoreSlim (рівень 1) гарантує, що одночасно
// пише файл лише один writer.
Task[] concurrentSaves =
{
    asyncSongService.SaveAsync(lab2FilePath),
    asyncSongService.SaveAsync(lab2FilePath),
    asyncSongService.SaveAsync(lab2FilePath)
};
await Task.WhenAll(concurrentSaves);

// Перевірка валідності JSON: файл десеріалізується без
// помилок і містить саме ті елементи, що є в сервісі.
string jsonAfterSaves = await File.ReadAllTextAsync(lab2FilePath);
List<Song>? savedSongs =
    System.Text.Json.JsonSerializer.Deserialize<List<Song>>(jsonAfterSaves);
Console.WriteLine($"   Конкурентне збереження (3 одночасні SaveAsync) завершено.");
Console.WriteLine($"   Валідний (valid) JSON: {savedSongs is not null}");
Console.WriteLine($"   Елементів у файлі після конкурентного Save: {(savedSongs ?? new()).Count}");
Console.WriteLine($"   JSON-файл: {lab2FilePath}");

// ------------------------------------------------------------
// 6. LOCK (взаємне виключення): 8 потоків × 10 000 increment
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("6. LOCK: 8 потоків × 10 000 increments (критична секція):");
await SynchronizationDemos.RunLockDemoAsync(
    threadCount: 8,
    incrementsPerThread: 10_000,
    console: Console.Out);

// ------------------------------------------------------------
// 7. SEMAPHORESLIM: 10 worker'ів, maxConcurrency = 3
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("7. SEMAPHORESLIM: 10 worker'ів, maxConcurrency = 3:");
await SynchronizationDemos.RunSemaphoreDemoAsync(
    workerCount: 10,
    maxConcurrency: 3,
    console: Console.Out);

// ------------------------------------------------------------
// 8. AUTORESETEVENT — справжня producer/consumer (M = 500)
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("8. AUTORESETEVENT (producer/consumer), M = 500:");
await SynchronizationDemos.RunProducerConsumerDemoAsync(
    itemCount: 500,
    console: Console.Out);

// Локальний статичний метод для багаторазового виведення учасників.
// IEnumerable<ContestParticipant> означає: метод приймає послідовність учасників.
static void PrintParticipants(IEnumerable<ContestParticipant> participants)
{
    foreach (ContestParticipant participant in participants)
    {
        Console.WriteLine($"- {participant.Id} | {participant.GetDescription()}");
    }
}
