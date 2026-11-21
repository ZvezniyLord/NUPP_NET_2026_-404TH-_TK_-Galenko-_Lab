using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MusicContest.Infrastructure.Models;

namespace MusicContest.Infrastructure.Data;

public sealed class MusicContestContext(DbContextOptions<MusicContestContext> options) : DbContext(options)
{
    public DbSet<ContestParticipantModel> Participants => Set<ContestParticipantModel>();
    public DbSet<SoloSingerModel> SoloSingers => Set<SoloSingerModel>();
    public DbSet<VocalGroupModel> VocalGroups => Set<VocalGroupModel>();
    public DbSet<ParticipantProfileModel> ParticipantProfiles => Set<ParticipantProfileModel>();
    public DbSet<SongModel> Songs => Set<SongModel>();
    public DbSet<PerformanceModel> Performances => Set<PerformanceModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPT (table-per-type): спільна таблиця + окрема таблиця на тип-нащадок.
        modelBuilder.Entity<ContestParticipantModel>(entity =>
        {
            entity.UseTptMappingStrategy();
            entity.ToTable("ContestParticipants");
            entity.HasKey(x => x.DatabaseId);
            entity.Property(x => x.DatabaseId).ValueGeneratedOnAdd();
            entity.HasIndex(x => x.Id).IsUnique();
            entity.Property(x => x.StageName).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Country).IsRequired().HasMaxLength(100);
            entity.HasMany(x => x.Performances)
                  .WithOne(x => x.Participant)
                  .HasForeignKey(x => x.ParticipantDatabaseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Songs)
                  .WithMany(x => x.Participants)
                  .UsingEntity<Dictionary<string, object>>(
                      "ParticipantSongs",
                      right => right.HasOne<SongModel>().WithMany()
                          .HasForeignKey("SongDatabaseId").OnDelete(DeleteBehavior.Cascade),
                      left => left.HasOne<ContestParticipantModel>().WithMany()
                          .HasForeignKey("ParticipantDatabaseId").OnDelete(DeleteBehavior.Cascade),
                      join =>
                      {
                          join.ToTable("ParticipantSongs");
                          join.HasKey("ParticipantDatabaseId", "SongDatabaseId");
                      });
        });

        modelBuilder.Entity<SoloSingerModel>(entity =>
        {
            entity.ToTable("SoloSingers");
            entity.Property(x => x.VoiceType).HasMaxLength(80);
            entity.Property(x => x.VocalRange).HasMaxLength(80);
        });
        modelBuilder.Entity<VocalGroupModel>(entity =>
        {
            entity.ToTable("VocalGroups");
            entity.Property(x => x.Genre).HasMaxLength(80);
        });

        modelBuilder.Entity<ParticipantProfileModel>(entity =>
        {
            entity.ToTable("ParticipantProfiles");
            entity.HasKey(x => x.DatabaseId);
            entity.HasIndex(x => x.Id).IsUnique();
            entity.Property(x => x.Biography).HasMaxLength(1000);
            entity.Property(x => x.ContactEmail).HasMaxLength(160);
            entity.HasOne(x => x.Participant)
                  .WithOne(x => x.Profile)
                  .HasForeignKey<ParticipantProfileModel>(x => x.ParticipantDatabaseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongModel>(entity =>
        {
            entity.ToTable("Songs");
            entity.HasKey(x => x.DatabaseId);
            entity.HasIndex(x => x.Id).IsUnique();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(160);
            entity.Property(x => x.OriginalArtist).HasMaxLength(160);
            entity.Property(x => x.Key).HasMaxLength(40);
            entity.HasMany(x => x.Performances)
                  .WithOne(x => x.Song)
                  .HasForeignKey(x => x.SongDatabaseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PerformanceModel>(entity =>
        {
            entity.ToTable("Performances");
            entity.HasKey(x => x.DatabaseId);
            entity.HasIndex(x => x.Id).IsUnique();
            entity.Property(x => x.Score).HasColumnType("REAL");
        });
    }
}

// Один і той самий шлях до SQLite для dotnet ef та консольного застосунку.
public static class MusicContestDatabase
{
    public static string PathToDatabase()
    {
        string? dir = Directory.GetCurrentDirectory();
        while (dir is not null && !File.Exists(System.IO.Path.Combine(dir, "MusicContest.sln")))
            dir = Directory.GetParent(dir)?.FullName;
        if (dir is null)
            throw new DirectoryNotFoundException("Не знайдено корінь із MusicContest.sln.");
        string docs = System.IO.Path.Combine(dir, "docs");
        Directory.CreateDirectory(docs);
        return System.IO.Path.Combine(docs, "musiccontest.db");
    }

    public static DbContextOptions<MusicContestContext> Options() =>
        new DbContextOptionsBuilder<MusicContestContext>()
            .UseSqlite($"Data Source={PathToDatabase()}")
            .Options;
}

public sealed class MusicContestContextFactory : IDesignTimeDbContextFactory<MusicContestContext>
{
    public MusicContestContext CreateDbContext(string[] args) => new(MusicContestDatabase.Options());
}
