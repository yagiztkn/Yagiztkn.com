using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
