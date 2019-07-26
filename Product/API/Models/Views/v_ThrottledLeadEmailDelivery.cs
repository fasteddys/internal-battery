using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    [NotMapped]
    public class v_ThrottledLeadEmailDelivery
    {
        public int PartnerContactId { get; set; }
        public int CampaignId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TinyId { get; set; }
        public bool IsUseSeedEmails { get; set; }
        public string EmailSubAccountId { get; set; }
        public string EmailTemplateId { get; set; }
        public int? UnsubscribeGroupId { get; set; }
    }
}
