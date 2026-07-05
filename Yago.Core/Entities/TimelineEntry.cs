using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class TimelineEntry
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Year { get; set; } = string.Empty;    

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]        
        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
