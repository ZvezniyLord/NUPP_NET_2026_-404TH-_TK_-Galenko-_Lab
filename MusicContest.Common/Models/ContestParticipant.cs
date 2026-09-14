using System.Text.Json.Serialization;

namespace MusicContest.Common.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SoloSinger), "solo")]
[JsonDerivedType(typeof(VocalGroup), "group")]
public abstract class ContestParticipant : IEntity
{
    // Статичне поле
    public static int CreatedCount { get; private set; }

    // Статичний конструктор
    static ContestParticipant()
    {
        CreatedCount = 0;
    }

    public Guid Id { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Age { get; set; }

    // Конструктор
    protected ContestParticipant()
    {
        Id = Guid.NewGuid();
        CreatedCount++;
    }

    // Конструктор з параметрами
    protected ContestParticipant(string stageName, string country, int age) : this()
    {
        StageName = stageName;
        Country = country;
        Age = age;
    }

    // Метод
    public virtual string GetDescription()
    {
        return $"{StageName}, {Country}, {Age} років";
    }

    // Статичний метод
    public static int GetCreatedCount()
    {
        return CreatedCount;
    }
}
