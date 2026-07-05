using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class SiteSettings
    {
        public int Id { get; set; }
        public bool IsMaintenanceMode { get; set; }
        public string MaintenanceMessage { get; set; } = "Sitemiz şu an bakım modunda. Kısa süre içinde geri döneceğiz.";
        public string? CvUrl { get; set; }
    }
}
