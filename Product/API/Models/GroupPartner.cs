using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class GroupPartner : BaseModel
    {
        public int GroupPartnerId { get; set; }
        public Guid GroupPartnerGuid { get; set; }
        public virtual Group Group { get; set; }
        public int GroupId { get; set; }
        public virtual Partner Partner { get; set; }
        public int PartnerId { get; set; }
    }
}
