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
    // Random.Shared — спільна thread-safe копія Random:
    // метод можна викликати з безлічі потоків одночасно.
    public static VocalGroup CreateNew()
    {
        // Random.Shared безпечний для потоку: різні потоки
        // одночасно отримують незалежні рандомні значення.
        Random random = Random.Shared;

        return new VocalGroup(
            $"Group-{Guid.NewGuid():N}",         // унікальне сценічне ім'я
            $"Country-{random.Next(1, 31)}",     // country: країна
            random.Next(18, 35),                 // age: вік
            random.Next(2, 12),                  // memberCount: кількість учасників
            $"Genre-{random.Next(1, 10)}",       // genre: жанр
            random.Next(1) > 0);                  // hasBackingVocals: є бек-вокал (true/false)
    }
}
