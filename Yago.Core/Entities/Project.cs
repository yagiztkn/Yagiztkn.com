using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.Core.Entities
{
    public class Project
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string GitHubLink { get; set; }
        public string LiveLink { get; set; }
        public string Technologies { get; set; }
        public DateTime CreatedDate { get; set; } 
    }
}
