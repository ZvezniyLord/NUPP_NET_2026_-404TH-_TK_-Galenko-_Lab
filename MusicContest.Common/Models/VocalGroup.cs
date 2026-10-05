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

    // Статичний фабрічний метод CreateNew = "створити новий" із згенерованими даними.
    //
    // Random = генератор псевдовипадкових чисел.
    // Random.Shared = спільний (shared) екземпляр Random у сучасному .NET,
    // який потокобезпечний (thread-safe) і зручний для паралельного створення
    // об'єктів у різних потоках: не треба створювати власний Random для
    // кожного потоку (це класична помилка).
    //
    // Рандомні логічні значення:
    //   StageName  = сценічне ім'я (з випадкового списку);
    //   Country    = країна;
    //   MemberCount = кількість учасників: 2-12;
    //   Genre      = жанр: випадково з масиву;
    //   HasBackingVocals = є бек-вокал: true/false.
    public static VocalGroup CreateNew()
    {
        Random rng = Random.Shared;

        string[] names = { "Northern Lights", "Pulsar", "Delta", "Aurora", "Vektor",
            "Sonata", "Ekh", "Lyra", "Zora", "Nova" };
        string[] countries = { "Україна", "Poland", "Czechia", "Slovakia", "Romania",
            "Moldova", "Bulgaria", "Latvia", "Lithuania", "Estonia" };
        string[] genres = { "pop", "rock", "folk", "jazz", "electronic", "chamber",
            "classical", "hip-hop", "metal", "indie" };

        int memberCount = 2 + rng.Next(11);        // 2..12
        int age = 18 + rng.Next(43);               // 18..60

        return new VocalGroup(
            names[rng.Next(names.Length)],
            countries[rng.Next(countries.Length)],
            age,
            memberCount,
            genres[rng.Next(genres.Length)],
            rng.Next(2) == 0);
    }
}
