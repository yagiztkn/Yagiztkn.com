using System;
using System.ComponentModel.DataAnnotations;

namespace Yago.Core.Entities
{
    public class ContactMessage
    {
        public int ID { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(5000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime SendDate { get; set; } = DateTime.Now;
    }
}