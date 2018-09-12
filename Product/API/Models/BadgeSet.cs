using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class BadgeSet : BaseModel
    {
        public int BadgeSetId { get; set; }
        public Guid? BadgeSetGuid { get; set; }
        public int BadgeId { get; set; }
        [Required]
        public string BadgeSetName { get; set; }
        public string BadgeSetDescription { get; set; }
        public bool? Hidden { get; set; }
        public int? SortOrder { get; set; }
        public string Icon { get; set; }
        public string Slug { get; set; }

    }
}
