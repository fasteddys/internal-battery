using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class EmailTemplateDto
    { 
        public Guid EmailTemplateGuid { get; set; }
        public string SendGridTemplateId { get; set; }
        public string SendGridSubAccount { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }

    public class EmailTemplateListDto
    {
        public List<EmailTemplateDto> EmailTemplates { get; set; } = new List<EmailTemplateDto>();
        public int TotalRecords { get; set; }
    }

}
