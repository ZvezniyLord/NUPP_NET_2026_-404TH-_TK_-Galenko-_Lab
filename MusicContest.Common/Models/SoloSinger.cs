namespace MusicContest.Common.Models;

public class SoloSinger : ContestParticipant
{
    public string VoiceType { get; set; } = string.Empty;
    public string VocalRange { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    // Конструктор
    public SoloSinger()
    {
    }

    // Конструктор з параметрами
    public SoloSinger(
        string stageName,
        string country,
        int age,
        string voiceType,
        string vocalRange,
        int yearsOfExperience)
        : base(stageName, country, age)
    {
        VoiceType = voiceType;
        VocalRange = vocalRange;
        YearsOfExperience = yearsOfExperience;
    }

    // Метод із перевизначенням
    public override string GetDescription()
    {
        return $"Сольний виконавець: {base.GetDescription()}, голос: {VoiceType}, діапазон: {VocalRange}";
    }

    // Метод
    public bool HasEnoughExperience(int minimumYears)
    {
        return YearsOfExperience >= minimumYears;
    }
}
