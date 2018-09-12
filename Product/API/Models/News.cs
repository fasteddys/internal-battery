using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class News : BaseModel
    {
        public int NewsId { get; set; }
        public Guid? NewsGuid { get; set; }
        [Required]
        public string Headline { get; set; }
        [Required]
        public string SubText { get; set; }
        public DateTime DateActive { get; set; }
        public string Body { get; set; }
        public int? SortOrder { get; set; }
        public string ExternalLink { get; set; }
        public int NewsTypeId { get; set; }
    }
}
