using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class NotificationCreateDto
    { 
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsTargeted { get; set; }
        public DateTime? ExpirationDate { get; set; } 
        public List<Guid> Groups { get; set; }
    }
}
