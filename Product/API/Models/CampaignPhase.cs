using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CampaignPhase : BaseModel
    {
        public int CampaignPhaseId { get; set; }
        [Required]
        public Guid CampaignPhaseGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }

    }
}
