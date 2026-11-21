namespace MusicContest.Common.Data;

// Єдине джерело демонстраційних українських даних для ЛР1, ЛР2 і ЛР3.
// Імена учасників та назви конкурсних колективів вигадані (навчальні приклади).
// Відомі українські пісні наведено з їхніми авторами / виконавцями.
// Тривалість і тональність наведено лише для навчальної демонстрації, а не як музикознавчі метадані.
public static class UkrainianMusicData
{
    public sealed record SingerSeed(
        string StageName, int Age, string VoiceType, string VocalRange, int YearsOfExperience);

    public sealed record GroupSeed(
        string StageName, int Age, int MemberCount, string Genre, bool HasBackingVocals);

    public sealed record SongSeed(
        string Title, string OriginalArtist, int DurationSeconds, string Key);

    public static readonly SingerSeed[] Singers =
    {
        new("Олена Ковальчук", 21, "сопрано", "A3–E6", 8),
        new("Андрій Бондаренко", 25, "тенор", "C3–C5", 6),
        new("Марія Шевченко", 22, "альт", "G3–D5", 5),
        new("Тарас Мельник", 28, "баритон", "G2–G4", 9),
    };

    public static readonly GroupSeed[] Groups =
    {
        new("Дніпровські голоси", 24, 4, "народний поп", true),
        new("Карпатська ватра", 23, 5, "фолк", true),
    };

    public static readonly SongSeed[] Songs =
    {
        new("Червона рута", "Володимир Івасюк", 234, "фа мажор"),
        new("Обійми", "Океан Ельзи", 242, "мі мінор"),
        new("Тримай", "Христина Соловій", 213, "ля мінор"),
        new("Щедрик", "Микола Леонтович", 192, "мі мінор"),
        new("Україна", "Тарас Петриненко", 247, "соль мажор"),
        new("Думи", "Артем Пивоваров і Надя Дорофєєва", 219, "ре мінор"),
        new("Плакала", "KAZKA", 223, "сі мінор"),
        new("Несе Галя воду", "Українська народна пісня", 205, "соль мажор"),
    };
}
