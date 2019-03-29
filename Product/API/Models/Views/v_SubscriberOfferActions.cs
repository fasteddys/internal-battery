using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_SubscriberOfferActions
    {
        public Guid SubscriberActionGuid { get; set; }
        public DateTime OccurredDate { get; set; }
        public Guid SubscriberGuid { get; set; }
        public string SubscriberEmail { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }
        public string Action { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }
    }
}
