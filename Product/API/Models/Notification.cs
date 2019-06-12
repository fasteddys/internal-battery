using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Notification : BaseModel
    {
        public int NotificationId { get; set; }
        public Guid NotificationGuid { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public int IsTargeted { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
