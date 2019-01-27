using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Contact : BaseModel
    {
        public int ContactId { get; set; }
        [Required]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string LinkIdentifier { get; set; }
        public Guid? ContactGuid { get; set; }
        public int? SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
    }
}
