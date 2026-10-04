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
}
