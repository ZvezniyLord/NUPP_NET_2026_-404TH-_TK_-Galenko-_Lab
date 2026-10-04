using MusicContest.Common.Models;

namespace MusicContest.Common.Extensions;

// Статичний клас містить метод розширення.
// Extension method = метод розширення: додає зручний метод до вже існуючого типу,
// не змінюючи код самого класу ContestParticipant.
public static class ContestParticipantExtensions
{
    // Ключове слово this перед ContestParticipant робить метод методом розширення.
    // Завдяки цьому замість ContestParticipantExtensions.ToShortInfo(found)
    // можна писати природніше: found.ToShortInfo().
    public static string ToShortInfo(this ContestParticipant participant)
    {
        return $"{participant.StageName} ({participant.Country})";
    }
}
