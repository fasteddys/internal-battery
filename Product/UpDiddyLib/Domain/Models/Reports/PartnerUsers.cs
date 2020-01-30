using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.Reports
{
    public class PartnerUsers
    {
        public Guid PartnerGuid { get; set; }
        public string PartnerName { get; set; }
        public int UsersCreated { get; set; }
        public int EnrollmentsCreated { get; set; }    
    }
}
