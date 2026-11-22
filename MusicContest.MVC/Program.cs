using Microsoft.EntityFrameworkCore;
using MusicContest.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped(_ => new MusicContestContext(MusicContestDatabase.Options()));

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicContestContext>();
    await db.Database.MigrateAsync();
}
app.Run();
