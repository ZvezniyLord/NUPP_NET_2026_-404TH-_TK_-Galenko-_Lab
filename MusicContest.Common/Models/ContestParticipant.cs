using System.Text.Json.Serialization;

namespace MusicContest.Common.Models;

// Ці атрибути потрібні для поліморфної JSON-серіалізації.
// Вони повідомляють System.Text.Json, що змінна типу ContestParticipant
// насправді може містити SoloSinger або VocalGroup.
// У JSON додається службове поле "$type", за яким тип відновлюється під час Load().
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SoloSinger), "solo")]
[JsonDerivedType(typeof(VocalGroup), "group")]
// abstract class = абстрактний клас.
// ContestParticipant = учасник конкурсу.
// Це спільна база для різних типів учасників.
// Створити new ContestParticipant() напряму не можна — створюємо конкретного нащадка.
public abstract class ContestParticipant : IEntity
{
    // Статичне поле / властивість.
    // static означає: значення належить самому класу, а не окремому об'єкту.
    // Тобто один CreatedCount спільний для всіх SoloSinger і VocalGroup.
    // private set: читати можна ззовні, змінювати — лише всередині цього класу.
    public static int CreatedCount { get; private set; }

    // Статичний конструктор.
    // Виконується автоматично лише один раз перед першим використанням класу.
    // Його не викликають через new.
    static ContestParticipant()
    {
        CreatedCount = 0;
    }

    // Властивості базового класу:
    // Id = Identifier = унікальний ідентифікатор;
    // StageName = сценічне ім'я;
    // Country = країна;
    // Age = вік.
    public Guid Id { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Age { get; set; }

    // Конструктор без параметрів.
    // protected означає, що він доступний самому класу і класам-нащадкам,
    // але його не можна викликати напряму з Program.cs.
    protected ContestParticipant()
    {
        // Guid.NewGuid() автоматично генерує новий унікальний Id.
        Id = Guid.NewGuid();

        // ++ збільшує лічильник створених об'єктів на 1.
        CreatedCount++;
    }

    // Конструктор з параметрами.
    // : this() спочатку викликає конструктор без параметрів,
    // тому створення Id і збільшення лічильника не дублюються.
    protected ContestParticipant(string stageName, string country, int age) : this()
    {
        StageName = stageName;
        Country = country;
        Age = age;
    }

    // Звичайний метод об'єкта.
    // virtual означає, що клас-нащадок може перевизначити цей метод через override.
    public virtual string GetDescription()
    {
        return $"{StageName}, {Country}, {Age} років";
    }

    // Статичний метод належить класу, а не конкретному об'єкту.
    // Виклик: ContestParticipant.GetCreatedCount().
    public static int GetCreatedCount()
    {
        return CreatedCount;
    }
}
