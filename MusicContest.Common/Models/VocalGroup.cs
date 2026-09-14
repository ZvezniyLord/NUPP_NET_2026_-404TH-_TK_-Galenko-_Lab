namespace MusicContest.Common.Models;

public class VocalGroup : ContestParticipant
{
    public int MemberCount { get; set; }
    public string Genre { get; set; } = string.Empty;
    public bool HasBackingVocals { get; set; }

    // Конструктор
    public VocalGroup()
    {
    }

    // Конструктор з параметрами
    public VocalGroup(
        string stageName,
        string country,
        int age,
        int memberCount,
        string genre,
        bool hasBackingVocals)
        : base(stageName, country, age)
    {
        MemberCount = memberCount;
        Genre = genre;
        HasBackingVocals = hasBackingVocals;
    }

    // Метод із перевизначенням
    public override string GetDescription()
    {
        return $"Вокальний гурт: {base.GetDescription()}, учасників: {MemberCount}, жанр: {Genre}";
    }

    // Метод
    public bool IsLargeGroup()
    {
        return MemberCount >= 5;
    }
}
