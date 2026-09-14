using MusicContest.Common.Models;

namespace MusicContest.Common.Extensions;

public static class ContestParticipantExtensions
{
    // Метод розширення
    public static string ToShortInfo(this ContestParticipant participant)
    {
        return $"{participant.StageName} ({participant.Country})";
    }
}
