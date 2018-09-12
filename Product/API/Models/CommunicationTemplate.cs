using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CommunicationTemplate : BaseModel
    {
        public int CommunicationTemplateId { get; set; }
        public Guid? CommunicationTemplateGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public int CommunicationTypeId { get; set; }
        public string TextTemplate { get; set; }
        public string HtmlTemplate { get; set; }
    }
}
