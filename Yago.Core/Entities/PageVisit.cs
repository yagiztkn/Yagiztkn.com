using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class PageVisit
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
    }
}
