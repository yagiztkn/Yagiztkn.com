using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Yago.Core.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty; 

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;  

        public int DisplayOrder { get; set; }
    }
}