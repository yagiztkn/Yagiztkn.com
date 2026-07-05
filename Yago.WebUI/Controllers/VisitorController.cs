using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yago.DataAcsess.Context;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = today.AddDays(-6);

            // Toplam ziyaret
            var totalVisits = _context.PageVisits.Count();

            // Bugünkü ziyaret
            var todayVisits = _context.PageVisits
                .Count(v => v.VisitedAt.Date == today);

            // Bu haftaki günlük ziyaretler (grafik için)
            var dailyVisits = _context.PageVisits
                .Where(v => v.VisitedAt.Date >= weekAgo)
                .GroupBy(v => v.VisitedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList()
                .Select(x => new
                {
                    date = x.Date.ToString("dd MMM"),
                    count = x.Count
                });

            // En çok ziyaret edilen sayfalar
            var topPages = _context.PageVisits
                .GroupBy(v => v.Path)
                .Select(g => new { Path = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Benzersiz IP sayısı (yaklaşık tekil ziyaretçi)
            var uniqueVisitors = _context.PageVisits
                .Select(v => v.IpAddress)
                .Distinct()
                .Count();

            return Ok(new
            {
                totalVisits,
                todayVisits,
                uniqueVisitors,
                dailyVisits,
                topPages
            });
        }

        [HttpDelete("clear")]
        public IActionResult ClearOldLogs()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var old = _context.PageVisits.Where(v => v.VisitedAt < cutoff);
            _context.PageVisits.RemoveRange(old);
            _context.SaveChanges();
            return Ok(new { message = "30 günden eski loglar temizlendi." });
        }
    }
}