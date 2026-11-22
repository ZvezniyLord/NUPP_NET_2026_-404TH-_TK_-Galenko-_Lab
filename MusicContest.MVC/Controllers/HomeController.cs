using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicContest.Infrastructure.Data;

namespace MusicContest.MVC.Controllers;

public sealed class HomeController(MusicContestContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Songs = await db.Songs.AsNoTracking().OrderBy(s => s.Title).ToListAsync();
        ViewBag.Performances = await db.Performances.AsNoTracking()
            .Include(p => p.Song)
            .Include(p => p.Participant)
            .OrderBy(p => p.DatabaseId)
            .ToListAsync();
        return View();
    }
}
