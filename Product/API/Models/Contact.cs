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
        [Obsolete("This property is being moved to PartnerContact and will be removed from the model very soon.", false)]
        public string FirstName { get; set; }
        [Obsolete("This property is being moved to PartnerContact and will be removed from the model very soon.", false)]
        public string LastName { get; set; }
        [Required]
        public Guid ContactGuid { get; set; }
        public int? SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }

        [Obsolete("This supporting values for this property is being moved to PartnerContact and will be removed from the model very soon.", false)]
        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
    }
}
