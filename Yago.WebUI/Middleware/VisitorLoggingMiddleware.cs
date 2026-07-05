using Yago.DataAcsess.Context;
using Yago.Core.Entities;

namespace Yago.WebUI.Middleware
{
    public class VisitorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        // Admin ve API isteklerini loglama
        private static readonly string[] _ignoredPrefixes = new[]
        {
            "/api/", "/swagger", "/favicon", "/_", "/cv/"
        };

        public VisitorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            var path = context.Request.Path.Value ?? "/";

            // Sadece GET isteklerini ve belirli path'leri logla
            var shouldLog =
                context.Request.Method == "GET" &&
                !_ignoredPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (shouldLog)
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var referrer = context.Request.Headers["Referer"].ToString();

                // Bot trafiğini filtrele
                var isBot = !string.IsNullOrEmpty(userAgent) && (
                    userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
                    userAgent.Contains("crawler", StringComparison.OrdinalIgnoreCase) ||
                    userAgent.Contains("spider", StringComparison.OrdinalIgnoreCase));

                if (!isBot)
                {
                    var visit = new PageVisit
                    {
                        Path = path,
                        IpAddress = ip,
                        UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent,
                        Referrer = string.IsNullOrEmpty(referrer) ? null : referrer,
                        VisitedAt = DateTime.UtcNow,
                    };

                    db.PageVisits.Add(visit);
                    await db.SaveChangesAsync();
                }
            }

            await _next(context);
        }
    }
}