using MusicContest.Common.Data;

namespace MusicContest.Common.Models;

// VocalGroup = вокальний гурт.
// Це другий клас-нащадок ContestParticipant.
public class VocalGroup : ContestParticipant
{
    // MemberCount = кількість учасників;
    // Genre = жанр;
    // HasBackingVocals = чи є бек-вокал.
    public int MemberCount { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool HasBackingVocals { get; set; }

    // Конструктор без параметрів.
    public VocalGroup()
    {
    }

    // Конструктор з параметрами.
    public VocalGroup(
        string stageName,
        string country,
        int age,
        int memberCount,
        string genre,
        bool hasBackingVocals)
        // base(...) передає спільні дані конструктору базового класу.
        : base(stageName, country, age)
    {
        MemberCount = memberCount;
        Genre = genre;
        HasBackingVocals = hasBackingVocals;
    }

    // Перевизначення GetDescription().
    // Тут проявляється поліморфізм: змінна може мати тип ContestParticipant,
    // але виконається метод фактичного об'єкта — SoloSinger або VocalGroup.
    public override string GetDescription()
    {
        return $"Вокальний гурт: {base.GetDescription()}, учасників: {MemberCount}, жанр: {Genre}";
    }

    // Метод повертає true, якщо у гурті 5 або більше учасників.
    public bool IsLargeGroup()
    {
        return MemberCount >= 5;
    }

    // СТАТИЧНИЙ ФАБРИЧНИЙ МЕТОД CreateNew("створити новий").
    // Фабрика — метод, який повертає готовий об'єкт.
    // Random.Shared є спільним потокобезпечним генератором випадкових чисел.
    // Його можна викликати з кількох потоків одночасно.
    public static VocalGroup CreateNew()
    {
        var seeds = UkrainianMusicData.Groups;
        var selected = seeds[Random.Shared.Next(seeds.Length)];
        return new VocalGroup(selected.StageName, "Україна", selected.Age,
            selected.MemberCount, selected.Genre, selected.HasBackingVocals);
    }
}
