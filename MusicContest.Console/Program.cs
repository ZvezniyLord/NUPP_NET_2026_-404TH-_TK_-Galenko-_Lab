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

// SAVE / LOAD — додаткове завдання ЛР1.
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
// 1. ASYNC CRUD + PARALLEL CREATION: паралельно створюємо від
//    тисячі об'єктів (тут 2000) через Song.CreateNew() і
//    CrudServiceAsync<Song>.CreateAsync (клас Parallell).
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("1. PARALLEL CREATION через CRUD service + CreateNew (Parallel.ForEachAsync):");

string lab2DataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string lab2FilePath = Path.Combine(lab2DataDirectory, "lab2_songs.json");

// Детермінований запуск: прибираємо файл попереднього запуску.
if (File.Exists(lab2FilePath))
{
    File.Delete(lab2FilePath);
}

// Сервіс одразу знає свій FilePath — саме туди збереже колекцію (SaveAsync()).
CrudServiceAsync<Song> songService = new(lab2FilePath);

ParallelCreateResult parallelResult = await Parallell.RunAsync(
    songService,
    Parallell.DefaultObjectCount,
    console: Console.Out);

// ------------------------------------------------------------
// 2. LINQ (Language Integrated Query) на створеній колекції:
//    min / max / average цифрової властивості DurationSeconds.
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("2. LINQ (min/max/average на створених об'єктах):");

// Зчитуємо колекцію з самого сервісу (snapshot), а не з окремого списку.
List<Song> songs = (await songService.ReadAllAsync()).ToList();

// Count (кількість) — скільки елементів у сервісі.
int total = songs.Count();
Console.WriteLine($"   Count:       {total}");

// Min (мінімум) — мінімальна тривалість пісні.
int minDuration = songs.Min(x => x.DurationSeconds);
Console.WriteLine($"   Min (DurationSeconds): {minDuration} s");

// Max (максимум) — максимальна тривалість пісні.
int maxDuration = songs.Max(x => x.DurationSeconds);
Console.WriteLine($"   Max (DurationSeconds): {maxDuration} s");

// Average (середнє) — середня тривалість пісні.
double avgDuration = songs.Average(x => x.DurationSeconds);
Console.WriteLine($"   Average (DurationSeconds): {avgDuration:F2} s");

// Where (фільтр) — пісні довші за середню тривалість.
int longerThanAverage = songs.Where(x => x.DurationSeconds > avgDuration).Count();
Console.WriteLine($"   Where (Duration > Average): {longerThanAverage}");

// Select + OrderByDescending — найдовша пісня: сортуємо за спаданням,
// беремо перший Title.
string? longestTitle = songs
    .OrderByDescending(x => x.DurationSeconds)
    .Select(x => x.Title)
    .First();
Console.WriteLine($"   Select/OrderBy (найдовша пісня): {longestTitle}");

// Aggregate (агрегація) — сумарна тривалість усіх пісень.
double sumOfDurations = songs.Aggregate(0.0, (acc, x) => acc + x.DurationSeconds);
Console.WriteLine($"   Aggregate (сума тривалостей, с): {sumOfDurations:0}");

// OrderBy/Top-3 — три найдовші пісні.
List<Song> topThree = songs
    .OrderByDescending(x => x.DurationSeconds)
    .Take(3)
    .ToList();
Console.WriteLine("   OrderBy/Top-3 (найдовші пісні):");
foreach (Song s in topThree)
{
    Console.WriteLine($"      - {s.Title}: {s.DurationSeconds} s, {s.Key}");
}

// ------------------------------------------------------------
// 3. ПАГІНАЦІЯ (pagination): ReadAllAsync(page, amount) на тій самій
//    великій колекції (2000 елементів). Сторінки по 5 елементів.
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("3. ПАГІНАЦІЯ (ReadAllAsync(page, amount)):");

int pageSize = 5;

IEnumerable<Song> pageOne = await songService.ReadAllAsync(page: 1, amount: pageSize);
Console.WriteLine($"   Page 1 ({pageSize}): {string.Join(", ", pageOne.Select(s => s.Title))}");

IEnumerable<Song> pageTwo = await songService.ReadAllAsync(page: 2, amount: pageSize);
Console.WriteLine($"   Page 2 ({pageSize}): {string.Join(", ", pageTwo.Select(s => s.Title))}");

// Сторінка за межами колекції — порожня колекція (не помилка).
IEnumerable<Song> pageBeyond = await songService.ReadAllAsync(page: 999, amount: pageSize);
Console.WriteLine($"   Page 999 ({pageSize}): елементів {pageBeyond.Count()} (порожня сторінка)");

// ------------------------------------------------------------
// 4. IENUMERABLE<T>: foreach безпосередньо по сервісу (snapshot).
//    Enumerator ітерує миттєву копію, а не внутрішню колекцію, тому
//    одночасні зміни сервісу не ламають цикл.
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("4. IENUMERABLE: foreach безпосередньо по сервісу (перші 5 + сума):");

int visited = 0;
foreach (Song item in songService)
{
    if (visited < 5)
    {
        Console.WriteLine($"   - {item.Title} ({item.DurationSeconds} s)");
    }
    visited++;
}
Console.WriteLine($"   Загальна кількість відвіданих: {visited}");

// ------------------------------------------------------------
// 5. SAVEASYNC: збереження згенерованої колекції у файл + демо
//    конкурентного збереження (3 одночасні SaveAsync, файл валідний).
// ------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("5. SAVEASYNC (збереження колекції у JSON за FilePath):");

await songService.SaveAsync();

Task[] concurrentSaves =
{
    songService.SaveAsync(),
    songService.SaveAsync(),
    songService.SaveAsync()
};
await Task.WhenAll(concurrentSaves);

// Перевірка валідності JSON: файл десеріалізується без помилок і
// містить саме ті елементи, що є в сервісі.
string jsonAfterSaves = await File.ReadAllTextAsync(lab2FilePath);
List<Song>? savedSongs = System.Text.Json.JsonSerializer.Deserialize<List<Song>>(jsonAfterSaves);
Console.WriteLine($"   Конкурентне збереження (3 одночасні SaveAsync) завершено.");
Console.WriteLine($"   Валідний (valid) JSON: {savedSongs is not null}");
Console.WriteLine($"   Елементів у файлі після конкурентного Save: {(savedSongs ?? new()).Count}");
Console.WriteLine($"   Файл: {lab2FilePath}");

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
static void PrintParticipants(IEnumerable<ContestParticipant> participants)
{
    foreach (ContestParticipant participant in participants)
    {
        Console.WriteLine($"- {participant.Id} | {participant.GetDescription()}");
    }
}
