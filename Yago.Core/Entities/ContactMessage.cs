using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class ContactMessage
    {

        public int ID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SendDate { get; set; } = DateTime.Now;
    }
}
