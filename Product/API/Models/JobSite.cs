using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobSite : BaseModel
    {
        public int JobSiteId { get; set; }
        public Guid JobSiteGuid { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Uri Uri { get; set; }
        public List<JobPage> JobListings { get; set; } = new List<JobPage>();
    }
}
