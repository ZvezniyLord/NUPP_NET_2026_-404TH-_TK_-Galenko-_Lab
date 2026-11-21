using Microsoft.EntityFrameworkCore;
using MusicContest.Common.Data;
using MusicContest.Infrastructure.Models;

namespace MusicContest.Infrastructure.Data;

/// <summary>
/// Демонстраційне наповнення: лише українські імена та український репертуар.
/// Запускається після міграцій, не стирає власні записи користувача.
/// Навчальні імена конкурсантів вигадані.
/// </summary>
public static class UkrainianDemoSeeder
{
    public static async Task<bool> EnsureAsync(MusicContestContext db)
    {
        if (await db.Participants.AnyAsync())
        {
            // Сумісність із попередньою версією ЛР3, в якій був англомовний seed.
            var people = await db.Participants.Include(x => x.Profile).ToListAsync();
            var tracks = await db.Songs.ToListAsync();
            bool changed = false;
            foreach (var person in people)
            {
                if (person.StageName == "Astra")
                {
                    person.StageName = UkrainianMusicData.Singers[0].StageName;
                    if (person.Profile is not null)
                    {
                        person.Profile.Biography = "Сольна учасниця українського вокального конкурсу";
                        person.Profile.ContactEmail = "olena.kovalchuk@example.test";
                    }
                    changed = true;
                }
                else if (person.StageName == "Northern Lights")
                {
                    person.StageName = UkrainianMusicData.Groups[0].StageName;
                    if (person.Profile is not null)
                    {
                        person.Profile.Biography = "Український вокальний квартет";
                        person.Profile.ContactEmail = "dniprovoices@example.test";
                    }
                    changed = true;
                }
            }
            foreach (var song in tracks)
            {
                int idx = song.Title == "Gravity" ? 0 : song.Title == "Skyfall" ? 1 : -1;
                if (idx < 0) continue;
                var sample = UkrainianMusicData.Songs[idx];
                song.Title = sample.Title;
                song.OriginalArtist = sample.OriginalArtist;
                song.DurationSeconds = sample.DurationSeconds;
                song.Key = sample.Key;
                changed = true;
            }
            if (changed) await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            return false;
        }

        var songs = UkrainianMusicData.Songs.Select(sample => new SongModel
        {
            Title = sample.Title,
            OriginalArtist = sample.OriginalArtist,
            DurationSeconds = sample.DurationSeconds,
            Key = sample.Key
        }).ToList();
        db.Songs.AddRange(songs);

        var participants = new List<ContestParticipantModel>();
        for (int i = 0; i < UkrainianMusicData.Singers.Length; i++)
        {
            var seed = UkrainianMusicData.Singers[i];
            participants.Add(new SoloSingerModel
            {
                StageName = seed.StageName, Country = "Україна", Age = seed.Age,
                VoiceType = seed.VoiceType, VocalRange = seed.VocalRange,
                YearsOfExperience = seed.YearsOfExperience,
                Profile = new ParticipantProfileModel
                {
                    Biography = "Сольний учасник українського вокального конкурсу",
                    ContactEmail = $"solo{i+1}@example.test"
                }
            });
        }
        for (int i = 0; i < UkrainianMusicData.Groups.Length; i++)
        {
            var seed = UkrainianMusicData.Groups[i];
            participants.Add(new VocalGroupModel
            {
                StageName = seed.StageName, Country = "Україна", Age = seed.Age,
                MemberCount = seed.MemberCount, Genre = seed.Genre,
                HasBackingVocals = seed.HasBackingVocals,
                Profile = new ParticipantProfileModel
                {
                    Biography = "Український навчальний вокальний колектив",
                    ContactEmail = $"ensemble{i+1}@example.test"
                }
            });
        }

        db.Participants.AddRange(participants);
        await db.SaveChangesAsync();

        for (int i = 0; i < participants.Count; i++)
        {
            var first = songs[i % songs.Count];
            var second = songs[(i + 3) % songs.Count];
            participants[i].Songs.Add(first);
            participants[i].Songs.Add(second);
            db.Performances.Add(new PerformanceModel
            {
                Participant = participants[i], Song = first,
                Score = 82.0 + i, IsCompleted = true
            });
            db.Performances.Add(new PerformanceModel
            {
                Participant = participants[i], Song = second,
                Score = 85.0 + i, IsCompleted = true
            });
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return true;
    }
}
