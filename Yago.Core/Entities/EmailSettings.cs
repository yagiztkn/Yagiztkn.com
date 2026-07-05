using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; }
        public string AppPassword { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
    }
}
