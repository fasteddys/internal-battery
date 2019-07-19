using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_UnreadNotifications
    {
        public Guid SubscriberGuid { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public int TotalUnread { get; set; }
    }
}
