using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobPage : BaseModel
    {
        public int JobPageId { get; set; }
        public Guid JobPageGuid { get; set; }
        [Required]
        public Uri Uri { get; set; }
        public int JobPageStatusId { get; set; }
        public virtual JobPageStatus JobPageStatus { get; set; }
        public int? JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; }
        public int JobSiteId { get; set; }
        public virtual JobSite JobSite { get; set; }
        [Required]
        public string UniqueIdentifier { get; set; }
        [Required]
        public string RawData { get; set; }
    }
}
