using MusicContest.Common.Models;

namespace MusicContest.Common.Extensions;

// Метод розширення додає операцію до ContestParticipant без зміни самого класу.
public static class ContestParticipantExtensions
{
    // Параметр this передає екземпляр типу, для якого оголошено розширення.
    // Тому метод можна викликати як participant.ToShortInfo().
    public static string ToShortInfo(this ContestParticipant participant)
    {
        return $"{participant.StageName} ({participant.Country})";
    }
}
