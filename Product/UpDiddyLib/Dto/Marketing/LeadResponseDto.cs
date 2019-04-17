using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadResponseDto
    {
        public Guid? LeadIdentifier { get; set; }
        public bool IsBillable { get; set; }
        public List<LeadStatusDto> LeadStatuses { get; set; }
        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
