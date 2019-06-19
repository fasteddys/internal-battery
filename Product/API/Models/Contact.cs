using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Contact : BaseModel
    {
        public int ContactId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Guid ContactGuid { get; set; }
        public int? SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public virtual List<PartnerContact> PartnerContacts { get; set; }
    }
}
