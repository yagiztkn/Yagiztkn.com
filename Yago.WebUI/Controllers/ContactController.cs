using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Yago.Core.Entities;
using Yago.DataAcsess.Context;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOptions<EmailSettings> _emailSettings;

        public ContactController(AppDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] ContactMessageRequest request)
        {
            var contactMessage = new ContactMessage
            {
                FullName = request.FullName,
                Email = request.Email,
                Subject = request.Subject,
                Message = request.Message,
                SendDate = DateTime.Now,
                IsRead = false
            };

            _context.ContactMessages.Add(contactMessage);
            _context.SaveChanges();

            // Bildirim maili gönder (arka planda, hata olursa kullanıcıyı etkilemesin)
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
                    From = new MailAddress(settings.SenderEmail, "yagiz.tkn Site Bildirimi"),
                    Subject = $"📬 Yeni Mesaj: {request.Subject}",
                    Body = $@"Merhaba Yağız,

Sitenden yeni bir mesaj aldın!

━━━━━━━━━━━━━━━━━━━━━━━
Gönderen : {request.FullName}
E-posta  : {request.Email}
Konu     : {request.Subject}
━━━━━━━━━━━━━━━━━━━━━━━

Mesaj:
{request.Message}

━━━━━━━━━━━━━━━━━━━━━━━
Admin panelinden yanıtlamak için: https://yagiztkn.com/admin/mesajlar
",
                    IsBodyHtml = false,
                };

                // Bildirimi kendi mail adresine gönder
                mailMessage.To.Add(settings.SenderEmail);
                smtpClient.Send(mailMessage);
            }
            catch
            {
                // Mail gönderilemese bile kullanıcıya hata döndürme
                // Mesaj zaten veritabanına kaydedildi
            }

            return Ok(new { message = "Mesajınız başarıyla gönderildi." });
        }
    }

    public class ContactMessageRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}