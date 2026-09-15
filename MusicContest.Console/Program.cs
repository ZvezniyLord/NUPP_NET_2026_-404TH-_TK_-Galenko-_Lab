using System.Text;
using MusicContest.Common.Extensions;
using MusicContest.Common.Models;
using MusicContest.Common.Services;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== MUSIC CONTEST CRUD DEMO ===");
Console.WriteLine();

CrudService<ContestParticipant> participantService = new();

SoloSinger singer = new(
    "Astra",
    "Україна",
    21,
    "сопрано",
    "A3-E6",
    8)
{
    Id = Guid.Parse("11111111-1111-1111-1111-111111111111")
};

VocalGroup group = new(
    "Northern Lights",
    "Україна",
    24,
    4,
    "pop / soul",
    true)
{
    Id = Guid.Parse("22222222-2222-2222-2222-222222222222")
};

// CREATE
participantService.Create(singer);
participantService.Create(group);

Console.WriteLine("1. Після Create:");
PrintParticipants(participantService.ReadAll());

// READ
Console.WriteLine();
Console.WriteLine("2. Read за Id:");
ContestParticipant found = participantService.Read(singer.Id);
Console.WriteLine(found.GetDescription());

// Метод розширення
Console.WriteLine($"Коротко: {found.ToShortInfo()}");

// UPDATE
Console.WriteLine();
Console.WriteLine("3. Після Update:");
singer.StageName = "Astra Nova";
participantService.Update(singer);
PrintParticipants(participantService.ReadAll());

// REMOVE
Console.WriteLine();
Console.WriteLine("4. Після Remove гурту:");
participantService.Remove(group);
PrintParticipants(participantService.ReadAll());

// Статичний метод
Console.WriteLine();
Console.WriteLine($"Створено об'єктів-нащадків ContestParticipant: {ContestParticipant.GetCreatedCount()}");

Song song = Song.FromMinutes("Gravity", "Sara Bareilles", 3.9, "F major");
Console.WriteLine($"Пісня: {song.Title}; тривалість: {song.GetFormattedDuration()}");

Performance performance = new(singer.StageName, song.Title, 86.5)
{
    Id = Guid.Parse("33333333-3333-3333-3333-333333333333")
};

// Підписка на подію
performance.ScoreChanged += (item, oldScore, newScore) =>
{
    Console.WriteLine($"Подія ScoreChanged: {item.ParticipantStageName}: {oldScore:F1} -> {newScore:F1}");
};

Console.WriteLine();
Console.WriteLine("5. Делегат і подія:");
performance.SetScore(92.0);
performance.Complete();
Console.WriteLine($"Виступ завершено: {performance.IsCompleted}");

// SAVE / LOAD
string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
string filePath = Path.Combine(dataDirectory, "participants.json");
participantService.Save(filePath);

CrudService<ContestParticipant> loadedService = new();
loadedService.Load(filePath);

Console.WriteLine();
Console.WriteLine("6. Дані після Save та Load:");
PrintParticipants(loadedService.ReadAll());
Console.WriteLine($"JSON-файл: {filePath}");

static void PrintParticipants(IEnumerable<ContestParticipant> participants)
{
    foreach (ContestParticipant participant in participants)
    {
        Console.WriteLine($"- {participant.Id} | {participant.GetDescription()}");
    }
}
