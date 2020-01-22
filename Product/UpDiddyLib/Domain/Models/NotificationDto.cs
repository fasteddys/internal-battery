using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class NotificationListDto
    {
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
        public int TotalRecords { get; set; }
    }
    
    public class NotificationDto
    {
        public Guid NotificationGuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsTargeted { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int HasRead { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }

        public DateTime? SentDate { get; set; } 
    }
}
