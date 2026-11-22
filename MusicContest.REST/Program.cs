using Microsoft.EntityFrameworkCore;
using MusicContest.Common.Services;
using MusicContest.Infrastructure.Data;
using MusicContest.Infrastructure.Models;
using MusicContest.Infrastructure.Repositories;
using MusicContest.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Контекст SQLite має життєвий цикл scoped для кожного HTTP-запиту.
builder.Services.AddScoped(sp => new MusicContestContext(MusicContestDatabase.Options()));
builder.Services.AddScoped<IRepository<SongModel>, EfRepository<SongModel>>();
builder.Services.AddScoped<IRepository<PerformanceModel>, EfRepository<PerformanceModel>>();
builder.Services.AddScoped<ICrudServiceAsync<SongModel>, RepositoryCrudServiceAsync<SongModel>>();
builder.Services.AddScoped<ICrudServiceAsync<PerformanceModel>, RepositoryCrudServiceAsync<PerformanceModel>>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicContestContext>();
    await db.Database.MigrateAsync();
}
app.Run();

public partial class Program { }
