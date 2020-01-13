using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class NotificationDto : BaseDto
    {
        public Guid NotificationGuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsTargeted { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int HasRead { get; set; }
    }
}
