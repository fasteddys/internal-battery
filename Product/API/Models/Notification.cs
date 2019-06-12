using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Notification : BaseModel
    {
        public int NotificationId { get; set; }
        public Guid NotificationGuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsTargeted { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
