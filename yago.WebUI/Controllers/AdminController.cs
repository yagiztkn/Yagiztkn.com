using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Yago.Core.Entities;
using Yago.DataAcsess.Context;

namespace Yago.WebUI.Controllers
{
    // Brute force takibi için static dictionary
    public static class LoginAttemptTracker
    {
        private static readonly Dictionary<string, (int Count, DateTime LockUntil)> _attempts = new();
        private static readonly object _lock = new();

        public static bool IsLocked(string ip)
        {
            lock (_lock)
            {
                if (_attempts.TryGetValue(ip, out var entry))
                {
                    if (entry.LockUntil > DateTime.UtcNow && entry.Count >= 5)
                        return true;
                    if (entry.LockUntil <= DateTime.UtcNow)
                        _attempts.Remove(ip);
                }
                return false;
            }
        }

        public static void RecordFailure(string ip)
        {
            lock (_lock)
            {
                if (_attempts.TryGetValue(ip, out var entry))
                {
                    var newCount = entry.Count + 1;
                    var lockUntil = newCount >= 5
                        ? DateTime.UtcNow.AddMinutes(15)
                        : entry.LockUntil;
                    _attempts[ip] = (newCount, lockUntil);
                }
                else
                {
                    _attempts[ip] = (1, DateTime.UtcNow.AddMinutes(1));
                }
            }
        }

        public static void RecordSuccess(string ip)
        {
            lock (_lock) { _attempts.Remove(ip); }
        }
    }

    [ApiController]
    [Route("api/[controller]")]   
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminController : ControllerBase  
    {
        private readonly AppDbContext _context;
        private readonly IOptions<EmailSettings> _emailSettings;

        public AdminController(AppDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (LoginAttemptTracker.IsLocked(ip))
            {
                return StatusCode(429, new
                {
                    message = "Çok fazla başarısız deneme. Lütfen 15 dakika sonra tekrar dene."
                });
            }

            var admin = _context.Admins.FirstOrDefault(a => a.Username == request.Username);

            if (admin != null && admin.PasswordHash == HashPassword(request.Password))
            {
                LoginAttemptTracker.RecordSuccess(ip);

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, request.Username) };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Ok(new { username = request.Username });
            }

            LoginAttemptTracker.RecordFailure(ip);
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı!" });
        }

        [HttpGet("me")]   // GET /api/Admin/me
        public IActionResult Me()
        {
            var username = User.Identity?.Name;
            if (username == null) return Unauthorized();
            return Ok(new { username });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("messages")]
        public IActionResult GetMessages([FromQuery] string searchTerm = "")
        {
            var allMessages = _context.ContactMessages.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                allMessages = allMessages.Where(m =>
                    m.FullName.Contains(searchTerm) ||
                    m.Email.Contains(searchTerm) ||
                    m.Subject.Contains(searchTerm) ||
                    m.Message.Contains(searchTerm));
            }

            var messages = allMessages.OrderByDescending(m => m.SendDate).ToList();

            // DEĞİŞTİ: ViewBag yerine tek bir JSON nesnesi içinde dönüyoruz
            return Ok(new
            {
                stats = new
                {
                    totalMessages = _context.ContactMessages.Count(),
                    unreadMessages = _context.ContactMessages.Count(m => !m.IsRead),
                    todayMessages = _context.ContactMessages.Count(m => m.SendDate.Date == DateTime.Now.Date),
                    totalProjects = _context.Projects.Count()
                },
                messages
            });
        }

        [HttpPost("messages/{id}/reply")]
        public IActionResult ReplyMessage(int id, [FromBody] ReplyMessageRequest request)
        {
            var message = _context.ContactMessages.Find(id);
            if (message == null || string.IsNullOrEmpty(request.ReplyContent))
                return NotFound();

            try
            {
                var settings = _emailSettings.Value;
                var smtpClient = new SmtpClient(settings.SmtpServer)
                {
                    Port = settings.SmtpPort,
                    Credentials = new NetworkCredential(settings.SenderEmail, settings.AppPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.SenderEmail, "Yağız Tekin"),
                    Subject = "RE: " + message.Subject,
                    Body = $"Merhaba {message.FullName},\n\n{request.ReplyContent}\n\n---\nSaygılarımla,\nYağız Tekin",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(message.Email);
                smtpClient.Send(mailMessage);

                message.IsRead = true;
                _context.SaveChanges();

                return Ok(new { message = "E-posta başarıyla gönderildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "E-posta gönderilemedi: " + ex.Message });
            }
        }

        [HttpPost("messages/{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var message = _context.ContactMessages.Find(id);
            if (message == null) return NotFound();

            message.IsRead = true;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("messages/{id}")]
        public IActionResult DeleteMessage(int id)
        {
            var message = _context.ContactMessages.Find(id);
            if (message == null) return NotFound();

            _context.ContactMessages.Remove(message);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var admin = _context.Admins.FirstOrDefault();
            if (admin == null)
                return NotFound(new { message = "Sistemde kayıtlı yönetici bulunamadı!" });

            if (admin.PasswordHash != HashPassword(request.CurrentPassword))
                return BadRequest(new { message = "Mevcut şifrenizi yanlış girdiniz!" });

            if (!IsPasswordSecure(request.NewPassword, out string errorMessage))
                return BadRequest(new { message = errorMessage });

            admin.PasswordHash = HashPassword(request.NewPassword);
            _context.SaveChanges();

            return Ok(new { message = "Şifre güncellendi, lütfen yeniden giriş yapın." });
        }

        // ===== HELPER METODLAR (değişmedi) =====

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        private bool IsPasswordSecure(string password, out string errorMessage)
        {
            errorMessage = "";
            if (password.Length < 6) { errorMessage = "Şifre en az 6 karakter olmalıdır."; return false; }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUpper) { errorMessage = "Şifre en az bir büyük harf içermelidir."; return false; }
            if (!hasLower) { errorMessage = "Şifre en az bir küçük harf içermelidir."; return false; }
            if (!hasDigit) { errorMessage = "Şifre en az bir rakam içermelidir."; return false; }
            if (!hasSpecial) { errorMessage = "Şifre en az bir özel karakter içermelidir."; return false; }

            return true;
        }
    }

    // YENİ: JSON body'den veri almak için küçük request modelleri
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ReplyMessageRequest
    {
        public string ReplyContent { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}