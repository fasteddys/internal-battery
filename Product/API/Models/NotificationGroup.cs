using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class NotificationGroup : BaseModel
    {
        public int NotificationGroupId { get; set; }
        public Guid  NotificationGroupGuid { get; set; }

        public int NotificationId { get; set; }
        public virtual Notification Notification { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

    }
}
