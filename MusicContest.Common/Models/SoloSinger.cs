using MusicContest.Common.Data;

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
    // Цей конструктор також використовують під час десеріалізації об'єкта з JSON.
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
        // Базовий конструктор ініціалізує спільні властивості. Нижче задаються поля похідного типу.
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

    // СТАТИЧНИЙ ФАБРИЧНИЙ МЕТОД CreateNew("створити новий").
    // Фабричний метод повертає готовий об'єкт.
    // Через Random.Shared (спільна thread-safe копія Random)
    // Random.Shared дає змогу безпечно створювати об'єкти з різними значеннями в паралельних
    // потоках.
    public static SoloSinger CreateNew()
    {
        var seeds = UkrainianMusicData.Singers;
        var selected = seeds[Random.Shared.Next(seeds.Length)];
        return new SoloSinger(selected.StageName, "Україна", selected.Age,
            selected.VoiceType, selected.VocalRange, selected.YearsOfExperience);
    }
}
