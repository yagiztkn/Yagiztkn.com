using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yago.Core.Entities;
using Yago.DataAcsess.Context;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SiteSettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var settings = _context.SiteSettings.FirstOrDefault();
            if (settings == null)
                return Ok(new { isMaintenanceMode = false, maintenanceMessage = "" });

            return Ok(settings);
        }

        [HttpPut]
        [Authorize]
        public IActionResult Update([FromBody] SiteSettingsRequest request)
        {
            var settings = _context.SiteSettings.FirstOrDefault();
            if (settings == null)
            {
                _context.SiteSettings.Add(new Yago.Core.Entities.SiteSettings
                {
                    IsMaintenanceMode = request.IsMaintenanceMode,
                    MaintenanceMessage = request.MaintenanceMessage,
                    CvUrl = request.CvUrl
                });
            }
            else
            {
                settings.IsMaintenanceMode = request.IsMaintenanceMode;
                settings.MaintenanceMessage = request.MaintenanceMessage;
                settings.CvUrl = request.CvUrl;
                _context.SiteSettings.Update(settings);
            }

            _context.SaveChanges();
            return Ok();
        }


        [HttpPost("upload-cv")]
        [Authorize]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya seçilmedi." });

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cv");
            Directory.CreateDirectory(uploadsPath);

            var fileName = "cv.pdf"; // Her zaman aynı isim, yeni yükleme eskinin üzerine yazar
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var cvUrl = $"/cv/{fileName}?v={DateTime.UtcNow.Ticks}";

            var settings = _context.SiteSettings.FirstOrDefault();
            
            if (settings == null)
            {
                settings = new SiteSettings { CvUrl = cvUrl };
                _context.SiteSettings.Add(settings);
            }
            else
            {
                settings.CvUrl = cvUrl;
                _context.SiteSettings.Update(settings);
            }

            await _context.SaveChangesAsync();

            return Ok(new { cvUrl });
        }

        public class SiteSettingsRequest
        {
            public bool IsMaintenanceMode { get; set; }
            public string MaintenanceMessage { get; set; } = string.Empty;
            public string? CvUrl { get; set; }

        }
    }
}


