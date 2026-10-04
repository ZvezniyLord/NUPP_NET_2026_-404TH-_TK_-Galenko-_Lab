namespace MusicContest.Common.Models;

// SoloSinger = сольний співак / сольний виконавець.
// ":" означає наслідування: SoloSinger отримує властивості й методи ContestParticipant.
public class SoloSinger : ContestParticipant
{
    // VoiceType = тип голосу;
    // VocalRange = вокальний діапазон;
    // YearsOfExperience = кількість років досвіду.
    public string VoiceType { get; set; } = string.Empty;
    public string VocalRange { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    // Конструктор без параметрів.
    // Він також зручний для десеріалізації — відновлення об'єкта з JSON.
    public SoloSinger()
    {
    }

    // Конструктор з параметрами дозволяє одразу передати всі основні дані.
    public SoloSinger(
        string stageName,
        string country,
        int age,
        string voiceType,
        string vocalRange,
        int yearsOfExperience)
        // base(...) викликає конструктор базового класу ContestParticipant.
        // Спільні дані ініціалізуються там, а специфічні — нижче.
        : base(stageName, country, age)
    {
        VoiceType = voiceType;
        VocalRange = vocalRange;
        YearsOfExperience = yearsOfExperience;
    }

    // override = перевизначення успадкованого virtual-методу.
    // base.GetDescription() викликає базову реалізацію, після чого додаємо
    // інформацію, характерну саме для сольного виконавця.
    public override string GetDescription()
    {
        return $"Сольний виконавець: {base.GetDescription()}, голос: {VoiceType}, діапазон: {VocalRange}";
    }

    // Метод перевіряє, чи достатньо у виконавця досвіду.
    // minimumYears = мінімальна потрібна кількість років.
    public bool HasEnoughExperience(int minimumYears)
    {
        return YearsOfExperience >= minimumYears;
    }

    // Статичний фабрічний метод CreateNew = "створити новий" із згенерованими даними.
    //
    // Random = генератор псевдовипадкових чисел.
    // Random.Shared = спільний (shared) екземпляр Random у сучасному .NET,
    // який потокобезпечний (thread-safe) і зручний для паралельного
    // створення об'єктів у різних потоках: не треба створювати
    // власний Random для кожного потоку (це класична помилка).
    //
    // Рандомні логічні значення:
    //   StageName  = сценічне ім'я (з випадкового списку);
    //   Country    = країна;
    //   Age        = вік: 18-60 років;
    //   VoiceType  = тип голосу;
    //   VocalRange = діапазон;
    //   YearsOfExperience = років досвіду: 0-35.
    public static SoloSinger CreateNew()
    {
        // rng.Next(max) повертає 0..max-1; rng.Next(min, max) повертає min..max-1.
        // Тобто age = 18 + Next(43) -> 18..60, experience = Next(36) -> 0..35.
        Random rng = Random.Shared;

        string[] names = { "Astra", "Nordwind", "Sirena", "Borealis", "Eklipsa",
            "Halcyon", "Lyra", "Melodia", "Vestra", "Zorya" };
        string[] countries = { "Україна", "Poland", "Czechia", "Slovakia", "Romania",
            "Moldova", "Bulgaria", "Latvia", "Lithuania", "Estonia" };
        string[] voiceTypes = { "сопрано", "мецо", "тенор", "бас" };
        string[] ranges = { "C4-A5", "D4-B5", "A3-E6", "C3-G5", "F3-C6", "B3-G5" };

        int age = 18 + rng.Next(43);                          // 18..60
        int experience = rng.Next(36);                        // 0..35

        return new SoloSinger(
            names[rng.Next(names.Length)],
            countries[rng.Next(countries.Length)],
            age,
            voiceTypes[rng.Next(voiceTypes.Length)],
            ranges[rng.Next(ranges.Length)],
            experience);
    }
}
