using System.Text;
using MusicContest.Common.Extensions;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

// UTF-8 потрібен, щоб українські символи коректно відображалися в консолі.
Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== MUSIC CONTEST CRUD DEMO ===");
Console.WriteLine();

// Створюємо generic CRUD-сервіс.
// T тут дорівнює ContestParticipant, тому цей сервіс зберігає
// будь-яких нащадків ContestParticipant: і SoloSinger, і VocalGroup.
CrudService<ContestParticipant> participantService = new();

// Створюємо об'єкт SoloSinger.
// singer = співак / виконавець.
SoloSinger singer = new(
    "Astra",       // stageName = сценічне ім'я
    "Україна",     // country = країна
    21,            // age = вік
    "сопрано",     // voiceType = тип голосу
    "A3-E6",       // vocalRange = вокальний діапазон
    8)             // yearsOfExperience = років досвіду
{
    // У звичайній роботі Id вже автоматично створюється через Guid.NewGuid().
    // Тут ми навмисно задаємо постійний GUID, щоб демонстраційний результат
    // був однаковим при кожному запуску програми.
    // ...0001 умовно означає "перший демонстраційний об'єкт".
    Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
};

// Створюємо другий тип нащадка — VocalGroup.
// group = гурт.
VocalGroup group = new(
    "Northern Lights",
    "Україна",
    24,
    4,             // memberCount = кількість учасників
    "pop / soul",  // genre = жанр
    true)          // hasBackingVocals = є бек-вокал
{
    // Постійний демонстраційний GUID другого об'єкта.
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

// Використання методу розширення.
// found має тип ContestParticipant, але завдяки extension method
// ми можемо викликати ToShortInfo() так, ніби цей метод був у самому класі.
Console.WriteLine($"Коротко: {found.ToShortInfo()}");

// UPDATE = оновлення.
// Спочатку змінюємо властивість об'єкта, потім передаємо його сервісу.
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
// Для static не потрібен конкретний об'єкт singer або group,
// метод викликається через назву класу ContestParticipant.
Console.WriteLine();
Console.WriteLine($"Створено об'єктів-нащадків ContestParticipant: {ContestParticipant.GetCreatedCount()}");

// Статичний фабричний метод FromMinutes створює Song,
// приймаючи тривалість у хвилинах.
Song song = Song.FromMinutes("Gravity", "Sara Bareilles", 3.9, "F major");
Console.WriteLine($"Пісня: {song.Title}; тривалість: {song.GetFormattedDuration()}");

// Створюємо об'єкт виступу.
Performance performance = new(singer.StageName, song.Title, 86.5)
{
    // Третій постійний GUID — лише для наочності демонстрації.
    Id = Guid.Parse("00000000-0000-0000-0000-000000000003")
};

// ПІДПИСКА НА ПОДІЮ.
// += означає "додати обробник події".
// (item, oldScore, newScore) => { ... } — лямбда-вираз,
// тобто короткий запис анонімного методу.
performance.ScoreChanged += (item, oldScore, newScore) =>
{
    Console.WriteLine($"Подія ScoreChanged: {item.ParticipantStageName}: {oldScore:F1} -> {newScore:F1}");
};

Console.WriteLine();
Console.WriteLine("5. Делегат і подія:");

// SetScore змінює оцінку та викликає ScoreChanged.
performance.SetScore(92.0);

// Complete змінює IsCompleted на true.
performance.Complete();
Console.WriteLine($"Виступ завершено: {performance.IsCompleted}");

// SAVE / LOAD — додаткове завдання.
// AppContext.BaseDirectory = папка, звідки запущена програма.
// Path.Combine безпечно формує шлях незалежно від операційної системи.
string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string filePath = Path.Combine(dataDirectory, "participants.json");

// Save: серіалізація колекції та запис у JSON-файл.
participantService.Save(filePath);

// Створюємо новий порожній сервіс, щоб показати,
// що дані справді можна відновити не з пам'яті старого сервісу, а з файлу.
CrudService<ContestParticipant> loadedService = new();

// Load: читання JSON і десеріалізація назад у C#-об'єкти.
loadedService.Load(filePath);

Console.WriteLine();
Console.WriteLine("6. Дані після Save та Load:");
PrintParticipants(loadedService.ReadAll());
Console.WriteLine($"JSON-файл: {filePath}");

// Локальний статичний метод для багаторазового виведення учасників.
// IEnumerable<ContestParticipant> означає: метод приймає послідовність учасників.
static void PrintParticipants(IEnumerable<ContestParticipant> participants)
{
    // foreach перебирає кожен елемент колекції по черзі.
    foreach (ContestParticipant participant in participants)
    {
        Console.WriteLine($"- {participant.Id} | {participant.GetDescription()}");
    }
}
