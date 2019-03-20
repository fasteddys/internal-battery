using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Partner : BaseModel
    {
        public int PartnerId { get; set; }
        public Guid? PartnerGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public List<PartnerReferrer> Referrers { get; set; }
    }
}