using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobReferral: BaseModel
    {
        public int JobReferralId { get; set; }
        public Guid JobReferralGuid { get; set; }
        public int JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; }
        public int ReferralId { get; set; }
        public int? RefereeId { get; set; }
        public string RefereeEmailId { get; set; }
        public bool IsJobViewed { get; set; }
    }
}
