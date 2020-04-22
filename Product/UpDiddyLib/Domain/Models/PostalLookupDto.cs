using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class PostalLookupDto
    {
        public Guid CityGuid { get; set; }
        public string CityName { get; set; }
        public Guid StateGuid { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public Guid PostalGuid { get; set; }
        public string PostalCode { get; set; }
    }
}