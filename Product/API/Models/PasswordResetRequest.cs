using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PasswordResetRequest : BaseModel
    {
        public int PasswordResetRequestId { get; set; }
        public Guid PasswordResetRequestGuid { get; set; }
        public DateTime? ResetDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        [Required]
        public int RedemptionStatusId { get; set; }
        public virtual RedemptionStatus RedemptionStatus { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
    }
}
