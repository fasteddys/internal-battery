using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Badge : BaseModel
    {
        public int BadgeId { get; set; }
        public Guid? BadgeGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Icon { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Points { get; set; }
        public string Description { get; set; }
        public bool? Hidden { get; set; }
        public string SortOrder { get; set; }
    }
}
