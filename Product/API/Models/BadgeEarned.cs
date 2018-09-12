using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class BadgeEarned : BaseModel
    {
        public int BadgeEarnedId { get; set; }
        public Guid? BadgeEarnedGuid { get; set; }
        [Required]
        public string BadgeId { get; set; }
        [Required]
        public string SubscriberId { get; set; }
        public DateTime DateEarned { get; set; }
        public int PointValue { get; set; }
    }
}
